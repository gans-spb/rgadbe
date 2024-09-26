/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Forms - Plot Tab
 */

using System.Drawing.Design;
using System.Runtime.CompilerServices;

partial class RgaWindow : Form {

    // TAB Plot ---------------------------------------------------------------
    InstrumentGraph Graph;
   
    TrackBar yscaleBar;
    TrackBar xscaleBar;
    Label yscaleBarLabel;
    Label xscaleBarLabel;

    PressureUnits pressureRadio = new PressureUnits();
    
    NumericUpDown plotLenBox = new NumericUpDown();
    Label plotLenLabel;
    
    List<TextBox> MonitorTextBoxList;
    TextBox dataBox1 = new TextBox();
    TextBox dataBox2 = new TextBox();
    TextBox dataBox3 = new TextBox();
    TextBox dataBox4 = new TextBox();
    TextBox dataBox5 = new TextBox();
    
    Label dataLabel1 = new Label();
    Label dataLabel2 = new Label();
    Label dataLabel3 = new Label();
    Label dataLabel4 = new Label();
    Label dataLabel5 = new Label();

    TextBox logBox = new TextBox();

    CheckBox filamentCBox;

    Button moniBtn  = new Button();
    Button sweepBtn = new Button();
    Button trendBtn = new Button();
    Button stopBtn  = new Button();

    Timer timer01s;
    Timer timer1s;
    
