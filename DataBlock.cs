/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr, Ioffe
 * Data stream collect and DB save
 */

partial class RgaWindow : Form {
    DataBlock DBlock;
}


//Data stream init, cont write, DB update
 class DataBlock{
    
    private readonly RgaWindow rgaWin;
    
    Timer timerPoll;   //Polling timer
    Timer timerSafety; //Safety timer
    TimeSpan tlim;
    public void setTimerPollInterval(int p) {timerPoll.Interval = p;}

    //(0) init timers
    public DataBlock(RgaWindow rw){
        rgaWin = rw;

        //request poll, from GUI
        timerPoll = new Timer { Enabled = false};
        timerPoll.Tick +=
            new System.EventHandler( 
                (o,e) =>{ EmitDataCommand(); } );

        //flush to DB delay, fixed
        timerSafety =  new Timer { Enabled = false, 
            Interval = AppConst.SafetyTimer*1000};
        timerSafety.Tick += 
            new System.EventHandler( 
                (o,e) =>{ CommitDataArray();} );        
    }

    public enum eMeasMode{Monitor, Sweep, Trend}

    public RgaWindow.MeasMode measMode;

    public ExtorMeasurements mes;
    
    ExtorOperateTable cot;  //current operation table
    ExtorMassChannels cmt;  //current mass table

    public List<String> MonitorParamList { get; set; } //cache from MoniTable.(curID.MoniSet);

    public List<String> dataL;  //a data sample
    List<List<string>>  dataLL; //list of samples
    public int dataIndex = 0;   //current sample index
    int awaitIndex;             //next index should be
    
    bool isDBcreated;
    public bool isRun = false;  //poll request emitted
    public string cause;        //reason of stop
    public DateTime dtStart;    //calc process time

    //(1)Set measurement type, prepare dataarrays, create Measurement and commit to DB
    public async void Start(RgaWindow.MeasMode mode){

        measMode = mode;
        
        mes = new ExtorMeasurements();

        dataLL = new List<List<string>>();
        dataIndex = 0;
        mes.DataSet = dataLL;
        mes.PollTime = Int32.Parse(rgaWin.pollBox.Value);
        mes.TimeLim  = Int32.Parse(rgaWin.limitBox.Value);
        cause = "fatal";  //start from fatal, until first CreateDBRecord

        Log.Information("Start " + RgaWindow.MeasModeToString(measMode) + 
            " poll=" + mes.PollTime + "s tlimit=" + mes.TimeLim +"m");
        
        //set current Calib
        rgaWin.calibUI2DEV();  

        //operating params to device,update DB if changed
        rgaWin.operatingUI2DEV();
        if (rgaWin.isChangedOperation)
            cot = rgaWin.operatingUI2DB();
        else 
            cot = App.DBcon.Operate.Find(rgaWin.curID.Operate);

        //mass table channels to device,update DB if changed
        rgaWin.masstableUI2DEV();
        if (rgaWin.isChangedMassChannels)
            cmt = rgaWin.masstableUI2DB();
        else 
            cmt = App.DBcon.MassChannels.Find(rgaWin.curID.MassCh);

        //request current device output values
        rgaWin.outputDEV2UI();
        
        //Get monitoring parameters from DB
        MonitorParamList = App.DBcon.MoniTable.Find(rgaWin.curID.MoniSet).ParamList;
        
        //take time to complete device interchange
        await DelayedStart(AppConst.PreRequest);
    }
    
    //(2) async delayed second part of run, to finish init device setting
    async private Task DelayedStart(int dt){
        
        await Task.Delay(dt);

        rgaWin.PlotSetup(measMode);
        LoLoHiHi = rgaWin.masstableAlarmTable();

        isRun = true;
        isDBcreated = false;

        //Timers
        dtStart = App.GetDTNow();
        mes.StartTime = dtStart;

        timerPoll.Interval = int.Parse(rgaWin.pollBox.Value) * 1000;
        timerPoll.Enabled  = true;

        tlim = TimeSpan.FromSeconds( mes.TimeLim * 60 );
        timerSafety.Enabled = true;

        switch(measMode){
        
        case RgaWindow.MeasMode.Monitor:
            mes.Type = "moni";
            mes.Name = "Monitor" +
                " pset"+ rgaWin.curID.MoniSet.ToString() +
                " mes" + rgaWin.curID.Meas.ToString();
            dataL = [.. Enumerable.Repeat ("", MonitorParamList.Count+1)];
            break;
        
        case RgaWindow.MeasMode.Sweep:
            mes.Type = "sweep";
            mes.Name = "Sweep" +
                " oper" + rgaWin.curID.Operate.ToString()+
                " mes"  + rgaWin.curID.Meas.ToString();

            awaitIndex = 0;
            
            //calc values per sample
            ExtorOperateTable ot = App.DBcon.Operate.Find(rgaWin.curID.Operate);
            int sz = (1 + int.Parse(cot.ParamDic["HighMass"]) - int.Parse(cot.ParamDic["LowMass"]))
                    * int.Parse(cot.ParamDic["SamplesPerAmu"]);
            dataL = [.. Enumerable.Repeat ("", sz+1)];
            
            //calc one sweep time
            float tm = sz / float.Parse(ot.ParamDic["ScanSpeed"]);

            //if sweep longer than poll
            if (mes.PollTime < tm){
                string msg ="Sweep "+(int)tm+"s longer poll " + mes.PollTime +"s! ";
                Log.Warning(msg);
                    mes.Desc = (mes.Desc == null) ? msg : mes.Desc + msg ;
                }
            break;

        case RgaWindow.MeasMode.Trend:
            mes.Type = "trend";
            mes.Name = "Trend" +
                " ch"  + rgaWin.curID.MassCh.ToString() +
                " mes" + rgaWin.curID.Meas.ToString();
            awaitIndex = 0;
            
            tm = rgaWin.masstableOpTime(); //real trend time about +3sec
            if (tm+3 > mes.PollTime){
                string msg ="Trend "+(int)tm+"s can be longer poll " + mes.PollTime +"s! ";
                Log.Warning(msg);
                    mes.Desc = (mes.Desc == null) ? msg : mes.Desc + msg ;
                }

            List<string> mt = rgaWin.masstableUI2List();
            mes.Desc = String.Join(" ", mt) + "; ";
            dataL = [.. Enumerable.Repeat ("", mt.Count+1)];
            break;

        default: 
            break;
        }
    }
    
    //(3) Send command to device every timerPoll
    public void EmitDataCommand(){

        //2Do "outputs" request corrupt first record
        //correct start time
        if (dataIndex==0){
            mes.StartTime = App.GetDTNow();
            dtStart = mes.StartTime;
        }

        if (isRun && 
            tlim > new TimeSpan(0) &&
            App.GetDTNow().Subtract(dtStart) > tlim){
                cause = "timer";
                rgaWin.Stop("timer");  //=> Timer Stop
                return;
            }
        
        switch(measMode){
        case RgaWindow.MeasMode.Monitor:
                foreach (string p in MonitorParamList)
                    rgaWin.serialQueTX.Enqueue("get:" + p);    //=>
            break;
        
        case RgaWindow.MeasMode.Sweep:
            //sweep time < pool time, increment index manually
            if ( dataIndex == awaitIndex-1) {
                List<String> db = new List<string>(dataL);
                dataLL.Add(db);
                dataIndex++;
                } 

            rgaWin.sendToQpBox("sweep:count:1");
            awaitIndex++;
            dataL[0] = dataIndex.ToString();
            break;

        case RgaWindow.MeasMode.Trend:
            //if trend take more time than poll
            if ( dataIndex == awaitIndex-1) {
                List<String> db = new List<string>(dataL);
                dataLL.Add(db);
                dataIndex++;
                } 
            rgaWin.sendToQpBox("trend:count:1");
            awaitIndex++;
            dataL[0] = dataIndex.ToString();
            break;

        default: 
            break;
        }
    }
    
    //(4) Create DB measurement record at first safety timer evt
    void CreateDBRecord(){
        try{

        mes.OperateId = rgaWin.curID.Operate;

        if (rgaWin.isChangedOperation == true){
            App.DBcon.Operate.Add(cot);
            App.DBcon.SaveChanges();
            mes.OperateId = cot.Id;
            rgaWin.curID.Operate = cot.Id;
        }

        //masstab changed but not mass
        if (rgaWin.isChangedMassTables)
            App.DBcon.SaveChanges();

        if (rgaWin.isChangedMassChannels == true){App.con("mch_sc");
            App.DBcon.MassChannels.Add(cmt);
            App.DBcon.SaveChanges();
            mes.MassChId= cmt.Id;
            rgaWin.curID.MassCh = cmt.Id;
        }

        if (rgaWin.changedParams != "" ){
            mes.Desc = ((mes.Desc==null)? "" : mes.Desc ) + rgaWin.changedParams;
            rgaWin.DropFlagOnTabs();
            }

        //For MoniID see MonitoringInit.moniIdComboBox.SelectedIndexChanged
        ExtorOutputTable et = rgaWin.outputUI2DB();
        App.DBcon.Outputs.Add(et);
        App.DBcon.SaveChanges();
        rgaWin.curID.Output = et.Id;
        mes.OutputsId = et.Id;

        mes.CalibId   = rgaWin.curID.Calib;
        mes.MassChId  = rgaWin.curID.MassCh;
        mes.MoniSetId = rgaWin.curID.MoniSet;
        
        App.DBcon.Measurements.Add(mes);
        App.DBcon.SaveChanges();
        rgaWin.curID.Meas = mes.Id;
        rgaWin.updStatusID();

        Log.Information("DB measure created id=" + rgaWin.curID.Meas.ToString());
        isDBcreated = true;
        }catch(Exception e){
            string es = e.ToString();

            string ess = App.StringFromEx(e);
            if(e.ToString().Contains("SQLite Error"))
                ess = es.Substring(
                    e.ToString().IndexOf("SQLite Error"),
                    e.ToString().IndexOf("SQLite Error") + 50
                );
            ess = ess.Split(System.Environment.NewLine)[0];
                
            Log.Error("DB meas create err: " + ess);
            //App.con(es);
            rgaWin.Stop("errdb");
            }
    }
    
    //(5) Update DB record at PollTimer
    public void CommitDataArray(){
        if(dataIndex==0) return;    //no data yet, device not answer

        if (!isDBcreated){
            CreateDBRecord();
                if (cause == "errdb") return;
            }
        else if (cause == "fatal") //fatal only for fist entry
            cause = "auto";

        try{
        mes.Samples = dataIndex;
        DateTime dtn = App.GetDTNow();
        mes.StopTime = dtn;
        mes.ProcessTime = dtn.Subtract (mes.StartTime);

        var ds =  App.DBcon.Measurements.Find(rgaWin.curID.Meas);
        ds.DataSet =  dataLL;

        mes.Final = cause;

        App.DBcon.Update(ds);
        App.DBcon.SaveChanges();

        }catch(Exception e){ 
            Log.Error("DB meas update err: " + App.StringFromEx(e)); 
        }
    }

    //(6) Stop from TabPlot.Stop
    public void Stop(){
        Log.Information("Stop " + RgaWindow.MeasModeToString(measMode) + " by " + cause);

        //rgaWin.sendToQpBox("ccmd:stop");    //=> stop point

        timerPoll.Enabled = false;
        timerSafety.Enabled = false;
        
        isRun = false;
        if ( cause != "errdb")
            CommitDataArray();
    }
    
    //(7) Flush data to DB at Main.Exception
    public void FatalFlush(){
        //2DO
    }

    // - - - Serial input stream
    //BeginStream:LowMass:1:HighMass:45:SamplesPerAmu:6:sweep:9
    public void onSweepStart(){
        //nothing 2do
    }

    //parce sweep line
    //<- s10:0:1.938e-15:8.094e-15:6.281e-15
    //2DO not support base64
    public void onSweepLine(string fromQp){
        if (!isRun || measMode != RgaWindow.MeasMode.Sweep) return;

        string[] fields = fromQp.Split(':');
        if (fields.Length < 3) return;

        int ss;
        if (Int32.TryParse(fields[1], out ss) == false) return;

        for (int i = 2; i < fields.Length; i++)
            if (fields[0]=="s10")
                dataL[1 + ss++] = fields[i];
            else if (fields[0]=="s16"){
                float fval;
                RgaWindow.hexToFloat(fields[i], out fval);
                dataL[1 + ss++] = fval.ToString();
            }
        
        if (ss==dataL.Count-1){
                List<String> db = new List<string>(dataL);
                dataLL.Add(db);
                dataIndex++;
            }       
    }

    //<- BeginTrend:sweep:54:1:12
    public void onTrendStart(){
        //nothing 2do
    }
    
    //alarms threshoulds
    List<List<float?>> LoLoHiHi;


    //parce trend line
    //<- t10:0:1.154e-13
    public void onTrendLine(string fromQp){
        if (!isRun || measMode != RgaWindow.MeasMode.Trend) return;

        string[] fields = fromQp.Split(':');
        if (fields.Length < 3) return;

        int ss;
        if (Int32.TryParse(fields[1], out ss) == false) return;
        //if (fields.Length-2>dataL.Count-1) App.con("!");

        string sval = "";
        float  fval = 0;
        for (int i = 2; i < fields.Length; i++){
            if (fields[0]=="t10"){
                sval = fields[i];
                fval = float.Parse(sval,
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            else if (fields[0]=="t16"){
                RgaWindow.hexToFloat(fields[i], out fval);
                sval = fval.ToString();
                } else sval = "";
            
            dataL[1+ss] = sval;

            CheckAlarm(ss, fval);

            ss++;
        
            if (ss==dataL.Count-1){
                List<String> db = new List<string>(dataL);
                dataLL.Add(db);
                dataIndex++;
            }
        }
    }

    //2do can be simlified
    void CheckAlarm(int ss, float fval){
        //var al = LoLoHiHi[ss][1]; App.con (al.ToString());
        int ch = (int)LoLoHiHi[ss][0];
        if (LoLoHiHi[ss][2]!=null && fval < LoLoHiHi[ss][2] && LoLoHiHi[ss][6] == 0) {
            LoLoHiHi[ss][6] = fval;
            Log.Warning("Alarm Ch{0}! {1:G}<LoAlarm", ch, fval);
            rgaWin.AlarmUpdate( "ch"+ch+"-LoAlarm; ");
            }
        if (LoLoHiHi[ss][3]!=null && fval < LoLoHiHi[ss][3] && LoLoHiHi[ss][7] == 0) {
            LoLoHiHi[ss][7] = fval;
            Log.Warning("Alarm Ch{0}! {1:G}<LoWarn", ch, fval);
            rgaWin.AlarmUpdate( "ch"+ch+"-LoWarn; ");
            }
        if (LoLoHiHi[ss][4]!=null && fval > LoLoHiHi[ss][4] && LoLoHiHi[ss][8] == 0) {
            LoLoHiHi[ss][8] = fval;
            Log.Warning("Alarm Ch{0}! HiWarn<{1:G}", ch, fval);
            rgaWin.AlarmUpdate( "ch"+ch+"-HiWarn; ");
            }
        if (LoLoHiHi[ss][5]!=null && fval > LoLoHiHi[ss][5] && LoLoHiHi[ss][9] == 0) {
            LoLoHiHi[ss][9] = fval;
            Log.Warning("Alarm Ch{0}! HiAlarm<{1:G}", ch, fval);
            rgaWin.AlarmUpdate( "ch"+ch+"-HiAlarm; ");
            }
    }

    //------------------
    //see Serial.enqueuePlotDataPoint
    public RgaWindow.GraphPoint gPoint; //Last Plot point

    //Parse serial monitoring
    int plotIndex;
    public bool onMonitoringLine(string[] fields){
        if (!isRun || measMode != RgaWindow.MeasMode.Monitor) return false;
            
            //try convert values
            int    intval; 
            float  floatval;
            string sval = fields[2];
            if (Int32.TryParse(sval, out intval) != true) 
                float.TryParse(sval, System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out floatval);
            else floatval = intval;
                
            //find monitoring parameters and create Plot point
            dataL[0] = dataIndex.ToString();
            
            if (dataIndex==0) plotIndex=0;
            int trace = 0;
            foreach (string s in MonitorParamList){
                if (fields[1] == s) {
                    
                    gPoint.mode = RgaWindow.MeasMode.Monitor;
                    gPoint.trace = trace;
                    gPoint.index = plotIndex;
                    gPoint.x = (float)plotIndex;
                    gPoint.y = floatval;
                    
                    dataL[trace+1] = sval;
                    
                    if (trace==dataL.Count-2){
                        List<String> db = new List<string>(dataL);
                        dataLL.Add(db);
                        dataIndex++;
                        //plot rotator for Moni - here
                        if ( (++plotIndex % rgaWin.plotSamples) == 0) plotIndex = 0;
                    }

                    return true;
                }
            trace++;
            }
        return false;
    }

 }
