/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst. and Extorr, Igor Bocharov
 * Forms - From control auxulary helpers
 */

partial class RgaWindow{

    //current measurement
    public struct defIDs  {
        public int Meas;
        public int Calib;
        public int Output;
        public int Operate;
        public int MoniSet;
        public int MassCh;
    } 
    public defIDs curID;

    //Update from DB to UI controls
    //Form.onShow and DBmeas.onSelect
    void AllDB2UI(){

        isDB2UI = true;
        try{
            plotDB2UI();
            operatingDB2UI();
            outputDB2UI();
            calibDB2UI();
            masstabDB2UI();
            measDB2UI();

            updStatusID();
        }catch(Exception e){
            Log.Fatal("DB read structure error! " + App.StringFromEx(e));
        }

        isDB2UI = false;

        DropFlagOnTabs();
    }
    
    //change gui control values but consistent to DB records
    bool isDB2UI = false; 

    Dictionary<String,String> ParamsUI2Dic(List<string> parl){

        Dictionary<String,String> paramDic = [];

        foreach (string par in parl){
            NumBoxBase ctl = findControl(par);
            if (ctl == null ) continue;
            //App.con("UI->DIC "+par +"="+ ctl.Value);
            paramDic.Add( par, ctl.Value);
        }
        if (parl.Contains("PressureUnits"))
            paramDic.Add( "PressureUnits", pressureRadio.Value);

        return paramDic;
    }
    
    void updateBoxIntFloat(string bname, string val){
        //App.con("updBox:"+bname+"="+val);
        int intval;
        float floatval;
            if (Int32.TryParse(val, out intval) == true)
                    updateBox(bname, intval);
        else if (
            float.TryParse(val, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out floatval) == true)
                    updateBox(bname, floatval);
        
        //Update Amp-Tor-Pas unit here
        if (bname == "PressureUnits");

    }

//Controls dictionary list ------------------------------------------------
    Dictionary<string, NumBoxBase> controlsTable;
    Dictionary<NumBoxBase, string> variableTable;

    void setupControlsDict()
    {
        controlsTable = new Dictionary<string, NumBoxBase>();

        //Control
        controlsTable["ScanSpeed"] = scanSpeedBox;
        controlsTable["LowMass"] = lowMassBox;
        controlsTable["HighMass"] = highMassBox;
        controlsTable["SamplesPerAmu"] = samplesPerAmuBox;
        controlsTable["AutoStream"] = autoStreamBox;
        controlsTable["Focus1Volts"] = focus1Box;
        controlsTable["ElectronVolts"] = electronEnergyBox;
        controlsTable["FilamentEmissionMa"] = filamentEmissionBox;
        controlsTable["MultiplierVolts"] = multiplierVoltsBox;
        controlsTable["AutoZero"] = AutoZeroBox;
        controlsTable["Filament"] = FilamentOnBox;
        controlsTable["Encoding"] = commEncodingBox;
        controlsTable["SamplesPerLine"] = commSamplesBox;
        //controlsTable["PressureUnits"] = pressureRadio; //cannt inherited from NumBoxBase

        //Outputs
        controlsTable["DegasMa"] = degasCurrentBox;
        controlsTable["InteriorDegC"] = elecTempBox;
        controlsTable["SupplyVolts"] = powerSupplyBox;
        controlsTable["IonizerVolts"] = filamentVoltageBox;
        controlsTable["IonizerOhms"] = filamentResistanceBox;        
        //?"IonizerAmps"
        controlsTable["QuadrupoleDegC"] = sensorTempBox;     

        controlsTable["SourceGrid1Ma"] = source1Box;
        controlsTable["SourceGrid2Ma"] = source2Box;
        controlsTable["RfAmpVolts"] = rfAmpBox;

        controlsTable["PiraniTempVolts"] = piraniTempBox;  // in volts
        controlsTable["PiraniCorrVolts"] = piraniCorrBox;
        //"PiraniOhms"
        controlsTable["PiraniVolts"] = piraniVoltsBox;
        controlsTable["PiraniTorr"] = piraniTorrBox;
        controlsTable["PressureAmps"] = pressureAmpsBox;
        controlsTable["PressureTorr"] = pressureTorrBox;
        //"PressurePascal"

        controlsTable["LastSweep"] = lastSweepBox;
        controlsTable["FirstSweep"] = firstSweepBox;
        
   
        controlsTable["ReferenceVolts"] = referenceBox;
        controlsTable["GroundVolts"] = groundBox;
        controlsTable["Focus1FB"] = focus1FbBox;
        controlsTable["RepellerVolts"] = repellerBox;        
        controlsTable["FbPlus"] = plusFbBox;
        controlsTable["FbMinus"] = minusFbBox;

        controlsTable["FilamentPowerPct"] = filamentPwrBox;
        controlsTable["FilamentDacCoarse"] = filamentDacCoarseBox;
        controlsTable["FilamentDacFine"] = filamentDacFineBox;
        controlsTable["FilamentStatus"] = filamentStatusBox;

        //Calibrations
        controlsTable["LowCalMass"] = lowCalMassBox;
        controlsTable["LowCalResolution"] = lowCalResolutionBox;
        controlsTable["LowCalIonEnergy"] = lowCalIonEnergyBox;
        controlsTable["LowCalPosition"] = lowCalPositionBox;
        controlsTable["HighCalMass"] = highCalMassBox;
        controlsTable["HighCalResolution"] = highCalResolutionBox;
        controlsTable["HighCalIonEnergy"] = highCalIonEnergyBox;
        controlsTable["HighCalPosition"] = highCalPositionBox;
        
        controlsTable["PiraniZero"] = piraniZeroAtmBox;
        controlsTable["Pirani1ATM"] = piraniOneAtmBox;
        controlsTable["PiraniZeroTemp"] = piraniZeroAtmTempBox;
        controlsTable["Pirani1ATMTemp"] = piraniOneAtmTempBox;

        controlsTable["TotalCapPf"] = totalCapBox;
        controlsTable["PartialCapPf"] = partialCapBox;
        controlsTable["TotalSensitivity"] = totalSensitivityBox;
        controlsTable["PartialSensitivity"] = partialSensitivityBox;

        controlsTable["VersionMajor"] = versionMajorBox;
        controlsTable["VersionMinor"] = versionMinorBox;
        controlsTable["SerialNumber"] = serialBox;
        controlsTable["ModelNumber"] = modelBox;


        variableTable = new Dictionary<NumBoxBase, string>();
        foreach( KeyValuePair<string, NumBoxBase> p in controlsTable){
//          Console.WriteLine("variable {0}", p.Key);
            variableTable[p.Value] = p.Key;
            NumBoxBase b = p.Value;
            b.ValueChanged += new StringEventHandler(genericInputHandler);
        }
    }
    