    void setupPlotTab()
    {
        //Plot
        Graph = new InstrumentGraph();
        Graph.YAxisTitle = "AMPS";
        Graph.XAxisTitle = "AMU";
        Graph.GraphTitle = "Mass Sweep";

        //Check new point and redraw plot
        timer01s = new Timer(){
            Interval = 100,
            Enabled = true
        };
        timer01s.Tick += new System.EventHandler(on01sTick);

        //Scalers
        yscaleBar = new TrackBar();
        yscaleBar.Minimum = 0;
        yscaleBar.Maximum = 100;
        yscaleBar.Width = 100;
        yscaleBar.TickStyle = TickStyle.None;
        yscaleBar.Orientation = Orientation.Horizontal;
        
        //2DO make callback from MousePozWheel
        yscaleBar.ValueChanged += (o,e)=>{
            double zoomY = yscaleBar.Value / (1.0 * (yscaleBar.Maximum - yscaleBar.Minimum));
            double zoomp = zoomY * (AppConst.loGraphP - AppConst.hiGraphP) + AppConst.hiGraphP;
            double maxY  = Math.Pow(10, zoomp);
            double minY  = -(maxY/10);
            
            Graph.yScale = yscaleBar.Value;
            Graph.SetRangeY(minY, maxY);
            //App.con("slideZoom "+ zoomY +" " +zoomp+" " + minY+" "+maxY+"; "); //TEST
        };
        
        yscaleBarLabel = new Label();
        yscaleBarLabel.AutoSize= true;
        yscaleBarLabel.Text = "YScale";

        xscaleBar = new TrackBar();
        xscaleBar.Minimum = 0;
        xscaleBar.Maximum = 100;
        xscaleBar.Width = 100;
        xscaleBar.TickStyle = TickStyle.None;
        xscaleBar.Orientation = Orientation.Horizontal;

        xscaleBar.ValueChanged += (o,e)=>{
            double zoomX = 1 - xscaleBar.Value / (1.0 * (xscaleBar.Maximum - xscaleBar.Minimum +1));
            Graph.xScale = 100-xscaleBar.Value;
            Graph.SetRangeX(0, zoomX * plotSamples);
        };
        
        xscaleBarLabel = new Label();
        xscaleBarLabel.AutoSize= true;
        xscaleBarLabel.Text = "XScale";

        //Text box for chart length
        plotLenBox.Width = 55;
        plotLenBox.BackColor = Color.PaleTurquoise;
        plotLenBox.TextAlign = HorizontalAlignment.Right;
        plotLenBox.Minimum = 10;
        plotLenBox.Maximum = 6000;
        plotLenBox.Increment = 100;
        plotLenBox.ValueChanged +=(o,e) =>{
            plotSamples = Convert.ToInt32(plotLenBox.Value);
            Graph.SetRangeXlimit(0, plotSamples);
            };
        plotLenBox.Value = AppConst.trendSize;

        plotLenLabel = new Label(){
            AutoSize= true,
            Text = "Len"
        };

        //Easter egg, test chart
        plotLenLabel.Click += (o,e) => { 
            MouseEventArgs me = e as MouseEventArgs;
            if ( me.Button == MouseButtons.Left)
                 TestPlot(MeasMode.Trend);
            else TestPlot(MeasMode.Sweep); };
        
        MonitoringInit();

        logBox.Size = new Size(200, 130);
        logBox.Font = new Font("Courier New", 8.25f);
        logBox.Multiline = true;
        logBox.WordWrap = false;
        logBox.ReadOnly = true;
        logBox.BackColor = Color.Ivory;
        logBox.ScrollBars = ScrollBars.Vertical;
      
        //Start - stop
        moniBtn.Text = "Monitor";
        moniBtn.BackColor = Color.PowderBlue;
        moniBtn.Click += (o,e) => serialQueRX.Enqueue("ccmd:moni");

        sweepBtn.Text = "Sweep";
        sweepBtn.BackColor = Color.LightGreen;
        sweepBtn.Click += (o,e) => serialQueRX.Enqueue("ccmd:sweep");
        
        trendBtn.Text = "Trend";
        trendBtn.BackColor = Color.LightGreen;
        trendBtn.Click += (o,e) => serialQueRX.Enqueue("ccmd:trend");

        stopBtn.Text = "Stop";
        stopBtn.BackColor = Color.Bisque;
        stopBtn.Click += (o,e) => Stop("manual");

        filamentCBox = new(){
            Text = "Filament",
            TextAlign = ContentAlignment.MiddleCenter,
            Appearance = Appearance.Button,
            BackColor = Color.Gold,
            Width = 70,
            Height = 35,
            Enabled = false
        };

        filamentCBox.Click += new EventHandler(OnFilament);
        tabPlot.Controls.Add(filamentCBox);

        pressureRadio.AmpsSelected += onAmpsSelected;
        pressureRadio.TorrSelected += onTorrSelected;
        pressureRadio.PasSelected += onPascalSelected;

        tabPlot.Controls.Add(Graph);
        
        tabPlot.Controls.Add(yscaleBar);
        tabPlot.Controls.Add(yscaleBarLabel);
        tabPlot.Controls.Add(xscaleBar);
        tabPlot.Controls.Add(xscaleBarLabel);

        tabPlot.Controls.Add(pressureRadio);
        tabPlot.Controls.Add(plotLenBox);
        tabPlot.Controls.Add(plotLenLabel);
        
        tabPlot.Controls.Add(dataBox1);
        tabPlot.Controls.Add(dataBox2);
        tabPlot.Controls.Add(dataBox3);
        tabPlot.Controls.Add(dataBox4);
        tabPlot.Controls.Add(dataBox5);
        tabPlot.Controls.Add(dataLabel1);
        tabPlot.Controls.Add(dataLabel2);
        tabPlot.Controls.Add(dataLabel3);
        tabPlot.Controls.Add(dataLabel4);
        tabPlot.Controls.Add(dataLabel5);

        tabPlot.Controls.Add(logBox);
        
        tabPlot.Controls.Add(moniBtn);
        tabPlot.Controls.Add(sweepBtn);
        tabPlot.Controls.Add(trendBtn);
        tabPlot.Controls.Add(stopBtn);

        tabPlot.Resize += new EventHandler(onSweepResize);
        
        //set callback for custom serilog, thread save through statusStrip component, avoid Form.Closing phase
        App.formSink.EmitMessage += new StringEventHandler(delegate (object o, StringEventArgs e){ 
            if (shuttingDown != true && logBox.IsHandleCreated == true)
                logBox.Invoke(new MethodInvoker(delegate { 
                    logBox.AppendText( DateTime.Now.ToString("HH:mm:ss ") + e.StringValue + System.Environment.NewLine); 
                    logBox.SelectionStart = logBox.TextLength;
                    logBox.ScrollToCaret();
                    })
                );
        });

    }


