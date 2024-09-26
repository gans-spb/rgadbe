/* Extorr Residual Gas Analyzers DataBase Edition
 * base on Extorr ASCII Firmware V0.12 RGA App
 * (C) Ioffe inst. and Extorr, Igor Bocharov
 * Main Form - handlers
 */

//-----------------------------------------------------------------------------
// see csproj WarningLevel=0
global using System;
global using System.Threading.Tasks;
global using System.Collections;
global using System.Collections.Generic;
global using System.Windows.Forms;
global using System.Drawing;

global using Microsoft.EntityFrameworkCore;
global using System.ComponentModel.DataAnnotations;
global using System.ComponentModel.DataAnnotations.Schema;
global using System.Linq;

global using Serilog;

using Serilog.Core;
using Serilog.Events;

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;

//-----------------------------------------------------------------------------
//This app ReadOnly constants
public static class AppConst{
    public static readonly string Ver      = "v1.0b 01.Apr.24";
    
    public static readonly string BootName = "qpbox-Aug23.l2";
    public static readonly string DBName   = "rga.db";
    public static readonly string COMName  = "COM1";
    public static readonly int    COMSpeed = 115200;
    public static readonly int    IPPort   = 3373;
    public static readonly string IPAddr   = "0.0.0.0";
    public static readonly bool Boot = false;
    public static readonly bool Renew = false;

    public static readonly string          LogName = "rgadb";
    public static readonly LogEventLevel   LogLev  = LogEventLevel.Debug;
    public static readonly RollingInterval LogRoll = RollingInterval.Month;
    
    public static readonly int FormWidth   = 1000;
    public static readonly int FormHeight  = 800;
    
    public static readonly int SerialPollRX = 200;   //ms between serial process line (not readline)
    public static readonly int SerialPollTX = 200;   //ms between serial send, at least a 500ms, otherwise device STUCK!
    public static readonly int SafetyTimer  = 15;   //s  between cashe commit to DB, > moniPoll
    public static readonly int PollBoxMin   = 3;    //s time to device request before measurement
    public static readonly int PreRequest   = 3000; //ms delay before main request polling

    public static readonly float FilamentPre = 2.0E-4f; //max pirani pressure for Filament start

    public static readonly string csvDelimeter = " "; //delimeter for export
    public static readonly int trendSize   = 1000;    //samples per Plot window in Trend/Monitor mode
    public static readonly int loGraphP = -15;  //low plot range
    public static readonly int hiGraphP = +3;   //hi plot  range

    public static readonly string clntCmd = "ccmd"; //client command start from
}

//This app RW settings
public static class AppConf{
    public static String? DBName { get; set; }
    public static String? COMPortName { get; set; }
    public static int     IPPortNum { get; set; }
    public static bool    Boot { get; set; }
    public static bool    Renew { get; set; }

    public static Icon?   MainIcon { get; set; }
    public static Image?  AboutPic { get; set; }
    public static Stream? FwFile   { get; set; }
}

//-----------------------------------------------------------------------------
//Callback hanlders 
public delegate void StrEvtCb (String s);

public delegate void StringEventHandler(object sender, StringEventArgs e);

public class StringEventArgs : EventArgs {
        public string StringValue;
        public StringEventArgs(string s) => StringValue = s;
}

public delegate void BoolEventHandler(object sender, BoolEventArgs e);

public class BoolEventArgs : EventArgs {
        public bool BoolValue;
        public BoolEventArgs(bool b) => BoolValue = b;
}

//-----------------------------------------------------------------------------
// Main app, cosole output only as debug
class App
{
    //EF DB context
    public static AppDBContext DBcon = new AppDBContext();
    
    //MultiThread Async Telnet server
    public static TxtServer? TxtSrv;

    //Test console writer
    public static void con(String msg) {Console.WriteLine(msg);} //Test console print

    //DateTime Now without msec
    public static DateTime GetDTNow(){
        DateTime dt = System.DateTime.Now;
        dt = dt.AddTicks( - (dt.Ticks % TimeSpan.TicksPerSecond));
        return dt;
    }

    //Short string message from exception
    public static string StringFromEx (Exception e) {
        return
            e.ToString()[..64]
            .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
    }
    
        public static bool CheckComNameExist(string name){
        string[] ports = System.IO.Ports.SerialPort.GetPortNames();
        return ports.Contains(name);
    }

    public static string GetLastComName(){
        string[] ports = System.IO.Ports.SerialPort.GetPortNames();
        return ports.LastOrDefault(AppConst.COMName);
    }
    
    //String to float with 0.0F if exception
    public static float StrToFloatZero(string sval){
        float fval;
        if(float.TryParse(sval, 
            System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture,
            out fval))
                return fval;
        else    return 0.0F;
    }

