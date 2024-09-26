/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Serial port line parcer
 */

using System.IO.Ports;
using System.Threading;

partial class RgaWindow : Form
{
    SerialPort serial;
    byte[] inbuf = new byte[1];

    System.Windows.Forms.Timer timerSerialTX = new System.Windows.Forms.Timer();
    System.Windows.Forms.Timer timerSerialRX = new System.Windows.Forms.Timer();

    void StartRXTimer(){ 
        timerSerialRX.Tick += new System.EventHandler(onSerialTickRX);
        timerSerialRX.Interval = AppConst.SerialPollRX;
        timerSerialRX.Enabled = true;
    }

    bool openSerial(String filename, int baudrate)
    {
        serial = new SerialPort(); // filename, baudrate);
        serial.BaudRate = baudrate;
        serial.DataBits = 8;
        serial.StopBits = StopBits.One;
        serial.Parity = Parity.None;
        serial.PortName = filename;
        serial.ReadBufferSize = 4096 * 8;
        serial.ReadTimeout = 1000;  // 1 sec, throw exeption
        serial.WriteTimeout= 1000;
        serial.ReceivedBytesThreshold = 4096;  // testing
        serial.Encoding = System.Text.Encoding.GetEncoding(28591); // plain 8-bit characters

        timerSerialTX.Tick += new System.EventHandler(onSerialTickTX);
        timerSerialTX.Interval = AppConst.SerialPollTX;  //Poll device no less 500ms!!! Stuck otherwise!
        timerSerialTX.Enabled = false;

        try {
            serial.Open();
        } catch (Exception e) {
            string msg =  App.StringFromEx(e);
            ShowError( ("Unable to open " + filename + "port\n" + msg), "Open error");
            Log.Error("Open failed! " + msg);

            return false;
        }
        timerSerialTX.Enabled = true;
        return true;
    }

    void ShowError(string msg, string cap = "Protocol error")
    {
        MessageBox.Show(this, msg, cap,
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Exclamation
                       );
    }

    //READER ------------
    
    bool scanFor(string cmdline, string fieldname, out int value)
    {
        int i;
        int val = 0;
        string[] Argv = cmdline.Split(':');
        int Argc = Argv.Length;
        value = 0;

        for (i = 0; i < Argc; i++){
            if (Argv[i] == fieldname){
                if ( (i + 1) >= Argc)
                    return false;
                if (!Int32.TryParse(Argv[i + 1], out val))
                    return false;
                value = val;
                return true;
            }
        }
        return false;
    }

    int checksumForIncoming(string cmdline)
    {
        string[] Argv = cmdline.Split(':');
        int Argc = Argv.Length;
        string s = "";

        for (int i = 0; i < Argc; i++){
            if ( Argv[i] == "ck" )
                break;
            if (i != 0)
                s += ':';
            s += Argv[i];
        }
        return checksumFor(s);
    }

    //Messages from all telnet clients to serial
    void TelnetDeque()
    {
        lock (App.TxtSrv.gotAllMsg.SyncRoot) {
            while (App.TxtSrv.gotAllMsg.Count > 0){
                string cmd = (string)App.TxtSrv.gotAllMsg.Dequeue();
                //if (cmd=="") continue; //at session end
                Log.Verbose("<t " + cmd);
                    if (cmd.StartsWith(AppConst.clntCmd))
                        serialQueRX.Enqueue(cmd); //=> serial RX
                    else
                        if (isCommOpened) 
                            serialQueTX.Enqueue(cmd); //=> serial TX
                        else
                            logToConsole("serial port closed for cmd=" + cmd);
            }
        }
    }

    string ccmdHelp(){ 
        string ret="";
        ret += "--Client commands starts from 'ccmd:'\r\n";
        ret += "ccmd: - help, moni, sweep, trend, stop, filament.\r\n";
        ret += "ccmd:set:par:value - meas, poll, tlimit.\r\n";
        ret += "--Extorr commands - see Extorr ASCII Firmware API Mannual\r\n";
        ret += "as example: controls, channel, sweep:count:1, \r\n";
        ret += "set:highmass:123, get:piranitorr\r\n";
        ret += "--Putty to remove init crap at start negotiation:\r\n";
        ret += "Putty Settings->Connection->Telnet->Telnet negotiation mode = Passive\r\n";
        ret += "exit - terminate terminal session\r\n";

        return ret;
    }