    void genericInputHandler(object sender, StringEventArgs e)
    {
        string variable = findVariable((NumBoxBase)sender);
        if (variable == null){
            Console.WriteLine("Could not find variable");
            return;
        }
        string cmd = String.Format("set:{0}:{1}", variable, e.StringValue);  //=> send CMD to serial
        sendToQpBox(cmd);
        
        flagChanged(variable,e.StringValue);
        Log.Debug("GUI." + variable + " = " + e.StringValue); //log dbg level all GUI control changed events
    }

    string findVariable(NumBoxBase ctrl)
    {
        if (variableTable.ContainsKey(ctrl) == false)
            return null;
        return variableTable[ctrl];
    }

    //changes on UI from DB
    public bool isChangedOperation;
    public bool isChangedMassChannels;
    public bool isChangedMassTables;
    public string changedParams = "";

    //Changed tab marked by "*" - check this flag
    void flagChanged(string cname, string value){

        if (operatingParLst.Contains(cname)) {
            changedParams += (cname + "=" + value + "; ");
            tabOperating.Text = "Operating *";
            isChangedOperation = true;
            }

        if (calibParLst.Contains(cname)) {
            changedParams += (cname + "=" + value + "; ");
            tabCalibration.Text   = "Calibration *";
            }
        //isChangedOperation = true; //from DB only, NOT for manual change
        
        // "MassTable *"- see TabMassTable onCellValidating
    } 

    public void DropFlagOnTabs(){
        tabOperating.Text     = "Operating";
        tabCalibration.Text   = "Calibration";
        tabMassTable.Text     = "MassTable";
        isChangedOperation    = false;
        isChangedMassChannels = false;
        isChangedMassTables   = false;

        changedParams     = "";
    }
    
    void updateBox(string name, int value)
    {
        NumBoxBase outputCtrl = findControl(name);
        if ( outputCtrl != null )
            outputCtrl.setValue(value);
    }

    void updateBox(string name, double value)
    {
        NumBoxBase outputCtrl = findControl(name);
        if ( outputCtrl != null )
            outputCtrl.setValue(value);
    }

    void updateBox(string name, string value)
    {
        NumBoxBase outputCtrl = findControl(name);
        if ( outputCtrl != null )
            outputCtrl.setValue(value);
    }

    NumBoxBase findControl(string name){
        if (controlsTable==null) return null;

        if (controlsTable.ContainsKey(name) == false)
            return null;
        return controlsTable[name];
    }
}
