/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Forms - Data Base Tab
 */

partial class RgaWindow : Form {

   YearMonthComboBox dateSelBox = new YearMonthComboBox("Year-Month", "");
   MeasComboBox measSelBox = new MeasComboBox("Measure", "");
   TextBox datetimeTextBox = new TextBox();
   TextBox nameTextBox = new TextBox();
   TextBox descTextBox = new TextBox();
   Label measLabel;
   TextBox dataTextBox;
   Button dbDataReloadBtn;
   Button dbDataUpdateBtn;
   Button dbDataSaveBtn;

    void setupDBTab(){
        
      dateSelBox.cbonSelCh = onSelDateTime;
      measSelBox.cbonSelCh = onSelMeas;

      tabDataBase.Controls.Add(dateSelBox);
      tabDataBase.Controls.Add(measSelBox);
      tabDataBase.Controls.Add(datetimeTextBox);
      tabDataBase.Controls.Add(nameTextBox);
      tabDataBase.Controls.Add(descTextBox);
      
      dateSelBox.Location = new Point (80,10);
      dateSelBox.Width = 130;
      
      measSelBox.Location = dateSelBox.Location;
      measSelBox.Top +=40;
      measSelBox.Width = 130;

      datetimeTextBox.Location = measSelBox.Location;
      datetimeTextBox.Top +=40;
      datetimeTextBox.Width = 130;
      datetimeTextBox.Height = 55;
      datetimeTextBox.Multiline = true;
      datetimeTextBox.TextAlign = HorizontalAlignment.Right;
      datetimeTextBox.ReadOnly = true;
      datetimeTextBox.Enabled = false;

        Label datetimeLabel = new Label{
            Text = "Start/Stop",
            Location = datetimeTextBox.Location
        };
        datetimeLabel.Left -= 60;
        tabDataBase.Controls.Add(datetimeLabel);

        // ------ 
        measLabel = new Label{
            Text = "desc",
            Font = new Font(Label.DefaultFont, FontStyle.Bold),
            Location = dateSelBox.Location,
            AutoSize = true
            //Width = 250
        };
        measLabel.Left += 160;
        tabDataBase.Controls.Add(measLabel);
      
      nameTextBox.Location = measLabel.Location;
      nameTextBox.Top +=40;
      nameTextBox.Width = 200;
      nameTextBox.Height = 40;
      nameTextBox.BackColor = Color.LightCyan;
      
      descTextBox.Location = nameTextBox.Location;
      descTextBox.Top +=40;
      descTextBox.Width = 200;
      descTextBox.Height = 55;
      descTextBox.Multiline = true;
      descTextBox.WordWrap = true;
      descTextBox.ScrollBars = ScrollBars.Vertical;
      descTextBox.BackColor = Color.LightCyan;

        dataTextBox = new TextBox{
            Location = new Point(10, 160),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.Honeydew,
            ReadOnly = true
        };
        tabDataBase.Controls.Add(dataTextBox);

        tabDataBase.Resize += (e,o) =>{
            dataTextBox.Size = new Size(tabDataBase.Width-20,tabDataBase.Height-200);
        };
        
        // Buttons 
        dbDataReloadBtn = new Button{
            Text = "Reload",
            Location = new Point (nameTextBox.Left + 230, nameTextBox.Top),
            Width = 80,
        };
        
        dbDataReloadBtn.Click += (o,s)=>{
            measSelBox.onSelectionChanged(o,s);
        };

        tabDataBase.Controls.Add(dbDataReloadBtn);

        dbDataUpdateBtn = new Button{
            Text = "Update",
            Location = new Point (dbDataReloadBtn.Left, dbDataReloadBtn.Bottom + 12),
            Width = 80,
            Enabled = true,
            ForeColor = Color.DarkRed
        };

        dbDataUpdateBtn.Click += (o,s)=>{
            ExtorMeasurements mes = App.DBcon.Measurements.FirstOrDefault(x=> x.Id == curID.Meas);
            
            if (nameTextBox.Text != mes.Name) mes.Name = nameTextBox.Text;
            
            if (descTextBox.Text != "null") mes.Desc = descTextBox.Text;

            App.DBcon.SaveChanges();
        };
        
        nameTextBox.TextChanged+= (e,o) =>{dbDataUpdateBtn.Enabled = true;};
        descTextBox.TextChanged+= (e,o) =>{dbDataUpdateBtn.Enabled = true;};

        tabDataBase.Controls.Add(dbDataUpdateBtn);


        dbDataSaveBtn = new Button{
            Text = "ToFile",
            Location = new Point (dbDataUpdateBtn.Left, dbDataUpdateBtn.Bottom + 12),
            Width = 80,
            Enabled = true
        };
        
        dbDataSaveBtn.Click += (o,s)=>{

            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "csv files (*.csv)|*.csv| txt files (*.txt)|*.txt| All files (*.*)|*.*";
            sfd.FilterIndex = 0;
            sfd.RestoreDirectory = true;

            ExtorMeasurements mes = App.DBcon.Measurements.Find(curID.Meas);
            string fname = mes.Type + " ID." + mes.Id + " " +
                    mes.StartTime.ToString("yy.MM.dd HH.mm.ss");
            sfd.FileName = fname;

            
            if (sfd.ShowDialog() == DialogResult.OK)
                System.IO.File.WriteAllText(sfd.FileName, dataTextBox.Text);
        };
            
        tabDataBase.Controls.Add(dbDataSaveBtn);
    }


