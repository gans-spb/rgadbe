/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Forms - Output Tab
 */

using System.Threading;

partial class RgaWindow{
    //TAB Calibration ---------------------------------------------------------
    InputIntNumBox lowCalMassBox = new InputIntNumBox("Mass", "amu");
    InputIntNumBox lowCalResolutionBox = new InputIntNumBox("Resolution", "");
    InputNumBox lowCalIonEnergyBox = new InputNumBox("Ion Energy", ""); //!Float
    InputNumBox lowCalPositionBox = new InputNumBox("Position", "");
    
    InputIntNumBox highCalMassBox = new InputIntNumBox("Mass", "amu");
    InputIntNumBox highCalResolutionBox = new InputIntNumBox("Resolution", "");
    InputNumBox highCalIonEnergyBox = new InputNumBox("Ion Energy", ""); //!Float
    InputNumBox highCalPositionBox = new InputNumBox("Position", "");

    InputNumBox totalCapBox = new InputNumBox("Total Integrating Cap", "pF");
    InputNumBox partialCapBox = new InputNumBox("Partial Integrating Cap", "pF");
    InputNumBox totalSensitivityBox = new InputNumBox("Total Sensitivity", "(A/A)/torr");
    InputNumBox partialSensitivityBox = new InputNumBox("Partial Sensitivity", "mA/torr");


    OutputNumBox modelBox = new OutputNumBox("Model", "");
    OutputNumBox serialBox = new OutputNumBox("S/N ", "");
    
    OutputNumBox versionMajorBox = new OutputNumBox("Major", "");
    OutputNumBox versionMinorBox = new OutputNumBox("Minor", "");

    InputNumBox piraniOneAtmBox = new InputNumBox("1ATM", "");
    InputNumBox piraniZeroAtmBox = new InputNumBox("Zero", "");
    InputNumBox piraniOneAtmTempBox = new InputNumBox("1ATMTemp", "");
    InputNumBox piraniZeroAtmTempBox = new InputNumBox("ZeroTemp", "");

    ListBox listBoxCal = new ListBox();
    
    TextBox textBoxCalName = new TextBox();
    TextBox textBoxAddit = new TextBox();
    Button todevCalBtn = new Button();
    Button fromdevCalBtn = new Button();


