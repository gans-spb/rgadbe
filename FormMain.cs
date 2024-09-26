/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Main Form
 */

partial class RgaWindow : Form {
    //Top-level tab controls.
    TabControl tabControl;

    TabPage tabPlot;
    TabPage tabOperating;
    TabPage tabOutputs;
    TabPage tabCalibration;
    TabPage tabMassTable;
    TabPage tabDataBase;

	public RgaWindow()
	{
		Text = "RGA DataBase Edition";
		Size = new Size(AppConst.FormWidth, AppConst.FormHeight);
        MinimumSize = new System.Drawing.Size(AppConst.FormWidth, AppConst.FormHeight);
        Icon = AppConf.MainIcon;

        tabControl = new TabControl();
        tabPlot = new TabPage(" Plot");
        tabOutputs = new TabPage("Outputs");
        tabOperating = new TabPage("Operating");
        tabCalibration = new TabPage("Calibration");
        tabMassTable = new TabPage("MassTable");
        tabDataBase = new TabPage("DataBase");
                
        tabControl.TabPages.Add(tabPlot);
        tabControl.TabPages.Add(tabOperating);
        tabControl.TabPages.Add(tabOutputs);
        tabControl.TabPages.Add(tabCalibration);
        tabControl.TabPages.Add(tabMassTable);
        tabControl.TabPages.Add(tabDataBase);
        
        tabControl.SelectedIndex = 0; //TEST select tab at start

        Resize += new EventHandler(onRgaResize);
        
        tabControl.Location = new Point(5, 25);
        tabControl.Size = new Size(Width - 30, Height - 100);

        tabControl.SelectedIndexChanged +=(e,o) =>{
            //if (tabControl.TabPages[tabControl.SelectedIndex] == tabDataBase){ }
        };
  
        Controls.Add(tabControl);

        setupMenu();

        setupOutputTab();
        setupCalibrationTab();
        setupOperatingTab();
        setupPlotTab();
        setupMassTableTab();
        setupDBTab();
        
        setupControlsDict();
        
        FormClosing += new FormClosingEventHandler(onClose);
        Shown += onFormShown;
    }

	//All controls created here
    private void onFormShown(object sender, EventArgs e){
        RunButsEnDis(false);

        commInitCombos();

        DBlock = new DataBlock(this);
        
        //Here we got last measurements at DB tab
        dateSelBox.selectLastRecord();
        measSelBox.selectLastRecord();

        Graph.DropMousePoz();

        //Status bar, telnet client list
        App.TxtSrv.sendClientListCb = ClientListCb;

        //start serialQueRX reader, always on cause Telnet
        StartRXTimer();
    }

    //Main menu and status bar ------------------------------------------------

    private ToolStripMenuItem itemMenuTelnet;
    private ToolStripMenuItem itemMenuComOpen;
    private ToolStripMenuItem itemMenuComBoot;
    private ToolStripMenuItem itemMenuComClose;

    private ToolStripMenuItem itemMenuRenew;
    private ToolStripMenuItem itemMenuMonitor;
    private ToolStripMenuItem itemMenuSweep;
    private ToolStripMenuItem itemMenuTrend;
    private ToolStripMenuItem itemMenuStop;
    
    private ToolStripStatusLabel statusLabelIP;
    private ToolStripStatusLabel statusLabelCom;
    private ToolStripStatusLabel statusLabelRun;
    private ToolStripStatusLabel statusLabelDataBlock;
    private ToolStripStatusLabel statusLabelID;
    private ToolStripStatusLabel statusLabelAlarm;
    private ToolStripStatusLabel statusLabelLog;
    private ToolStripStatusLabel statusLabelTime;

    public Bitmap SquareLed(Color col){
        Bitmap bmp = new Bitmap(48, 48);
        Graphics g = Graphics.FromImage(bmp);
        g.FillRectangle(new SolidBrush(col), 8, 8, 28, 28);
        return bmp;
    }