    void measDB2UI(){
        //stub, got from MeasBox().onSelected
    }

   //Year-Month ComboBox selected
    void onSelDateTime(string s){
        measSelBox.SetDT(s);
        }

    //Measure selected, MeasComboBox.onSelectionChanged
    void onSelMeas(string s){
        if (s=="") return;
        int id = int.Parse(s.Split("-")[0]);
        updateDBTab(id);
    }
    
    //update Tab from DB
    void updateDBTab(int id){

        ExtorMeasurements mes = App.DBcon.Measurements.Find(id);
        if (mes==null) return;

        App.DBcon.Entry(mes).Reload();

        curID.Meas     = mes.Id;
        curID.Output   = (mes.OutputsId == null) ? 0 : (int)mes.OutputsId;//!!!
        curID.Operate  = mes.OperateId;
        curID.MassCh   = mes.MassChId;
        curID.MoniSet  = mes.MoniSetId;
        curID.Calib    = mes.CalibId;
        
        listBoxCal.SelectedIndex = curID.Calib - 1;

        string dts = mes.StartTime.ToString("yy.MM.dd HH.mm.ss");
            dts+=  System.Environment.NewLine;
            dts+= ((mes.StopTime != null)    ? ((DateTime) mes.StopTime).ToString("yy.MM.dd HH.mm.ss") : "null");
            dts+=  System.Environment.NewLine;
            dts+= ((mes.ProcessTime != null) ? ((TimeSpan)mes.ProcessTime).ToString() : "null");
        datetimeTextBox.Text = dts;

        measLabel.Text = "id." + mes.Id.ToString() +  " typ." + mes.Type +
            " sz." + mes.Samples  + " poll." + mes.PollTime + 
                " tLim." + mes.TimeLim + " end." + mes.Final;

        nameTextBox.Text = mes.Name;
        descTextBox.Text = (mes.Desc != null) ? mes.Desc : "null";
        dbDataUpdateBtn.Enabled = false;

        AllDB2UI();             //=> Upd all GUI

        //udate Data text box
        if (mes.DataSet ==null || mes.DataSet.Count == 0 ) {
            dataTextBox.Text = "Strange, but empty dataset!";
            Log.Warning("DB id=" + mes.Id + " has empty DataSet!" );
            return; //=>
        }

        //trace numbers for one Sample
        int trace = mes.DataSet[0].Count;
        List<System.Text.StringBuilder> sbl = new List<System.Text.StringBuilder>();
        
        foreach (var x in mes.DataSet[0])
            sbl.Add(new System.Text.StringBuilder());

        string buf; int k=0;
        foreach (List<string> data in mes.DataSet){
            foreach (string d in data){
                buf = d + AppConst.csvDelimeter;
                (sbl[k]).Append(buf);
                k++;
            }
        k=0;
        }

        string hdr = mes.Name + Environment.NewLine;
        hdr += "type="+ mes.Type + " id=" + mes.Id + 
            " poll=" + mes.PollTime + " tlim=" + mes.TimeLim +
            " count=" +mes.Samples + " end=" + mes.Final + Environment.NewLine;
                    
        hdr += "start stop span: ";
        hdr += dts.Replace(Environment.NewLine," ");

        hdr += ((mes.Desc==null) ? "" : (Environment.NewLine + "desc: " + descTextBox.Text)) +
                    Environment.NewLine;
        
        if (mes.Type == "moni") {
            hdr += "mon_params: ";
            var monit = App.DBcon.MoniTable.Find(mes.MoniSetId);
            hdr += String.Join(" ", monit.ParamList);
        } 
        if (mes.Type == "sweep") {
            var monit = App.DBcon.Operate.Find(mes.OperateId);
            string pardic = "";
            foreach (var kv in monit.ParamDic)
                pardic += kv.Key + " " + kv.Value + " ";
            hdr += "params: ";
            hdr += pardic;
        }
        if (mes.Type == "trend") {
            hdr += "mass_ch: ";
            hdr += String.Join(" ", masstableUI2List());
        }

        hdr += Environment.NewLine;

        System.Text.StringBuilder tsb = new();

        tsb.Append (hdr + Environment.NewLine);

        foreach (var sb in sbl)
            tsb.Append (sb + Environment.NewLine) ;
        
        dataTextBox.Text = tsb.ToString();

    }

 }