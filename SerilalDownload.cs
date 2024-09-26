/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Firmware downloader
 */

using System.IO;
using System.IO.Ports;
using System.Threading;

//Download firmware to device via serial (see FW manual)
class FirmwareDownloader
{
    SerialPort serial;
    Stream fstream;
    int increaseBaudRate;

    public bool bootFail = false;   //if True - than failed
    public string bootFailMsg = ""; //fail message

    public event StringEventHandler EmitMessage;
    public event BoolEventHandler   EmitFinished;

    void outputString(string s){
        if (EmitMessage != null)
            EmitMessage(this, new StringEventArgs(s));
    }
    
    // Cancel btn pressed
    bool cancelRequested;
    public void RequestCancel(){
        try{
            serial.ReadTimeout = 1;
            Thread.Sleep(100);
            serial.Close();
        }
        catch{}
        cancelRequested = true;
    }

    public FirmwareDownloader(String commPort, Stream str, int baudRate){
        fstream = str;
        serial = new SerialPort(commPort, 9600);
        serial.ReadTimeout = 10000;
        this.increaseBaudRate = baudRate;
    }

    //main downloader
    public void Download()
    {
        try {
            serial.BaudRate = 9600;
            serial.Open();
            doDownload();
            Log.Information("Boot ok");
        } 
        catch (Exception e){
            Log.Fatal("Boot failed! " + App.StringFromEx(e));
            bootFail = true;
        }
        serial.Close();  //always close port

        if (EmitFinished != null)                //onDownloadFinished or Canceled
            EmitFinished(this, new BoolEventArgs(bootFail)); //=> ret
    }

    // D W N L D =>
    void doDownload()
    { 
        outputString(String.Format("Boot starting port {0}", serial.PortName));
        
        const int loaderSize = 2560;

        byte[] initChunks = new byte[loaderSize]; // loaderSize = 2560

        fstream.Read(initChunks, 0, loaderSize); // get boot record from qpbox.l2

        //resetQpBox, enough for 1 second at 9600 baud.
        for (int i = 0; i < 1000; i++) //sendByte(0); //resetQpBox();
            serial.Write(new byte[]{0}, 0, 1);

        if (waitForByte(0xAC) == false) //port not answer, no device connected
            throw new Exception("No answer from device"); //=>
        
        outputString(String.Format("Downloading level-2 boot loader chunks..."));
        serial.Write(initChunks, 0, loaderSize); // send boot record

        waitReadIncomingBytes(); // wait for "{Init=1}" 

        if(increaseBaudRate != 9600){ // optionally increase baud based off of speed box
            string baudPacket = String.Format("{0}{1}{2}{3}",
            "{", "PacNum=1,Baud=", increaseBaudRate, "}"); 

            byte[] increaseBaud = System.Text.Encoding.ASCII.GetBytes(baudPacket);
            serial.Write(increaseBaud, 0, increaseBaud.Length);

            waitReadIncomingBytes(); // wait for "{PacNum=1}"
            serial.BaudRate = increaseBaudRate; // increase baud rate to communicate

            Thread.Sleep(100);  // wait 100 ms, allow some time 
                                // for the baud changes to take place
        }

        byte[] packet = new byte[1296];
        while ( (fstream.Read(packet, 0, packet.Length)) != 0){
            readIncomingBytes();
            serial.Write(packet, 0, packet.Length);
        }
        
        if(serial.BytesToRead != 0){
            readIncomingBytes(); // read the rest of the packet acks, then good to go
        }

        Thread.Sleep(100);
        serial.Write("{Go}"); // tell the firmware to start

        try{
            string ack = serial.ReadLine(); // wait for "ok:all channels cleared\r\n"
            outputString(ack);
            outputString("Finished");
        }
        catch (TimeoutException) {
            outputString("Boot failed");
            Thread.Sleep(1000); //? 2000->1000
        }
        return;
    }

    //------------------------------
    
    bool waitForByte(byte want)
    {
        outputString(String.Format("Waiting for byte: 0x{0:x}", want));

        for (int i = 0; i < 100; i++){
            byte b = (byte) serial.ReadByte();
            if (b == want){
                return true;
            } else {
                outputString(String.Format("discarding: {0:x}", b));
            }
        }
        outputString(String.Format("giving up"));
        return false;
    }

    void waitReadIncomingBytes()
    {
        while(serial.BytesToRead == 0){
            Thread.Sleep(10); // wait for bytes to come
        }
        Thread.Sleep(100);
        readIncomingBytes();
    }

    string lastResponse = "";
    void readIncomingBytes()
    {
        int num; 

        if (cancelRequested){
            // tbd: make a custom exception for this
            throw new System.Exception("Cancel Requested");
        }

        while ( (num = serial.BytesToRead) != 0){
            for (int i = 0; i < num; i++){
                byte b = (byte) serial.ReadByte();
                if (b == '{'){
                    lastResponse = "{";
                } else if (b == '}'){
                    lastResponse += "}";
                    outputString(lastResponse); //processResponse(lastResponse);
                                                //void processResponse(string s) => outputString(s);
                } else
                    lastResponse += (char) b;
            }
        }
    }
}
    

//Form FW Dwnload Dialog
class FirmwareDownload : Form
{
    //Button mainFormBtn;           //Calback to main form via Button
    Button cancel;
    Label message;
    FirmwareDownloader downloader;  //Dwnloader class
    bool canceledBtn = false;
    public bool bootFail = false;   //result
    //Stream fstream;

    //Call ShowDialog() after ctor
    public FirmwareDownload(){
        initForm();
    }

    //open port and create dwnload thread
    public void StartDownload(string commPort, int increaseBaud){

        Log.Information("Boot starting " + commPort +".9600->"+ increaseBaud);

        downloader = new FirmwareDownloader(commPort, AppConf.FwFile, increaseBaud);
        
        //publish form message
        downloader.EmitMessage  += (s,e)=>
            {message.Invoke(new MethodInvoker(delegate { message.Text = e.StringValue; }));};
        
        //dialog form auto closer
        downloader.EmitFinished += (s,e)=>{ 
                bootFail = downloader.bootFail;  //=> return value
                try{
                if (this.InvokeRequired && !this.IsDisposed)
                        this.Invoke( new Action(() => {Close();} ));
                }catch (ObjectDisposedException ex){} //do nothing
            };

        Thread t = new Thread(() => { downloader.Download(); });    //=> Dwnld thrd start
        t.Start();
        //pass through here immediately, thread run indepent, catch calback at EmitFinished

        return;
    }

    void initForm()
    {
        Text = "Firmware Download";
        Height = 150;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ControlBox = false;

        //Cancel button
        cancel = new Button();
        CancelButton = cancel;
        cancel.Text = "Cancel";
        cancel.DialogResult = DialogResult.Cancel;
        Controls.Add(cancel);

        //Manual stop by Btn
        cancel.Click += new EventHandler( (o,e)=>{ 
                canceledBtn = true;
                downloader.RequestCancel();
                Log.Warning("Boot canseled manually!");
                } 
            );

        int hmargin = 30;
        int vmargin = 10;
        cancel.Location = new Point(hmargin, ClientSize.Height - cancel.Height - vmargin);
        cancel.Left = ClientSize.Width - cancel.Width - hmargin;

        //Message text label
        message = new Label();
        message.AutoSize = true;
        message.Text = "";
        message.Location = new Point(hmargin, cancel.Top / 2);
        Controls.Add(message);
    }
}