    //parce Client`s Commands
    //2do return cmd response and errors
    void processClientCmd(string fromQp){
        //App.con("ccmd="+fromQp);
        string[] fields = fromQp.Split(':');
        if (fields.Length < 2) return;

        if (fields[1]=="help")  logToConsole(ccmdHelp());else
        if (fields[1]=="moni")  StartMoni();  else
        if (fields[1]=="sweep") StartSweep(); else
        if (fields[1]=="trend") StartTrend(); else
        if (fields[1]=="stop")  Stop("telnet"); else
        if (fields[1]=="filament") OnFilament(null,null);
        else
        if (fields[1]=="set"){
             if (fields.Length>3){
                     if (fields[2]=="poll")   pollBox.setValue(fields[3]);      //2DO add to ControlsDict
                else if (fields[2]=="tlimit") limitBox.setValue(fields[3]);
                else if (fields[2]=="meas")   updateDBTab(int.Parse(fields[3]));//2DO response
                  else logToConsole("Unknown ccmd:parameter="+fields[2]);
             }else logToConsole("Unknown value for client command ccmd:"+fields[1]+":"+fields[2]);
        }else logToConsole("Unknown client command ccmd="+fields[1]);
    }

    
    /* This takes a line of input from the QpBox and dispatches to the
     * appropriate handler, based on the command and data.*/

    void processLine(string fromQp)
    {
        //App.con(fromQp); //TEST

        char[] eolChars = { '\n', '\r' };
        fromQp = fromQp.TrimEnd(eolChars);

        string[] fields = fromQp.Split(':');
        int numFields = fields.Length;
        if (numFields < 1) return;
        //Console.WriteLine("     " + fields[0] + fields[1] + fields[2]);
               
        //Silent parce monitoring data end return
        if ( (DBlock.isRun == true) &&  
             (DBlock.measMode == MeasMode.Monitor) &&
             (fields.Length == 3) && 
             (fields[0] == "ok"))
                if (DBlock.onMonitoringLine(fields)){
                    enqueuePlotDataPoint (DBlock.gPoint);
                    textBoxMoniUpdate(DBlock.dataL);
                    if (autoStreamBox.Checked == true) 
                        logToConsole(String.Format("<- {0}", fromQp) );
                    return;
                    }
        
        Log.Verbose("<- " + fromQp);
        if (autoStreamBox.Checked == true)
            logToConsole(String.Format("<- {0}", fromQp) );
        
        int cksum;
        if (scanFor(fromQp, "ck", out cksum) && (cksum != checksumForIncoming(fromQp)) ){
            Log.Error("Serial incoming checksum error: {0}");
        }
        
        string head = fields[0];

        //collect data
        if ( (head == "s10") || (head == "s16") || (head == "s64") && (numFields >= 2))
            DBlock.onSweepLine(fromQp);
        
        if ( (head == "t10") || (head == "t16") || (head == "t64") && (numFields >= 2))
            DBlock.onTrendLine(fromQp);
    

        if ( (head == "cmd") && (numFields >= 2) ){
            Log.Error(fromQp);
            ShowError(fields[1]);
        
        
        } else if (head == AppConst.clntCmd){
            processClientCmd(fromQp);
        
        //parce Sweep data
        } else if ( (head == "s64") && (isSweeping == true)){
            processSweep64Line(fields[1], fields[2]);
        } else if ( (head == "s10") && (isSweeping == true) ){
            processSweep10Line(fromQp);
        } else if ( (head == "s16") && (isSweeping == true) ){
            processSweep16Line(fromQp);
        
        //parce Trend data
        }  else if ( head == "t10" ){
            processTrend10Line(fromQp);
        } else if ( head == "t16"){
            processTrend16Line(fromQp);
        } else if ( head == "t64"){
            processTrend64Line(fields[1], fields[2]);
        
        //parce Trend/Sweep commands
        } else if (head == "BeginStream"){
            setupSweep(fromQp);
        } else if (head == "BeginTrend"){
            setupTrend(fromQp);
        } else if ( head == "EndStream" ){
            terminateSweep();
        } else if (head == "EndTrend"){
            terminateTrend();
        
        //parce parameters data
        } else if ( ((head == "inf") || (head == "ok")) && (numFields >= 3) ){
            updateBoxIntFloat(fields[1], fields[2]);
        //hlam tail
        } else if ( head == "inf") {
            // just ignore informational messages.  Maybe we will log them later.
        } else if (head != "ok" ){
            // ignore unknown commands
            logToConsole ("Parse ignoring: " + fromQp);
            Log.Warning("Parse ignoring: {0}", fromQp);
        }
    }
    