    void setupCalibrationTab()
    {
        GroupBox highCalGroup = new GroupBox();
        highCalGroup.Text = "High Cal";
        highCalGroup.Location = new Point(20, 30);
        highCalGroup.Size     = new Size(200, 130);  
        highCalGroup.Controls.Add(highCalMassBox);
        highCalGroup.Controls.Add(highCalResolutionBox);
        highCalGroup.Controls.Add(highCalIonEnergyBox);
        highCalGroup.Controls.Add(highCalPositionBox);

        GroupBox lowCalGroup = new GroupBox();
        lowCalGroup.Text = "Low Cal";
        lowCalGroup.Location = highCalGroup.Location;
        lowCalGroup.Size = highCalGroup.Size;
        lowCalGroup.Top  = highCalGroup.Top + highCalGroup.Height + 15;
        lowCalGroup.Controls.Add(lowCalMassBox);
        lowCalGroup.Controls.Add(lowCalResolutionBox);
        lowCalGroup.Controls.Add(lowCalIonEnergyBox);
        lowCalGroup.Controls.Add(lowCalPositionBox);

        GroupBox verGroup = new GroupBox();
        verGroup.Text = "Unit";
        verGroup.Size = new Size(200, 130);
        verGroup.Location = lowCalGroup.Location;
        verGroup.Top += lowCalGroup.Height + 15;
        verGroup.Controls.Add(modelBox);
        verGroup.Controls.Add(serialBox);
        verGroup.Controls.Add(versionMajorBox);
        verGroup.Controls.Add(versionMinorBox);

        GroupBox detectorGroup = new GroupBox();
        detectorGroup.Text = "Detector";
        detectorGroup.Size = highCalGroup.Size;
        detectorGroup.Width += 60;
        detectorGroup.Location = highCalGroup.Location;
        detectorGroup.Left += (highCalGroup.Width + 20);
        detectorGroup.Controls.Add(totalCapBox);
        detectorGroup.Controls.Add(partialCapBox);
        detectorGroup.Controls.Add(totalSensitivityBox);
        detectorGroup.Controls.Add(partialSensitivityBox);

        GroupBox piraniGroup = new GroupBox();
        piraniGroup.Text = "Pirani Calibration";
        piraniGroup.Size = new Size(260, 130);
        piraniGroup.Left = detectorGroup.Left;
        piraniGroup.Top = lowCalGroup.Top;
        piraniGroup.Controls.Add(piraniOneAtmBox);
        piraniGroup.Controls.Add(piraniZeroAtmBox);
        piraniGroup.Controls.Add(piraniOneAtmTempBox);
        piraniGroup.Controls.Add(piraniZeroAtmTempBox);

        tabCalibration.Controls.Add(lowCalGroup);
        tabCalibration.Controls.Add(highCalGroup);
        tabCalibration.Controls.Add(verGroup);
        tabCalibration.Controls.Add(piraniGroup);
        tabCalibration.Controls.Add(detectorGroup);

        int tmargin = 25, lmargin = 70;
        int vdelta = 23, i = 0;

        highCalMassBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        highCalResolutionBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        highCalIonEnergyBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        highCalPositionBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        
        i = 0;
        lowCalMassBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        lowCalResolutionBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        lowCalIonEnergyBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        lowCalPositionBox.Location = new Point(lmargin, tmargin +  vdelta * i++);

        i = 0;
        modelBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        serialBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        versionMajorBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        versionMinorBox.Location = new Point(lmargin, tmargin +  vdelta * i++);

        i = 0;
        lmargin += 60;
        piraniOneAtmBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        piraniZeroAtmBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        piraniOneAtmTempBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        piraniZeroAtmTempBox.Location = new Point(lmargin, tmargin +  vdelta * i++);

        i = 0;
        partialCapBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        partialSensitivityBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        totalCapBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        totalSensitivityBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        
        //Load-save block
        listBoxCal.Location = new Point (
            piraniGroup.Left + piraniGroup.Width + 15, highCalGroup.Top + 8);
        listBoxCal.Size = new Size (200, verGroup.Top + verGroup.Height -28);
        listBoxCal.BackColor = Color.MintCream;

        //new calibration selected
        listBoxCal.SelectedValueChanged += (o,s)=>{ 
            if (listBoxCal.SelectedItem == null ) return;
            curID.Calib = int.Parse(listBoxCal.SelectedItem.ToString().Split("-")[0]);
            calibDB2UI(); 
            updStatusID();
            };

        textBoxCalName.Location  = new Point (piraniGroup.Left, verGroup.Top+10);
        textBoxCalName.Size = new Size (piraniGroup.Width,20);
        textBoxCalName.BackColor = Color.LightCyan;
        textBoxCalName.Enabled = false;
        textBoxCalName.ReadOnly = true;

        textBoxAddit.Location  = new Point (textBoxCalName.Left, textBoxCalName.Top+32);
        textBoxAddit.Size = new Size (textBoxCalName.Width, 53);
        textBoxAddit.AutoSize = false;
        textBoxAddit.WordWrap = true;
        textBoxAddit.Multiline = true;
        textBoxAddit.BackColor = Color.WhiteSmoke;
        textBoxAddit.Enabled = false;
        textBoxAddit.ReadOnly = true;

        fromdevCalBtn.Text = "FromDevice";
        fromdevCalBtn.Width = 100;
        fromdevCalBtn.Location = new Point(
            textBoxAddit.Left, textBoxAddit.Top + textBoxAddit.Height + 10);
        fromdevCalBtn.Click += (o,s)=>{sendToQpBox("calibration");;};

        todevCalBtn.Text = "ToDevice";
        todevCalBtn.Width = 100;
        todevCalBtn.Location = new Point(
            textBoxAddit.Left + textBoxAddit.Width - todevCalBtn.Width, fromdevCalBtn.Top);
        todevCalBtn.Click += (o,s)=>{calibUI2DEV();};

        tabCalibration.Controls.Add(listBoxCal);
        tabCalibration.Controls.Add(textBoxCalName);
        tabCalibration.Controls.Add(textBoxAddit);
        tabCalibration.Controls.Add(fromdevCalBtn);
        tabCalibration.Controls.Add(todevCalBtn);
        
        UpdCalListFromDB();
    }