    private void onSweepResize(object sender, EventArgs e)
    {
        Graph.Width  = tabPlot.Width;
        Graph.Height = tabPlot.Height - 150;

        //left side --------
        //scalers
        yscaleBar.Top = Graph.Bottom + 10;
        yscaleBar.Left = 1;
        yscaleBarLabel.Location = yscaleBar.Location;
        yscaleBarLabel.Left = yscaleBar.Width - yscaleBarLabel.Width - 5;
        yscaleBarLabel.Top += 22;
        yscaleBarLabel.BringToFront();
        
        xscaleBar.Top = yscaleBar.Bottom+5;
        xscaleBar.Left = 1;
        xscaleBarLabel.Location = xscaleBar.Location;
        xscaleBarLabel.Left = xscaleBar.Width - xscaleBarLabel.Width - 5;
        xscaleBarLabel.Top += 20;
        xscaleBarLabel.BringToFront();

        plotLenBox.Location   = new Point(xscaleBar.Left+40, xscaleBar.Bottom + 10);
        plotLenLabel.Location = new Point(plotLenBox.Left-30, plotLenBox.Top+2);
        
        pressureRadio.Top  = Graph.Bottom + 8;
        pressureRadio.Left = yscaleBar.Width + 10;

        filamentCBox.Location  = new Point(pressureRadio.Left, pressureRadio.Bottom + 14);

        dataBox1.Location = new Point(pressureRadio.Left + pressureRadio.Width + 25,
                                      Graph.Bottom + 15);
        dataBox2.Location = new Point(dataBox1.Left, dataBox1.Top + 25*1);
        dataBox3.Location = new Point(dataBox1.Left, dataBox1.Top + 25*2);
        dataBox4.Location = new Point(dataBox1.Left, dataBox1.Top + 25*3);
        dataBox5.Location = new Point(dataBox1.Left, dataBox1.Top + 25*4);

        dataLabel1.Top = dataBox1.Top + 2;
        dataLabel2.Top = dataBox2.Top + 2;
        dataLabel3.Top = dataBox3.Top + 2;
        dataLabel4.Top = dataBox4.Top + 2;
        dataLabel5.Top = dataBox5.Top + 2;
        
        dataLabel1.Left = dataBox1.Left + dataBox1.Width + 2;
        dataLabel2.Left = dataLabel1.Left;
        dataLabel3.Left = dataLabel1.Left;
        dataLabel4.Left = dataLabel1.Left;
        dataLabel5.Left = dataLabel1.Left;

        //Monitor-Sweep-Trend-Stop
        int pos_rx = tabPlot.Width  - moniBtn.Width  - 5;
        int pos_ry = tabPlot.Height - stopBtn.Height - 5;
        
        stopBtn.Location  = new Point (tabPlot.Width  - moniBtn.Width  -10,
                                       tabPlot.Height - moniBtn.Height -10);
        trendBtn.Location = new Point (stopBtn.Left, stopBtn.Top - 35*1);
        sweepBtn.Location = new Point (stopBtn.Left, stopBtn.Top - 35*2);
        moniBtn.Location  = new Point (stopBtn.Left, stopBtn.Top - 35*3);
        
        logBox.Top = Graph.Bottom + 10;
        logBox.Left = dataLabel1.Left + 120;
        logBox.Width  =  stopBtn.Left - dataLabel1.Left - 140;
    }

    void plotDB2UI(){
        //nothing yet
    }

    void MonitoringInit(){
        //Moni Parameter list and textLabels onSelect
        moniIdComboBox.SelectedIndexChanged += (s,e)=> {
            curID.MoniSet = Convert.ToInt32(moniIdComboBox.Text.Split("-")[0]);
            var tb = App.DBcon.MoniTable.Find(curID.MoniSet);
            if (tb==null || tb.ParamList.Count < 4) return;

            dataLabel1.Text = tb.ParamList[0];
            dataLabel2.Text = tb.ParamList[1];
            dataLabel3.Text = tb.ParamList[2];
            dataLabel4.Text = tb.ParamList[3];
            dataLabel5.Text = tb.ParamList[4];
        };

        //Moni ID dropbox
        var moniItems = App.DBcon.MoniTable
            .Select(t => t.Id +"-"+ t.Name )
            .ToList();
        moniIdComboBox.Items.AddRange(moniItems.ToArray());
        moniIdComboBox.SelectedIndex = moniIdComboBox.Items.Count - 1;
        
        
        //Five data boxes
        MonitorTextBoxList = new List<TextBox>{
            dataBox1, dataBox2, dataBox3, dataBox4, dataBox5
        };
        
        foreach (TextBox tb in MonitorTextBoxList){
            tb.Width = 65;
            tb.ReadOnly = true;
            tb.BackColor = Color.Lavender;
        }

        dataLabel1.AutoSize = true;
        dataLabel2.AutoSize = true;
        dataLabel3.AutoSize = true;
        dataLabel4.AutoSize = true;
        dataLabel5.AutoSize = true;
    }

