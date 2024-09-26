/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Forms - Output Tab
 */

 partial class RgaWindow{
     //TAB Output --------------------------------------------------------------
    OutputNumBox degasCurrentBox = new OutputNumBox("Degas current", "mA");
    OutputNumBox elecTempBox = new OutputNumBox("Electronics temperature", String.Format("{0}C", "\x00B0"));
    OutputNumBox powerSupplyBox = new OutputNumBox("Power supply", "volts");
    OutputNumBox filamentVoltageBox = new OutputNumBox("Filament voltage", "volts");
    OutputNumBox filamentResistanceBox = new OutputNumBox("Filament resistance", "ohms");
    OutputNumBox sensorTempBox = new OutputNumBox("Sensor temperature", String.Format("{0}C", "\x00B0") );
    //---
    OutputNumBox source1Box = new OutputNumBox("Source1 current", "mA");
    OutputNumBox source2Box = new OutputNumBox("Source2 current", "mA");
    OutputNumBox rfAmpBox = new OutputNumBox("RF Amp (0 to 20.0)", "volts");

    OutputNumBox piraniTempBox = new OutputNumBox("Pirani temp (-0.1 to -1.0)", "volts");
    OutputNumBox piraniCorrBox = new OutputNumBox("Pirani Corr (-0.1 to -1.0)", "volts");
    OutputNumBox piraniVoltsBox = new OutputNumBox("Pirani pressure", "volts");
    OutputNumBoxSci piraniTorrBox = new OutputNumBoxSci("Pirani pressure", "torr");
    OutputNumBoxSci pressureAmpsBox = new OutputNumBoxSci("Total Pressure", "amps");
    OutputNumBoxSci pressureTorrBox = new OutputNumBoxSci("Total Pressure", "torr");

    OutputNumBox firstSweepBox = new OutputNumBox("First Sweep", "");
    OutputNumBox lastSweepBox =  new OutputNumBox("Last Sweep", "");
    //---
    OutputNumBox referenceBox = new OutputNumBox("Reference (2.45 to 2.55)", "volts");
    OutputNumBox groundBox = new OutputNumBox("Ground ref (+/- 0.2)", "volts");
    OutputNumBox focus1FbBox = new OutputNumBox("Focus1 FB (Focus1)", "volts");
    OutputNumBox repellerBox = new OutputNumBox("Repeller (2.0 - Elec_En))", "volts");
    OutputNumBox plusFbBox = new OutputNumBox("+FB (2.25 to 2.5)", "volts");
    OutputNumBox minusFbBox = new OutputNumBox("-FB (2.25 to 2.5)", "volts");
    OutputNumBox filamentPwrBox = new OutputNumBox("Filament Power", "%");
    OutputNumBox filamentDacCoarseBox = new OutputNumBox("Filament DAC Coarse", "");
    OutputNumBox filamentDacFineBox = new OutputNumBox("Filament DAC Fine", "");
    OutputNumBox filamentStatusBox = new OutputNumBox("Filament Status", "");

    void setupOutputTab(){
        GroupBox topGroup = new GroupBox();
        topGroup.Location = new Point(20, 30);
        topGroup.Size = new Size(525, 110);
        tabOutputs.Controls.Add(topGroup);

        topGroup.Controls.Add(degasCurrentBox);
        topGroup.Controls.Add(elecTempBox);
        topGroup.Controls.Add(powerSupplyBox);
        topGroup.Controls.Add(filamentVoltageBox);
        topGroup.Controls.Add(filamentResistanceBox);
        topGroup.Controls.Add(sensorTempBox);
    
        GroupBox bottomGroup = new GroupBox();
        tabOutputs.Controls.Add(bottomGroup);
        bottomGroup.Size = new Size(525, 280);
        bottomGroup.Location = topGroup.Location;
        bottomGroup.Top += (topGroup.Height + 10);

        bottomGroup.Controls.Add(source1Box);
        bottomGroup.Controls.Add(source2Box);
        bottomGroup.Controls.Add(rfAmpBox);
        bottomGroup.Controls.Add(piraniTempBox);
        bottomGroup.Controls.Add(piraniCorrBox);
        bottomGroup.Controls.Add(piraniTorrBox);
        bottomGroup.Controls.Add(piraniVoltsBox);
        bottomGroup.Controls.Add(plusFbBox);
        bottomGroup.Controls.Add(minusFbBox);

        bottomGroup.Controls.Add(referenceBox);
        bottomGroup.Controls.Add(groundBox);
        bottomGroup.Controls.Add(focus1FbBox);
        bottomGroup.Controls.Add(repellerBox);
        bottomGroup.Controls.Add(filamentPwrBox);
        bottomGroup.Controls.Add(filamentDacCoarseBox);
        bottomGroup.Controls.Add(filamentDacFineBox);
        bottomGroup.Controls.Add(filamentStatusBox);
        bottomGroup.Controls.Add(pressureAmpsBox);
        bottomGroup.Controls.Add(pressureTorrBox);
        bottomGroup.Controls.Add(lastSweepBox);
        bottomGroup.Controls.Add(firstSweepBox);

        int tmargin = 20, lmargin = 150;
        int vdelta = 23, i = 0;

        // First column, top, of Output tab.
        degasCurrentBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        elecTempBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        powerSupplyBox.Location = new Point(lmargin, tmargin +  vdelta * i++);

        // Second column, top, of Output tab.
        lmargin = 400;
        i = 0;
        filamentVoltageBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        filamentResistanceBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        sensorTempBox.Location = new Point(lmargin, tmargin +  vdelta * i++);


        // First column, bottom, of Output tab.
        tmargin = 20;
        lmargin = 150;
        i = 0;
        source1Box.Location = new Point(lmargin, tmargin +  vdelta * i++);
        source2Box.Location = new Point(lmargin, tmargin +  vdelta * i++);
        rfAmpBox.Location =   new Point(lmargin, tmargin + vdelta * i++);
        piraniTempBox.Location = new Point(lmargin, tmargin + vdelta * i++);
        piraniCorrBox.Location =  new Point(lmargin, tmargin + vdelta * i++);
        piraniVoltsBox.Location = new Point(lmargin, tmargin + vdelta * i++);
        piraniTorrBox.Location = new Point(lmargin, tmargin + vdelta * i++);
        pressureAmpsBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        pressureTorrBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        firstSweepBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        lastSweepBox.Location = new Point(lmargin, tmargin +  vdelta * i++);

        // Second column, bottom, of Output tab.
        lmargin = 400;
        i = 0;

        referenceBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        groundBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        focus1FbBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        repellerBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        plusFbBox.Location =  new Point(lmargin, tmargin + vdelta * i++);
        minusFbBox.Location = new Point(lmargin, tmargin + vdelta * i++);
        filamentPwrBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        filamentDacCoarseBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        filamentDacFineBox.Location = new Point(lmargin, tmargin +  vdelta * i++);
        filamentStatusBox.Location = new Point(lmargin, tmargin +  vdelta * i++);

        Button fromdevOutputBtn = new Button(){
            Text = "FromDevice",
            Width = 100,
            Location = new Point(bottomGroup.Right-100, bottomGroup.Bottom + 10)
        };
        fromdevOutputBtn.Click += (o,s)=>{outputDEV2UI();};
        
        tabOutputs.Controls.Add(fromdevOutputBtn);
    }
       
    List<string> outputParamsList = [
        "GroundVolts", "ReferenceVolts", "PiraniTorr", "PiraniVolts", "PiraniOhms", "PiraniCorrVolts",
        "PiraniTempVolts", "Pirani1ATMCalSet", "PiraniZeroCalSet", "SupplyVolts", "QuadrupoleDegC", 
        "InteriorDegC", "IonizerVolts", "IonizerAmps", "IonizerOhms", "RfAmpVolts", "SourceGrid1Ma", 
        "SourceGrid2Ma", "FilamentDacCoarse", "FilamentDacFine", "FilamentPowerPct", "FbPlus", 
        "FbMinus", "Focus1FB", "RepellerVolts", "PressureAmps", "PressureTorr", "PressurePascal", 
        "FilamentStatus", "DegasMa", "isIdle", "LastSweep", "FirstSweep", 
        /*"FilTimeUntilSleep", "FilSleepTimeRemaining", "T1Store", "T1Tag", "ElapsedTime" */];
    
    void outputDB2UI(){
        ExtorOutputTable tab = App.DBcon.Outputs.Find(curID.Output);
        if (tab == null) return;
        foreach(var v in tab.ParamDic)
            updateBoxIntFloat(v.Key, v.Value);
    }
    
    public ExtorOutputTable outputUI2DB(){
        return
        new ExtorOutputTable
        {
            Name = ("OutsForMeasID:" + curID.Meas),
            ParamDic = ParamsUI2Dic(outputParamsList)
        };
    }
    
    public void outputDEV2UI(){
        if ( !isCommOpened || !isBooted) return;
        sendToQpBox("outputs");
    }

 }