    void UpdCalListFromDB(){
        
        var q = App.DBcon.CalibTable
            .ToList()
            .Select(t => t.Id + "-" + t.Name);

        foreach(string s in q)
            listBoxCal.Items.Add(s);
            
        listBoxCal.SelectedIndex = listBoxCal.Items.Count - 1; //last
    }

    List<string> calibParLst = 
        ["HighCalMass","HighCalResolution","HighCalIonEnergy","HighCalPosition",
        "LowCalMass","LowCalResolution","LowCalIonEnergy","LowCalPosition",
        "PartialIntegratingCap","PartialSensitivity","TotalIntegratingCap","TotalSensitivity",
        "Pirani1ATM","PiraniZero","Pirani1ATMTemp","PiraniZeroTemp",
        "ModelNumber","SerialNumber","VersionMajor","VersionMinor"];
    List<string> rocalibParLst = 
        ["ModelNumber","SerialNumber","VersionMajor","VersionMinor"];

    List<string> additcalibParLst = 
        ["RwSettleTicks","SwSettleTicks"/*,"TotalAmpOffset","PartialAmpOffset"*/];

    void calibDB2UI(){
        tabCalibration.Text = "Calibration";
        
        foreach(var v in calibParLst)
            updateBoxIntFloat(v, "0.0");

        ExtorCalibTable tab = App.DBcon.CalibTable.Find(curID.Calib);
        if (tab == null) return;
        foreach(var v in tab.ParamDic)
            updateBoxIntFloat(v.Key, v.Value);

        textBoxCalName.Text = tab.Name;
        
        string adp=""; 
        string oval;
        foreach (string par in additcalibParLst)
            adp+= ( par + "=" + (tab.ParamDic.TryGetValue(par, out oval) ? oval : "none") + "; ");
        textBoxAddit.Text = adp;

        //listBoxCal.SelectedIndex = curID.Calib-1;
    }

     void calibDB2DEV(){
        if ( !isCommOpened || !isBooted) return;

        ExtorCalibTable cal = App.DBcon.CalibTable.Find(curID.Calib);

        string oval;
        foreach (string par in calibParLst)
            if (cal.ParamDic.TryGetValue(par, out oval) )
                if (!rocalibParLst.Contains(par))
                    serialQueTX.Enqueue("set:" + par + ":" + oval);

        foreach (string par in additcalibParLst)
            if (cal.ParamDic.TryGetValue(par, out oval) )
                serialQueTX.Enqueue("set:" + par + ":" + oval);
    }

    public void calibUI2DEV(){
        if ( !isCommOpened || !isBooted) return;

        foreach (string par in calibParLst){
            NumBoxBase ctl = findControl(par);
            if (ctl ==null ) continue;
            if (rocalibParLst.Contains(par)) continue;
                serialQueTX.Enqueue("set:" + par + ":" + ctl.Value);
        }
        
        //2DO Additional calibration parameters are not in GUI, add plz, now got from DB
        ExtorCalibTable cal = App.DBcon.CalibTable.Find(curID.Calib);
        string oval;
        foreach (string par in additcalibParLst)
            if (cal.ParamDic.TryGetValue(par, out oval) )
                serialQueTX.Enqueue("set:" + par + ":" + oval);
        
        //2do as Enqueue take a time, Tab marked "*" like changed
    }

 }
 