    //update moniTextBox and Operting textBoxes from Serial
    void textBoxMoniUpdate(List<String> dataValues){
        if (dataValues.Count<MonitorTextBoxList.Count) return;
        int i=0;
        foreach (TextBox tb in MonitorTextBoxList){
            string sval = dataValues[i+1]; //+1 cause 0=index
            MonitorTextBoxList[i].Invoke(
                new MethodInvoker(delegate { MonitorTextBoxList[i].Text = sval; }));
            updateBox( DBlock.MonitorParamList[i], sval); //tabOutputs
            i++;
        }
    }

    //Run sweep/trend deals ---------------------------------------------------

    //RunStop
    void StartMoni(){
        DBlock.Start( MeasMode.Monitor);

        RunButsEnDis(false);
        statusLabelRun.Text = "Monitor";
        statusLabelRun.Image = SquareLed (Color.CornflowerBlue);
    }
    
    void StartSweep(){
        DBlock.Start( MeasMode.Sweep);

        RunButsEnDis(false);
        statusLabelRun.Text = "Sweep";
        statusLabelRun.Image = SquareLed (Color.Purple);
        
        if (autoStreamBox.Checked == false)
            sendToQpBox("stream");
    }

    void StartTrend()
    {
        DBlock.Start( MeasMode.Trend);
        
        RunButsEnDis(false);
        statusLabelRun.Text = "Trend";
        statusLabelRun.Image = SquareLed (Color.Blue);

        statusLabelAlarm.ToolTipText = "";
        statusLabelAlarm.Image = SquareLed(Color.LightGreen);

        if (autoStreamBox.Checked == false)
            sendToQpBox("stream");
    }

    public void Stop(string cause)
    {
        DBlock.cause = cause;

        DBlock.Stop();

        RunButsEnDis(true);
        statusLabelRun.Text = "Stop";
        statusLabelRun.Image = SquareLed (Color.Red);
        statusLabelAlarm.Image = SquareLed(Color.LightGreen);
    }

    public void AlarmUpdate(string msg){
        statusLabelAlarm.ToolTipText += msg;
        statusLabelAlarm.Image = SquareLed(Color.Red);
    }


    //Check Pirani pressure and start Filament
    async void OnFilament(object sender, EventArgs e)
    {
        Func<string, Task<string>> 
            reqApBoxAsync = async (msg) =>
            {
                piraniTorrBox.Value = "0";
                sendToQpBox(msg);
                await Task.Delay(500);
                //App.con(piraniTorrBox.Value);
                return piraniTorrBox.Value;
            };
        
        string pt = await reqApBoxAsync("get:PiraniTorr");
        if (pt=="0") {
            Log.Error("Can`t get Pirani pressure ");
            filamentCBox.Checked = false;
            return;
            }

        if (App.StrToFloatZero(pt) >= AppConst.FilamentPre){
            Log.Warning("Pirani pressure {0} to high for Filament!", 
            piraniTorrBox.Value);
            filamentCBox.Checked = false;
            return;
            }
        bool fc = filamentCBox.Checked;
        sendToQpBox("set:Filament:" + (fc?"1":"0"));
        Log.Information("Filament " + (fc?"fire!":"off"));
        filamentCBox.BackColor = fc ? Color.OrangeRed: Color.Gold ;
    }

    //Chart deals -------------------------------------------------------------

    void TestPlot(MeasMode mode){
        
        PlotSetup(mode);

        int imax = 300;
        int kmax = 12;
        
        if (mode==MeasMode.Sweep) {kmax =1; imax = 45; streamHighMass=45;}
        plotLenBox.Text = imax.ToString();
        
        Random rnd = new Random(123);
        for (int i=0; i<imax; i++){
            for (int k=0; k<kmax; k++){
            float val = (float) (rnd.Next(0, 10) + k*10) ;//* (0.2e-1f) ;
            /*if (k==1) val*= 1.5E-10F;
            if (k==2) val*= 1.5E3F;
            if (k==3) val+=(i*6.2F + val*1E3F);
            if (k==5) val= 555 - val *(i*3.05F);
            if (k==6) val+=(i*0.006F);
            if (k==9) val= 897 - val *(i*10.11F);*/

            enqueueDataPoint( mode, k, i, (float)i, val);
            }
        }
    }