    //Menu items
    void setupMenu(){       
        MenuStrip mainMenu = new MenuStrip();
        mainMenu.ShowItemToolTips = true;
        MainMenuStrip = mainMenu;
      
        //Com port open-boot-close, and exit app
        ToolStripMenuItem portItem = new ToolStripMenuItem("ComPort");

            itemMenuTelnet  = portItem.DropDownItems.Add("&Telnet", SquareLed (Color.Yellow),
                TelnetSrvOnOff ) as ToolStripMenuItem;
            itemMenuTelnet.ShortcutKeys  = Keys.Control | Keys.L;

            itemMenuComBoot  = portItem.DropDownItems.Add("&Boot", SquareLed (Color.Green),  
                (o,e)=>{onCommBoot(new Button(){Text="Boot"}, e);} ) as ToolStripMenuItem;
            itemMenuComBoot.ShortcutKeys  = Keys.Control | Keys.B;

            itemMenuComOpen  = portItem.DropDownItems.Add("&Open", SquareLed (Color.Orange), 
                (o,e)=>{onCommOpenClose(new Button(){Text="Open"}, e);}) as ToolStripMenuItem;
            itemMenuComOpen.ShortcutKeys  = Keys.Control | Keys.O;

            itemMenuComClose = portItem.DropDownItems.Add("&Close", SquareLed (Color.Red),  
                (o,e) => {onCommOpenClose(new Button(){Text="Close"}, e);} ) as ToolStripMenuItem;
            itemMenuComClose.ShortcutKeys  = Keys.Control | Keys.S;
            itemMenuComClose.Enabled = false;

            portItem.DropDownItems.Add(new ToolStripSeparator());
            portItem.DropDownItems.Add("Exit", null, this.onMenuAppExit);

        mainMenu.Items.Add(portItem);

        //Devise run Sweep-Trend-Stop
        ToolStripMenuItem startItem = new ToolStripMenuItem("Start");
        
            itemMenuRenew = startItem.DropDownItems.Add("Re&new", null,
                (e,o)=>{sendToQpBox("symbols");}) as ToolStripMenuItem;
            itemMenuRenew.ShortcutKeys  = Keys.Control | Keys.N;
            itemMenuRenew.Enabled = false;
            
            startItem.DropDownItems.Add(new ToolStripSeparator());

            itemMenuMonitor = startItem.DropDownItems.Add("&Monitor", SquareLed (Color.CornflowerBlue),
                (e,o)=>serialQueRX.Enqueue("ccmd:moni")) as ToolStripMenuItem;
            itemMenuMonitor.ShortcutKeys  = Keys.Control | Keys.M;
            itemMenuMonitor.Enabled = false;

            itemMenuSweep = startItem.DropDownItems.Add("S&weep", SquareLed(Color.Purple),
                (e,o)=>serialQueRX.Enqueue("ccmd:sweep")) as ToolStripMenuItem;
            itemMenuSweep.ShortcutKeys  = Keys.Control | Keys.W;
            itemMenuSweep.Enabled = false;

            itemMenuTrend = startItem.DropDownItems.Add("&Trend", SquareLed(Color.Blue), 
                (e,o)=>serialQueRX.Enqueue("ccmd:trend")) as ToolStripMenuItem;
            itemMenuTrend.ShortcutKeys  = Keys.Control | Keys.T;
            itemMenuTrend.Enabled = false;

            itemMenuStop  = startItem.DropDownItems.Add("&Stop",  SquareLed(Color.Red),  
                (e,o)=>{Stop("manual");}) as ToolStripMenuItem;
            itemMenuStop.ShortcutKeys  = Keys.Control | Keys.P;
            itemMenuStop.Enabled = false;
        mainMenu.Items.Add(startItem);       
    
        //An aux syslog-help-about modal box
        ToolStripMenuItem helpItem = new ToolStripMenuItem("Help");
            helpItem.DropDownItems.Add("Console",null,menuCon);
            helpItem.DropDownItems.Add("Log",null,menuLog);
            helpItem.DropDownItems.Add("Help",null,menuHelp);
            helpItem.DropDownItems.Add("About",null,menuAbout);
        mainMenu.Items.Add(helpItem);

        Controls.Add(mainMenu);
        
        //Status line bottom-----------------------------------------
        StatusStrip statusStrip = new StatusStrip();
        statusStrip.ShowItemToolTips = true;
        
        statusLabelIP = new ToolStripStatusLabel("ip:"+AppConf.IPPortNum.ToString(), 
            SquareLed(Color.Lime), TelnetSrvOnOff);

        statusLabelIP.TextImageRelation = TextImageRelation.TextBeforeImage;
        statusLabelIP.ImageAlign = ContentAlignment.MiddleLeft;
        statusLabelIP.AutoSize = false;
        statusLabelIP.Width = 70;
        statusLabelIP.ToolTipText = "nobody";
        statusStrip.Items.Add(statusLabelIP);

        statusLabelCom = new ToolStripStatusLabel(userCommPort, SquareLed(Color.Red), 
            (o,e) =>{onCommOpenClose(new Button(){Text="Open"}, e);}
            );
        statusLabelCom.TextImageRelation = TextImageRelation.TextBeforeImage;
        statusLabelCom.ImageAlign = ContentAlignment.MiddleLeft;
        statusLabelCom.AutoSize = false;
        statusLabelCom.Width = 60;
        statusStrip.Items.Add(statusLabelCom);
        
        statusLabelRun = new ToolStripStatusLabel("Stop", SquareLed(Color.Red), null);
        statusLabelRun.TextImageRelation = TextImageRelation.TextBeforeImage;
        statusLabelRun.ImageAlign = ContentAlignment.MiddleLeft;
        statusLabelRun.AutoSize = false;
        statusLabelRun.Width = 70;
        statusLabelRun.ToolTipText = "Moni/Sweep/Trend/Stop";
        statusStrip.Items.Add(statusLabelRun);

        statusLabelDataBlock = new ToolStripStatusLabel("", null);
        statusLabelDataBlock.TextImageRelation = TextImageRelation.TextBeforeImage;
        statusLabelDataBlock.ImageAlign = ContentAlignment.MiddleLeft;
        //statusLabelDataBlock.AutoSize = false;
        //statusLabelDataBlock.Width = 80;
        statusStrip.Items.Add(statusLabelDataBlock);

        statusLabelID = new ToolStripStatusLabel("ID");
        statusLabelID.ToolTipText = "MeasID OutputID OperateID MassChID MoniSetID CalibID";
        statusStrip.Items.Add(statusLabelID);

        statusLabelAlarm = new ToolStripStatusLabel("Alarms",SquareLed(Color.LightGreen), null);
        statusLabelAlarm.TextImageRelation = TextImageRelation.TextBeforeImage;
        statusLabelAlarm.ImageAlign = ContentAlignment.MiddleLeft;
        statusLabelAlarm.ToolTipText = "";
        statusStrip.Items.Add(statusLabelAlarm);

            ToolStripStatusLabel statusLabelSpring = new ToolStripStatusLabel();
            statusLabelSpring.Spring = true;
            statusStrip.Items.Add(statusLabelSpring);

        statusLabelLog = new ToolStripStatusLabel(System.IO.Path.GetFileName(App.logPath));
        statusLabelLog.IsLink = true;
        statusLabelLog.LinkBehavior = LinkBehavior.AlwaysUnderline;
        statusLabelLog.Click += new System.EventHandler(menuLog);
        statusStrip.Items.Add(statusLabelLog);

        //Clock 1sec 
        timer1s = new Timer();
        timer1s.Interval = 1000;
        timer1s.Tick += new System.EventHandler(onTick1s);
        timer1s.Enabled = true;
        
        statusLabelTime = new ToolStripStatusLabel("v1.0-Feb24");
        statusLabelTime.ForeColor = Color.SlateBlue;
        statusLabelTime.Alignment = ToolStripItemAlignment.Right;
        statusLabelTime.AutoSize = false;
        statusLabelTime.Width = 80;
        statusLabelTime.ToolTipText = AppConst.Ver;
        statusStrip.Items.Add(statusLabelTime);

        Controls.Add(statusStrip);
    }  

    //Menu handlers -----------------------------------------------------------
    void TelnetSrvOnOff(object sender, EventArgs e)
    {
        if (App.TxtSrv.isRun) {
            App.TxtSrv.Stop(); 
            SetTelnetSquareMode(1);
            }
        else {
            Task iptask = App.TxtSrv.Run();
            SetTelnetSquareMode(2);
        }
    }
    
    void ClientListCb(string s)
    {   
        statusLabelIP.ToolTipText = s;
        if (s=="closed") SetTelnetSquareMode(1);
            else if (s=="nobody") SetTelnetSquareMode(2);
                else            SetTelnetSquareMode(3);
    }

    void SetTelnetSquareMode(int mode)
    {
        statusLabelIP.Image = mode switch{
            1 => SquareLed(Color.Red),
            2 => SquareLed(Color.Lime),
            3 => SquareLed(Color.Green),
        };

        statusLabelIP.ToolTipText = mode switch{
            1 => "closed",
            2 => "nobody",
            3 => statusLabelIP.ToolTipText,
        };
    }

    private void onMenuAppExit(object sender, EventArgs e) {
        Application.Exit();
    }

    private void menuLog(object sender, EventArgs e) {
        System.Diagnostics.Process.Start("notepad.exe", App.logPath);
    }

    FormAuxConsole consoleAuxForm = null;
    void consoleAuxFormSetZero(Object sender, FormClosingEventArgs e){consoleAuxForm = null;}

    private void menuCon(object sender, EventArgs e) {
        consoleAuxForm = new FormAuxConsole(this);
        consoleAuxForm.FormClosing += new FormClosingEventHandler(consoleAuxFormSetZero);
    }

    private void menuHelp(object sender, EventArgs e) {
        string msg="";
        msg +="rgadbe.exe [options list]\n\n";
        msg +="--db DBname (rga.db)\n--com COM_port_name (COM1)\n--port IP_port_number (666)";
        msg +="\n--boot [boot at startup](noboot)";
        msg +="\n--renew [renew data each open port](norenew)";
        msg +="\n\n log file name 'rgadbe-20240214.log' autorotate every month";
        msg +="\n\n Telnet commands:";
        msg +="\n simple cmd - help moni sweep trend stop filament";
        msg +="\n set:cmd:value - meas poll tlimit";
        msg +="\n exit - close telnet session";
        MessageBox.Show(msg, "App console and telnet options");
    }

    private void menuAbout(object sender, EventArgs e) {
        FormAbout formAbout = new FormAbout();   
    }
    
    private int time1s;
    private void onTick1s(object sender, EventArgs e) {
        statusLabelTime.Text = TimeSpan.FromSeconds(time1s++).ToString("c");

        if (DBlock.isRun && DBlock.dataIndex > 0){
            statusLabelDataBlock.Text = (
                //DataBlock.MeasModeToString(DBlock.measMode) 
                "Id=" + curID.Meas.ToString()
                +  ", x=" + DBlock.dataIndex.ToString()
                + ", t=" + App.GetDTNow().Subtract (DBlock.dtStart)
            );
        }
    }

    public void updStatusID(){
        statusLabelID.Text = "meas" + curID.Meas + ":out"+ curID.Output + ":oper"+ curID.Operate +
                            ":mass" + curID.MassCh +":moni"+ curID.MoniSet +":cal"+ curID.Calib;
    }

    void onRgaResize(object sender, EventArgs e)
    {
        tabControl.Location = new Point(5, 25);
        tabControl.Size = new Size(Width - 30, Height - 100);
    }
    
    //finalizr
    bool shuttingDown = false;

    void onClose(object sender, System.EventArgs e)
    {
        shuttingDown = true;
        stopSerialReadThread();
    }
    
}