    public Queue<string> serialQueRX = new Queue<string>();
    
    private void onSerialTickRX(object sender, EventArgs e) {
        //if ( isCommOpened && isBooted)
            if (serialQueRX.Count >0 )
                processLine(serialQueRX.Dequeue());
    }
    
    //Stop btn. Form.Close
    bool readStopRequested = false;
    void stopSerialReadThread(){
        readStopRequested = true;
    }

    void doReadLoop()
    {
        //byte[] datab = new byte[64];
        //int bytesRead;
        string datas;

        while (readStopRequested == false){
            try {
                datas = serial.ReadLine();
            } 
            catch (TimeoutException){                
                continue;   //see serial.ReadTimeout=1000; it`s normal way
            } catch {
                Log.Error("Serial.ReadLine error");
                readStopRequested = true;
                continue;
            }
        serialQueRX.Enqueue(datas);
        //processLine(datas);
        }
        timerSerialTX.Enabled = false;
        serial.Close();

        // now we call an eventhandler on the gui thread to
        // update the gui state to reflect the serial closure.

        if (shuttingDown == false){
            EventHandler ev = new EventHandler(onSerialThreadClose);
            BeginInvoke(ev);
        }
    }

    void onSerialThreadClose(object sender, EventArgs e)
    {
        Log.Information("Port close");
        CommButsEnDis(true);
    }

    bool serialReadThread(string port, string speed)
    {
        int baudRate;
        if (Int32.TryParse(speed, out baudRate) == false){
            Console.WriteLine("cannot parse speed: {0}", speed);
        }
        
        if (openSerial(port, baudRate) == false)
            return false;
        
        Thread t = new Thread(new ThreadStart(doReadLoop));
        readStopRequested = false;
        t.Start();
        Log.Information("Port open " + port +"."+ speed);
        return true;
    }

    //SENDER------------------------
    int nextTag = 0;
    bool usingTag = false;      // connected to gui checkbox
    bool usingChecksum = false; // connected to gui checkbox
   
    public Queue<string> serialQueTX = new Queue<string>();
    
    private void onSerialTickTX(object sender, EventArgs e) {
        if ( isCommOpened && isBooted)
            if (serialQueTX.Count >0 )
                sendToQpBoxSilent(serialQueTX.Dequeue());
    }

    void sendToQpBoxSilent(string cmd)
    {
        if ( !isCommOpened && !isBooted) return;

        if (autoStreamBox.Checked == true)
            logToConsole(String.Format("-> {0}", cmd) );
        sendSerial(cmd);
    }

    //2do dont test with tag and checksum
    public void sendToQpBox(string cmd)
    {
        if ( !isCommOpened && !isBooted) return;

        if ((usingTag == true) && (scanFor(cmd, "tag", out int val) == false))
            cmd += String.Format(":tag:{0}", ++nextTag);
        if ( (usingChecksum == true) && (scanFor(cmd, "ck", out val) == false))
            cmd += String.Format(":ck:{0}", checksumFor(cmd) );

        logToConsole(String.Format("-> {0}", cmd) );
        Log.Verbose("-> " + cmd);
        
        sendSerial(cmd);
    }

    void sendSerial(string cmd){
        try{
            serial.Write(cmd);  //=>send to COM
            serial.Write("\n");
        } catch {
            Log.Fatal("Serial write failed!");
            onCommOpenClose( new Button{Text = "Close"},null);
        }
    }

    int checksumFor(string s)
    {
        int cksum = 0;
        foreach (char ch in s)
            cksum += ch;
        return cksum;
    }
}