    void onAmpsSelected(object sender, EventArgs e)
    {
        // we should really wait to get the pressure unit change acknowledgement
        // before changing the axis title
        sendToQpBox("set:PressureUnits:0");
        Graph.YAxisTitle = "AMPS";
        flagChanged("PressureUnits","Amps");
}

    void onTorrSelected(object sender, EventArgs e)
    {
        // we should really wait to get the pressure unit change acknowledgement
        // before changing the axis title
        sendToQpBox("set:PressureUnits:1");
        Graph.YAxisTitle = "TORR";
        flagChanged("PressureUnits","Torr");
    }

    void onPascalSelected(object sender, EventArgs e)
    {
        // we should really wait to get the pressure unit change acknowledgement
        // before changing the axis title
        sendToQpBox("set:PressureUnits:2");
        Graph.YAxisTitle = "PASCAL";
        flagChanged("PressureUnits","Pas");
    }
 
    // 100ms dequer
    private void on01sTick(object sender, System.EventArgs e){
        PlotDequeue();
        ConsoleDequeue();
        TelnetDeque();     //telnet clients to serial, see SerialDeals TelnetDeque()
    }
    
    Queue qpBoxLines = new Queue();

    void logToConsole(string line)  // < = from serial
    {
        lock(qpBoxLines.SyncRoot)
            qpBoxLines.Enqueue(line);
    }
   
    //Console writer
    void ConsoleDequeue(){

        lock (qpBoxLines.SyncRoot) {
            while (qpBoxLines.Count > 0) {
                
                string s = (string) qpBoxLines.Dequeue();

                consoleBox.AppendText(s + Environment.NewLine); //=> big console

                if (consoleAuxForm != null)                     //=> aux console
                    consoleAuxForm.AppendText(s + Environment.NewLine);

                App.TxtSrv.SendAll(s + Environment.NewLine);    //=> telnet
                
            }
        }
    }

    public void PlotSetup(MeasMode mode){
        measMode = mode;
        Graph.ClearGraph();
        nextTrendSample = 0;

        if (mode == MeasMode.Sweep){
            Graph.SetRangeXlimit(streamLowMass - 0.6, streamHighMass + 0.6);
            Graph.GraphTitle = "Sweep";
            Graph.XAxisTitle = "AMU";
            Graph.SetTraceNames(new List<string>());
            } 
        else if (mode == MeasMode.Trend){
            Graph.SetRangeXlimit(0, plotSamples);
            Graph.GraphTitle = "Trend";
            Graph.XAxisTitle = "SAMPLE";
            Graph.SetTraceNames(masstableUI2List(true));
            masstableUI2IntList(out plot_ch);
            //App.con( String.Join( " ",  plot_ch) );
        }
        else if (mode == MeasMode.Monitor){
            Graph.SetRangeXlimit(0, plotSamples);
            Graph.GraphTitle = "Monitoring";
            Graph.XAxisTitle = "Time,s";
            Graph.YAxisTitle = "Parameters";
            Graph.SetTraceNames(App.DBcon.MoniTable.Find(curID.MoniSet).ParamList);
        }
    }
    
    //Plot de-que and plot.refresh.onpaint, Redraw, Init
    void PlotDequeue()
    {   // consider simplifying this through the use of a queue,
        // as is done for qpBoxLines, below.
        
        if (newDataToPlot == true){ //flagged at plot.enqueueDataPoint
            while (graphPoints.Count > 0){

                GraphPoint pt = graphPoints.Dequeue();

                if ( (measMode == MeasMode.Sweep) && sweepParametersChanged() ){
                    Graph.ClearGraph();
                    Graph.SetRangeXlimit(streamLowMass - 0.6, streamHighMass + 0.6);
                }

                if ( (Double.IsNaN(pt.x) == false) && (Double.IsNaN(pt.y) == false) )
                    Graph.DataPoint(pt.trace, pt.index, pt.x, pt.y); //set new point
            }
            newDataToPlot = false;
            Graph.Refresh(); //=> call plot.renderTrace via onPaint event
        }
    }
}