    //Log file path hook for serilog
    public static string logPath = "";

    //Log custom Serilog for Form small console
    public static FormLogsSink? formSink;

    public class FormLogsSink: ILogEventSink
    {
        public void Emit(LogEvent logEvent){
            if (EmitMessage != null) EmitMessage(this, new StringEventArgs( logEvent.RenderMessage()));
        }
        public event StringEventHandler? EmitMessage; //! do not forget to set callback
    }

    //Log get Serilog current Log filename
    class CaptureFilePathHook : Serilog.Sinks.File.FileLifecycleHooks{
        public string? Path { get; private set; }

        public override System.IO.Stream OnFileOpened(string path, System.IO.Stream underlyingStream, System.Text.Encoding encoding){
            Path = path; return base.OnFileOpened(path, underlyingStream, encoding);
        }
    }
    
    //Load assembly resourses
    private static bool LoadAssRes(){
        try {
            var assStream = System.Reflection.Assembly.GetExecutingAssembly();
            AppConf.MainIcon = new Icon(assStream.GetManifestResourceStream("rgadbe.ExtorrIoffe.ico")!);
            AppConf.AboutPic = Image.FromStream(assStream.GetManifestResourceStream("rgadbe.IoffeExtor.png")!);
            AppConf.FwFile   = (assStream.GetManifestResourceStream("rgadbe.qpbox.l2"));
            return false;
        }
        catch(Exception e){
            Log.Fatal("Load resource failed! " + App.StringFromEx(e));
            return true;
        }
    }
    

    [STAThreadAttribute]
	public static int Main(string[] args)
    {
        //Log, levels: Verbose - Debug - Information - Warning - Error - Fatal
        formSink = new FormLogsSink();  //log to Form component
        formSink.EmitMessage += new StringEventHandler(delegate (object o, StringEventArgs a){});

        var logFilePathHook = new CaptureFilePathHook();
        Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.Sink(formSink, 
                            restrictedToMinimumLevel: AppConst.LogLev)
                        .WriteTo.File(AppConst.LogName+"-.log",
                            rollingInterval: AppConst.LogRoll,
                            restrictedToMinimumLevel: AppConst.LogLev,
                            outputTemplate: 
                                "{Timestamp:yy.MM.dd HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}",
                            hooks: logFilePathHook
                            )
                        .MinimumLevel.Verbose()
                        .CreateLogger();

        //Args parce
        var rootCommand = new RootCommand("Extorr RGA control sowtware"){
            new Option<string?>("--db",   getDefaultValue: () => AppConst.DBName,
                "Sqlite DB file, rga.db default"),
            new Option<string?>("--com",  getDefaultValue: () => GetLastComName(),
                "COM port to open."),
            new Option<int?>   ("--port", getDefaultValue: () => AppConst.IPPort,
                "IP localhost Port for remote telnet client"),
            new Option<bool?>  ("--boot", getDefaultValue: () => AppConst.Boot,
                "instantly open port and boot firmware"),
            new Option<bool?>  ("--renew", getDefaultValue: () => AppConst.Renew,
                "instantly renew parameters after open port")
        };

        //set app conf params
        rootCommand.Handler = CommandHandler.Create<string, string, int, bool, bool>(
            (db, com, port, boot, renew) =>
                    (AppConf.DBName, AppConf.COMPortName, AppConf.IPPortNum, AppConf.Boot, AppConf.Renew) 
                        = (db, com, port, boot, renew)
        );
        rootCommand.Invoke(args);

        Log.Information("Start " +AppConst.Ver +": " + AppConf.DBName +", "+ AppConf.COMPortName +", "+ AppConf.IPPortNum +
                         ((AppConf.Boot)?", boot":", noboot") + ((AppConf.Renew)?", renew":", norenew"));
        Log.Information("Log path " + logFilePathHook.Path);
        logPath = logFilePathHook.Path!;
            
        
        //App embed resourses
        if (LoadAssRes()) return 1;

        //EF DB connection
        if (DBcon.EnsureCreated(AppConf.DBName!)) return 2;

        //Telnet async server
        TxtSrv = new TxtServer(AppConst.IPAddr, AppConf.IPPortNum);
        Task tntask = TxtSrv.Run();

        try{
            RgaWindow RgaWin = new RgaWindow();
            Application.Run(RgaWin);//new RgaWindow());
            //=> M A I N   B O D Y
            TxtSrv.Stop();
            tntask.Wait();
        }
        catch(Exception e){
            Log.Fatal("App failed! " + App.StringFromEx(e));
            File.WriteAllText("core" + App.GetDTNow().ToString("yy.MM.dd")  + ".dump", e.ToString());
            return 3;
        }

        Log.Information("End --------------- ");
        return 0;
	}
}

//- E N D - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -