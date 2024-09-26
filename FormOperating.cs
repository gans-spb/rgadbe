/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Forms - Operating Tab
 */

using System.Threading;

partial class RgaWindow{
    // TAB Operating ----------------------------------------------------------
    ScanSpeedComboBox scanSpeedBox = new ScanSpeedComboBox("Scan Speed", "/sec");
    InputIntNumBox lowMassBox = new InputIntNumBox("Low Mass", "amu");
    InputIntNumBox highMassBox = new InputIntNumBox("High Mass", "amu");
    InputIntNumBox samplesPerAmuBox = new InputIntNumBox("Samples/amu", "");
    
    public InputIntNumBox pollBox  = new InputIntNumBox("Poll Every", "sec"); //DBlock get val
    public InputIntNumBox limitBox = new InputIntNumBox("Time Limit", "min");
    ComboBox moniIdComboBox = new ComboBox();

    InputIntNumBox focus1Box = new InputIntNumBox("Focus1", "volts");
    InputNumBox electronEnergyBox = new InputNumBox("Electron energy", "e volts");
    InputNumBox filamentEmissionBox = new InputNumBox("Filament emission", "mA");
    InputIntNumBox multiplierVoltsBox =  new InputIntNumBox("Multiplier voltage", "volts");

    QpCheckBox AutoZeroBox = new QpCheckBox("Auto Zero", "");
    QpCheckBox autoStreamBox = new QpCheckBox("Auto Stream", "");
    QpCheckBox FilamentOnBox = new QpCheckBox("Filament On", ""); //disabled at OperaTab, see TabPlot
   
    CommPortComboBox commPortBox = new CommPortComboBox("Port", "");
    CommSpeedComboBox commSpeedBox = new CommSpeedComboBox("Speed", "");
    public Button commOpenCloseBtn = new Button();
    Button commBootBtn = new Button();
    
    CommEncodingComboBox commEncodingBox = new CommEncodingComboBox("Encoding", "");
    InputIntNumBox commSamplesBox = new InputIntNumBox("Samples", "/line");
    CheckBox tagCheckBox = new CheckBox();
    CheckBox cksumCheckBox = new CheckBox();

    GroupBox sweepGroup = new GroupBox();
    GroupBox pollGroup = new GroupBox();
    GroupBox operatingGroup = new GroupBox();
    GroupBox commGroup = new GroupBox();
    
    TextBox consoleBox = new TextBox();
    TextBoxNoBeep commandBox = new TextBoxNoBeep();
    Label commandLabel = new Label();

    void setupOperatingTab()
    {
        userCommPort  = AppConf.COMPortName;
        userCommSpeed = AppConst.COMSpeed.ToString();

        moniIdComboBox.Width = 100;
        moniIdComboBox.BackColor = Color.PowderBlue;

        Label moniIdLabel = new Label { Text = "Monitor ID" };

        sweepGroup.Text = "Sweep Parameters";
        sweepGroup.Location = new Point(20, 30);
        sweepGroup.Size = new Size(200, 123);

        pollGroup.Text = "Poll";
        pollGroup.Location = new Point(sweepGroup.Left, sweepGroup.Bottom+2);
        pollGroup.Size = new Size(sweepGroup.Width, 90);

        operatingGroup.Text = "Operating Parameters";
        operatingGroup.Size = new Size(200, 215);
        operatingGroup.Width += 20;
        operatingGroup.Location = sweepGroup.Location;
        operatingGroup.Left += sweepGroup.Width + 20;
        
        FilamentOnBox.Enabled = false;  //disable manual control, see TabPlot
        FilamentOnBox.SetLabelColor(Color.Tan);

        commGroup.Text = "Communications";
        commGroup.Size = operatingGroup.Size;
        commGroup.Location = operatingGroup.Location;
        commGroup.Left += operatingGroup.Width + 20;

        commOpenCloseBtn.Text = "Open";
        commOpenCloseBtn.Size = new Size(60, 20); // commPortBox.Size;
        commOpenCloseBtn.Enabled = false;

        commBootBtn.Enabled = false;

        limitBox.BackColor = Color.PaleTurquoise;
        limitBox.Minimum=0;
        limitBox.Increment=60;

        pollBox.BackColor = Color.PaleTurquoise;
        pollBox.Minimum = AppConst.PollBoxMin;
        pollBox.Increment=1;

        pollBox.ValueChanged += (o,e) =>{
            DBlock.setTimerPollInterval(int.Parse(pollBox.Value) * 1000);
            };

        sweepGroup.Controls.Add(scanSpeedBox);
        sweepGroup.Controls.Add(lowMassBox);
        sweepGroup.Controls.Add(highMassBox);
        sweepGroup.Controls.Add(samplesPerAmuBox);
        sweepGroup.Controls.Add(autoStreamBox);
        
        pollGroup.Controls.Add(pollBox);
        pollGroup.Controls.Add(limitBox);
        pollGroup.Controls.Add(moniIdComboBox);
        pollGroup.Controls.Add(moniIdLabel);

        operatingGroup.Controls.Add(focus1Box);
        operatingGroup.Controls.Add(electronEnergyBox);
        operatingGroup.Controls.Add(filamentEmissionBox);
        operatingGroup.Controls.Add(multiplierVoltsBox);
        operatingGroup.Controls.Add(AutoZeroBox);
        operatingGroup.Controls.Add(FilamentOnBox);   //disabled from this Tab and from DB
        operatingGroup.Controls.Add(autoStreamBox);

        commGroup.Controls.Add(commPortBox);
        commGroup.Controls.Add(commSpeedBox);
        commGroup.Controls.Add(commBootBtn);
        commGroup.Controls.Add(commOpenCloseBtn);
        commGroup.Controls.Add(commEncodingBox);
        commGroup.Controls.Add(commSamplesBox);
        commGroup.Controls.Add(tagCheckBox);
        commGroup.Controls.Add(cksumCheckBox);
        
        tagCheckBox.Text = "Tags";
        tagCheckBox.CheckedChanged += new EventHandler(onTagCheckedChanged);
        cksumCheckBox.Text = "Checksum";
        cksumCheckBox.CheckedChanged += new EventHandler(onCksumCheckedChanged);

        int tmargin = 25, lmargin = 80;
        int vdelta = 23, i = 0;

        scanSpeedBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        lowMassBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        highMassBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        samplesPerAmuBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        
        i = 0;
        pollBox.Location = new Point(lmargin, tmargin +  vdelta * i++ - 9);
        limitBox.Location = new Point(lmargin, tmargin +  vdelta * i++ - 9);
        moniIdComboBox.Location = new Point(lmargin, tmargin +  vdelta * i++ - 9);
        moniIdLabel.Location = new Point(lmargin-63, moniIdComboBox.Top+2);

        i = 0; lmargin = 105;
        focus1Box.Location = new Point(lmargin, tmargin +  vdelta * i++);
        electronEnergyBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        filamentEmissionBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        multiplierVoltsBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        AutoZeroBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        autoStreamBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        FilamentOnBox.Location = new Point(lmargin, tmargin +  vdelta * i++);


        i = 0; lmargin = 60;
        commPortBox.Location =  new Point(lmargin, tmargin +  vdelta * i++);
        commSpeedBox.Location =  new Point(lmargin, tmargin +  vdelta * i++);
        i++;
        commEncodingBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        commSamplesBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        tagCheckBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        cksumCheckBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        commSamplesBox.SetWidth(65);

        //Buts
        commOpenCloseBtn.Location = commSpeedBox.Location;
        commOpenCloseBtn.Left += 80;
        commOpenCloseBtn.Size = new Size(60, 20); // commPortBox.Size;
        commOpenCloseBtn.Text = "Open";
        commOpenCloseBtn.ForeColor = Color.DarkRed;
        commOpenCloseBtn.Enabled = false;
        

        commBootBtn.Location = commPortBox.Location;
        commBootBtn.Left += 80;
        commBootBtn.Text = "Boot";
        commBootBtn.Size = commOpenCloseBtn.Size;
        commBootBtn.ForeColor = Color.DarkRed;
        commBootBtn.Enabled = false;

        commOpenCloseBtn.Click += new EventHandler(onCommOpenClose);
        commBootBtn.Click += new EventHandler(onCommBoot);
        commPortBox.ValueChanged += new StringEventHandler(onCommPortChanged);
        commSpeedBox.ValueChanged += new StringEventHandler(onCommSpeedChanged);

        consoleBox.Name = "console";
        consoleBox.Font = new Font("Courier New", 8.25f);
        consoleBox.Multiline = true;
        consoleBox.WordWrap = false;
        consoleBox.ReadOnly = true;
        consoleBox.BackColor = Color.Ivory;
        consoleBox.ScrollBars = ScrollBars.Both;

        commandLabel.AutoSize = true;
        commandLabel.Text = "Command:";
        commandLabel.ForeColor = Color.Purple;

        commandBox.KeyDown += new KeyEventHandler(onCommandBoxKey);

        tabOperating.Resize += new EventHandler(onTabOperatingResize);
        //tabOperating.Controls.Add(refreshOperatingBtn);
        tabOperating.Controls.Add(sweepGroup);
        tabOperating.Controls.Add(pollGroup);
        tabOperating.Controls.Add(operatingGroup);
        tabOperating.Controls.Add(commGroup);
        tabOperating.Controls.Add(consoleBox);
        tabOperating.Controls.Add(commandBox);
        tabOperating.Controls.Add(commandLabel);

        Button fromdevControlsBtn = new(){
            Text = "FromDevice",
            Width = 80,
            Location = new Point(operatingGroup.Right-88, operatingGroup.Bottom-30)
        };
        fromdevControlsBtn.Click += (o,s)=>{sendToQpBox("controls");};
        tabOperating.Controls.Add(fromdevControlsBtn);
        fromdevControlsBtn.BringToFront();
        
    }

    private void onTabOperatingResize(object sender, EventArgs e)
    {
        consoleBox.Left = sweepGroup.Left;
        consoleBox.Width = commGroup.Right-20;
        consoleBox.Top = operatingGroup.Bottom + 10;

        commandBox.Top = tabOperating.Height - commandBox.Height - 20;
        commandBox.Left = consoleBox.Left;
        commandBox.Width = consoleBox.Width;
        commandBox.BackColor = Color.Snow;

        consoleBox.Height = commandBox.Top - consoleBox.Top -10;

        commandLabel.Location = commandBox.Location;
        commandLabel.Top += 5;
        commandBox.Left += 65;
        commandBox.Width -= 60;
    }

    //startup com box parameters
    public void commInitCombos(){
        commSpeedBox.SetDefaultSpeed();

        if (!App.CheckComNameExist(userCommPort)){
                //wrong com port!
                statusLabelCom.ForeColor = Color.Red; 
                itemMenuComOpen.Enabled  = false;
                itemMenuComBoot.Enabled  = false;
            }
        else{
            commPortBox.SetDefaultName(userCommPort);
            if (AppConf.Boot) onCommBoot(null,null);
        }
    }

    void onTagCheckedChanged(object sender, EventArgs e){
        bool isChecked = ((CheckBox) sender).Checked;
        usingTag = isChecked;
    }

    void onCksumCheckedChanged(object sender, EventArgs e){
        bool isChecked = ((CheckBox) sender).Checked;
        usingChecksum = isChecked;
    }

    //Send command to console
    void onCommandBoxKey(object sender, KeyEventArgs e){
        if ((e.KeyCode == Keys.Enter) && (commandBox.Text.Length != 0) ){
            string command = commandBox.Text;
            commandBox.Text = "";
            if (command.StartsWith(AppConst.clntCmd))
                serialQueRX.Enqueue(command);
            else
                sendToQpBox(command);
        }
    }

    //Com Port deals ----------------------------------------------------------
   
    string userCommPort = "";
    string userCommSpeed = "";
    bool   isCommOpened = false; //comport opened
    bool   isBooted     = false; //comport booted ok

    //Menu and TabPlot buttons
    private void RunButsEnDis(bool state){
        itemMenuRenew.Enabled   = state; 
        itemMenuMonitor.Enabled = state;
        itemMenuSweep.Enabled   = state;
        itemMenuTrend.Enabled   = state;
        itemMenuStop.Enabled    = !state&isBooted;
        
        moniBtn.Enabled  = state;
        sweepBtn.Enabled = state;
        trendBtn.Enabled = state;
        stopBtn.Enabled  = !state&isBooted;

        if (!state) statusLabelRun.Image = SquareLed (Color.Red);
    }
    
    public void CommButsEnDis(bool state){
        commOpenCloseBtn.Text = (state) ? "Open" : "Close";

        commPortBox.Enabled  = state;
        commSpeedBox.Enabled = state;
        commBootBtn.Enabled  = state;

        filamentCBox.Enabled = !state;

        itemMenuComOpen.Enabled = state;
        itemMenuComBoot.Enabled = state;
        itemMenuComClose.Enabled = !state; 

        if (state){
            statusLabelRun.Text  = "Stop";
            statusLabelCom.Image = SquareLed (Color.Red);
        } else if (isBooted)
            statusLabelCom.Image = SquareLed (Color.Green);
            else 
            statusLabelCom.Image = SquareLed (Color.Orange);

        RunButsEnDis (!state&isBooted);
    }

    //Open-Close port
    void onCommOpenClose(object sender, EventArgs e)
    {
        if ( (userCommPort == "") || (userCommSpeed == "") ) return;

        if ( ((Button)sender).Text == "Open" && !isCommOpened ){
            if (serialReadThread(userCommPort, userCommSpeed) == true){ //try open
                isCommOpened = true;
                isBooted = true;    //TEST for debug to enable sweep butns
                CommButsEnDis(false);
                if (AppConf.Renew == true) sendToQpBox("symbols");  
            }
            else { //cant open port

            }
        } else if (((Button)sender).Text == "Close"  && isCommOpened ){
            isCommOpened = false;
            stopSerialReadThread();
            CommButsEnDis(true);
        }
    }

    //Boot firmware
    void onCommBoot(object sender, EventArgs e)
    {
        if ( isCommOpened || (userCommPort == "") || (userCommSpeed == "") ) return;

        FirmwareDownload dlg = new FirmwareDownload();
        dlg.StartDownload( userCommPort, Int32.Parse(userCommSpeed));
        dlg.ShowDialog(); //blocked!

        if (!dlg.bootFail){
            isBooted = true;
            onCommOpenClose(new Button(){Text="Open"}, e); //=>re open port
        } else 
            CommButsEnDis(true);
    }

    //Buttons en/dis
    void checkSpecified()
    {// tbd extorr: this is still not right
        commOpenCloseBtn.Enabled = App.CheckComNameExist(userCommPort);

        bool isOpened = (serial != null) && (serial.IsOpen);
        commBootBtn.Enabled = !isOpened && (userCommPort.Length != 0);

    //extorr Console.WriteLine("port: '{0}'", userCommPort);
    //extorr Console.WriteLine("speed: '{0}'", userCommSpeed);

        itemMenuComOpen.Enabled  = true;
        itemMenuComBoot.Enabled  = true;
        itemMenuComClose.Enabled = false;
    }

    void onCommPortChanged(object sender, StringEventArgs e)
    {
        userCommPort = e.StringValue;
        statusLabelCom.Text = e.StringValue;
        statusLabelCom.ForeColor = Color.Black;
        checkSpecified();
    }

    void onCommSpeedChanged(object sender, StringEventArgs e)
    {
        userCommSpeed = e.StringValue;
        int speed;
        Int32.TryParse(userCommSpeed, out speed);
        
        if ( (serial != null) && serial.IsOpen){
            sendToQpBox(String.Format("stop")); // stop to prevent data loss
            sendToQpBox(String.Format("set:BaudRate:{0}", speed));
            do {
                Thread.Sleep(100);
                // sleep and allow messages to occur.
            } while (serial.BytesToWrite != 0);

        }
        //extorr here we should really wait for ok:BaudRate response at old baud rate 
        // before changing to new baud rate, using sleep as a shortcut
        Thread.Sleep(100);
        if (serial != null)
            serial.BaudRate = speed;

        checkSpecified();
    }


    List<string> operatingParLst = [
        "ScanSpeed","LowMass","HighMass","SamplesPerAmu","AutoStream",
        "Focus1Volts","ElectronVolts","FilamentEmissionMa","MultiplierVolts",
        "AutoZero","Encoding","SamplesPerLine", "PressureUnits"
        /*,"Filament"*/ //Filamnet is dangerous so never auto set from DB, removed
    ];
    //PressureUnits are from TabPlot.radioBox and NOT supported in serialDeals
    
    void operatingDB2UI(){
        ExtorOperateTable tab = App.DBcon.Operate.Find(curID.Operate);
        if (tab == null) return;
        foreach(var v in tab.ParamDic)
            updateBoxIntFloat(v.Key, v.Value);

        if (tab.ParamDic.ContainsKey("PressureUnits"))
            pressureRadio.Value = tab.ParamDic["PressureUnits"];
        
        ExtorMeasurements mes = App.DBcon.Measurements.Find(curID.Meas);
        pollBox.setValue  (mes.PollTime);
        limitBox.setValue (mes.TimeLim);
        moniIdComboBox.SelectedIndex = curID.MoniSet-1;
    }

    public ExtorOperateTable operatingUI2DB(){
        return
        new ExtorOperateTable(){
            Name = ("OperFrom"+curID.Operate),
            ParamDic = ParamsUI2Dic(operatingParLst)
        };
    }

    public void operatingUI2DEV(){
        if ( !isCommOpened || !isBooted) return;

        foreach (string par in operatingParLst){
            NumBoxBase ctl = findControl(par);
            if (ctl ==null ) continue;
                serialQueTX.Enqueue("set:" + par + ":" + ctl.Value);
        }
    }      


}
