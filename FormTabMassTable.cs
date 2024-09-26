/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Forms - Mass Table Tab
 */

using System.Data;
using System.Reflection.Metadata.Ecma335;
using System.Threading;

partial class RgaWindow : Form {
   
    Button commitBut = new Button();
    Button discardBut = new Button();
    Button masstUploadBut = new Button();
    Button fromdeviceBut = new Button();
    DataGridView massTable = new DataGridView();
    bool isMassTableBinded = false;
    
    void setupMassTableTab() {
        
        commitBut.Text = "Commit";
        commitBut.Enabled = false;
        //commitBut.BackColor = Color.MistyRose;
        commitBut.ForeColor = Color.DarkRed;
        commitBut.Click += new EventHandler(onCommitBut);
        tabMassTable.Controls.Add(commitBut);

        discardBut.Text = "Discard";
        discardBut.Enabled = false;
        discardBut.Click += new EventHandler(onDiscardBut);
        tabMassTable.Controls.Add(discardBut);

        masstUploadBut.Text = "ToDevice";
        masstUploadBut.Enabled = true;
        masstUploadBut.Click += new EventHandler(onDevUploadBut);
        tabMassTable.Controls.Add(masstUploadBut);

        fromdeviceBut.Text = "Dev2Log";
        fromdeviceBut.Enabled = true;
        fromdeviceBut.Click += (o,e)=>{sendToQpBox("channel");};
        tabMassTable.Controls.Add(fromdeviceBut);

        App.DBcon.MassTable.Load();
        massTable.DataSource = App.DBcon.MassTable.Local.ToBindingList();
        
        massTable.Columns.Add("chCol", "Channel");
        massTable.Columns["chCol"].ValueType = typeof(uint);
        massTable.Columns["chCol"].DefaultCellStyle.BackColor = Color.Beige;
        massTable.Columns["chCol"].DefaultCellStyle.ForeColor = Color.Purple;
        massTable.ColumnHeadersDefaultCellStyle.Font = new Font(DataGridView.DefaultFont, FontStyle.Bold);
        massTable.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; //?not works

        massTable.DataBindingComplete += onDataBindingComplete;
        massTable.CellValueChanged += onCellValueChanged;
        massTable.CellValidating += onCellValidating;
        massTable.DataError += onDataError;
        massTable.UserDeletingRow += onUserDeletingRow;
        
        tabMassTable.Controls.Add(massTable);
        massTable.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);

        tabMassTable.Resize += new EventHandler(onResize);
    }
    
    private void onResize(object sender, EventArgs e){
        massTable.Width = tabMassTable.Width;
        massTable.Height = tabMassTable.Height - discardBut.Height - 10;
        
        discardBut.Left = 5;
        discardBut.Top  = tabMassTable.Height - discardBut.Height-2;
        
        commitBut.Top  = discardBut.Top;
        commitBut.Left = discardBut.Right + 10;
        
        masstUploadBut.Top = commitBut.Top;
        masstUploadBut.Left = commitBut.Right + 10;
        
        fromdeviceBut.Top = masstUploadBut.Top;
        fromdeviceBut.Left = masstUploadBut.Right + 10;
      
    }

    private void onCommitBut(object sender, EventArgs e){
        App.DBcon.SaveChanges();
        commitBut.Enabled = false;
        tabMassTable.Text = "MassTable";
    }
    
    //2do seems not works
    private void onDiscardBut(object sender, EventArgs e){
        App.DBcon.EnsureCreated(AppConf.DBName);
        App.DBcon.MassTable.Load();
        massTable.DataSource = App.DBcon.MassTable.Local.ToBindingList();
        massTable.Refresh();

        tabMassTable.Text = "MassTable";
        discardBut.Enabled = false;
    }

    //Acync! 
    private void onDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e){

        for (int i = 0; i < massTable.Columns.Count; i++)
            massTable.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

        //App.con("bind");
        isMassTableBinded = true;
        isDB2UI = true;
        UpdateMassChannels();   //update channel column each binding event
        isDB2UI = false; 
    } 

    //also every onBind
    private void onCellValueChanged(object sender,DataGridViewCellEventArgs e){   
        if (e.ColumnIndex>0){
            discardBut.Enabled = true;
            commitBut.Enabled = true;
        }

        //App.con("val_ch:"+e.RowIndex +"."+ e.ColumnIndex);
        //new entered last row
        if (e.RowIndex == massTable.RowCount-1) return;

        if (!isDB2UI) {
            var new_valv = massTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value; 
            string new_vals = (new_valv!=null) ? new_valv.ToString() : "null" ;
            //Console.Write(String.Format("r{0}.c{1}={2}, ", e.RowIndex, e.ColumnIndex, new_vals));//TEST

            string coln = massTable.CurrentCell.OwningColumn.Name;
            string rid = massTable.Rows[e.RowIndex].Cells[1].Value.ToString();
            string amus = (massTable.Rows[e.RowIndex].Cells[3].Value != null) ? 
                          massTable.Rows[e.RowIndex].Cells[3].Value.ToString() : "";
            int    amui = (int) massTable.Rows[e.RowIndex].Cells[4].Value;

            if (e.ColumnIndex==0){
                changedParams += "Amu" + amui + "." + amus + "->ch" + new_vals + "; ";
                tabMassTable.Text = "MassTable *";
                isChangedMassChannels = true;
                }
            else{
                changedParams += ( "id" + rid + ".Amu" + amui + "." + amus +"." + coln + "=" + new_vals + "; ");
                tabMassTable.Text = "MassTable **";
                isChangedMassTables = true;
                }
        }
    }

    private void onUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e){
        discardBut.Enabled = true;
        commitBut.Enabled = true;
        changedParams += ("DelRow=" + e.Row.Cells["Id"].Value.ToString() + "; ");

        //deleted string with value
        var ch = e.Row.Cells["chCol"].Value;
        if (ch != null && ch != ""){
            isChangedMassChannels = true;
            isChangedMassTables = true;
            tabMassTable.Text = "MassTable *";
            }
            else {
                tabMassTable.Text = "MassTable **";
                isChangedMassTables = true;
                }
    }

    //! not works
    private void onUserAddedRow(object sender, DataGridViewRowEventArgs e){
        tabMassTable.Text = "MassTable **";
        discardBut.Enabled = true;
        commitBut.Enabled = true;
        isChangedMassTables = true;
        changedParams += ("AddRow=" + e.Row.Index.ToString() + "; ");
    }

    List<float> dwelist =  [1, 2, 3.5F, 7, 14, 21, 42, 50, 83, 100, 167, 200, 333, 500, 1000, 2000, 5000, 10000];

    private void onCellValidating(object sender, DataGridViewCellValidatingEventArgs evt)
    {   
        var old_val = massTable.Rows[evt.RowIndex].Cells[evt.ColumnIndex].Value;
        var new_val = massTable.Rows[evt.RowIndex].Cells[evt.ColumnIndex].EditedFormattedValue;
        string new_sval = new_val.ToString();
        //App.con( ("old_val " + ((old_val!=null) ? old_val.ToString() : "null") + "->" + new_sval) ); //TEST
        
        //defocus from empty cell
        if (old_val==null && new_sval == "") return;
        //delete old value
        if (old_val!=null && new_val == null) return;
        //same value
        if (old_val!= null && new_val!= null &&old_val.ToString().Equals(new_sval)) return;
        //"" value means erase
        if (new_sval == "") return;
        
        //if dwell and must be at possible list
        if (evt.ColumnIndex == massTable.Columns["Dwell"].Index)
            if (App.StrToFloatZero(new_sval)>0 && 
                dwelist.Contains(App.StrToFloatZero(new_sval)))
                    return;
            else{
                MessageBox.Show(new_sval + " must be "+ 
                    ( String.Join(", ",dwelist) ), "Validate error");
                evt.Cancel = true;
                return;
                }
        
        //only for Channel column
        if (evt.ColumnIndex != massTable.Columns["chCol"].Index) return; 
        
        // between 0...11
        if (Int32.TryParse(new_sval, out int ival))
            if (ival>11){
                MessageBox.Show(" Value " + new_sval+ " > 11", "Overvalue");
                evt.Cancel = true;
            } else if (ival<0){
                MessageBox.Show(" Value " + new_sval+ " < 0", "Undervalue");
                evt.Cancel = true;
                }

        //equal already exist in column
        var rows = massTable.Rows
            .OfType<DataGridViewRow>()
            .Where(x => x.Cells["chCol"].Value != null)
            .Where(x => x.Cells["chCol"].Value.ToString().Equals(new_val))
            .ToArray();
        if (rows == null || rows.Length > 0){
            MessageBox.Show(new_sval + " must be unique", "Validate error");
            evt.Cancel = true;
        }
    }

    private void onDataError(object sender, DataGridViewDataErrorEventArgs err)
    { 
        if ((err.Exception) is ConstraintException) err.ThrowException = false;
        else MessageBox.Show(err.Exception.Message, "Data error " + err.Context.ToString());
    }

    void masstabDB2UI(){
        UpdateMassChannels();
    }

    //2do updated each last row selected
    public void UpdateMassChannels(){

        if (isMassTableBinded == false) return;
        if (curID.Meas==0) return;
        if (tabMassTable.Text == "MassTable *") return;
        //if (massTable.CurrentCell == null) return; //first Form start
        //if (massTable.CurrentCell.RowIndex == massTable.RowCount-2) return; //last-1 change

        var mmc = App.DBcon.Measurements
            .Include(m => m.MassCh)
            .Where(t => t.Id ==  curID.Meas)
            .First();

        if (mmc.MassCh==null){
            Log.Warning("meas id=" +curID.Meas + " dsnt have mass table");
            return;
        }
        List<int?> chList = mmc.MassCh.GetChList();

        for (int i=0; i<massTable.Rows.Count; i++)
            massTable.Rows[i].Cells[0].Value = null;

        int ch=0;
        foreach (var mid in chList){
            if (mid!=null){
                var rw = massTable.Rows
                    .OfType<DataGridViewRow>()
                    .Where (x => (int)x.Cells["Id"].Value == mid )
                    .First();
                if (rw!=null)
                    rw.Cells["chCol"].Value=ch;
                }
            ch++;
        }
    }
        
    private void onDevUploadBut(object sender, EventArgs e){
        masstableUI2DEV();

        //masstableAlarmTable();
        //masstableUI2IntList();
        //masstableUI2List(true);
        //masstableUI2IntList(out List<int> pl);
        //App.con( String.Join(" ", masstableUI2IntList()) );
        //ExtorMassChannels emc =  masstableUI2DB();
        //App.con(masstableOpTime().ToString());
    }

    public ExtorMassChannels masstableUI2DB(){
        
        ExtorMassChannels emc = new ExtorMassChannels();
        emc.Name = "MassCh from"+curID.MassCh;
        
        emc.SetChList(masstableUI2IntList(out List<int> pl));
        
        //foreach(var i in emc.GetChList()) Console.Write(  (i==null)? "nul ": i.ToString() + " "); Console.WriteLine();
        return emc;
    }

    //ret =  11, 3, null,  .... 8, null
    //out =  3,8,11
    public List<int?> masstableUI2IntList(out List<int> plot_list){
        List<int?> ret = new List<int?> 
            ([null,null,null,null,null,null,null,null,null,null,null,null]); //2do bad

        plot_list = [];
        foreach (DataGridViewRow row in massTable.Rows){
            var chc = row.Cells["ChCol"].Value;
            if (chc != null && chc.ToString() != ""){
                int id = (int)row.Cells["Id"].Value;
                int ch = System.Convert.ToInt32(chc);
                if (ch<12) ret[ch] = id;
                    if(Convert.ToBoolean(row.Cells["Enable"].Value))
                        plot_list.Add((int)ch);
            }           
        }
        plot_list.Sort();
        
        return ret;
    }

    //channel:11:amu:998:dwell:42:enabled:1
    public void masstableUI2DEV(){

        sendToQpBox("clearChannels");
        Thread.Sleep(250);
        
        foreach (DataGridViewRow row in massTable.Rows){
            var chc = row.Cells["ChCol"].Value;
            if (chc != null && chc.ToString() != ""){
                string cmd = "channel:" + chc.ToString() + 
                    ":amu:"+ row.Cells["Amu"].Value.ToString() +
                    ":dwell:"+ row.Cells["Dwell"].Value.ToString() +
                    ":enabled:"+ ( Convert.ToBoolean(row.Cells["Enable"].Value) ? "1":"0");
                serialQueTX.Enqueue(cmd);
            }
        }
    }

    //ch3-18-H2O Water, 3:18
    //see Plot.OnPaint
    //2DO not works on start cause MassTableGrid not binded yet
    public List<string> masstableUI2List(bool verbose = false){
        List<string> ret = new List<string>();
        
        if (isMassTableBinded == false) return ret;
        foreach (DataGridViewRow row in massTable.Rows){
            var chc = row.Cells["ChCol"].Value;
            if (chc != null && chc.ToString() != "" && 
                Convert.ToBoolean(row.Cells["Enable"].Value) )
                    if (verbose)
                        ret.Add ("ch"+chc.ToString() + "-" + row.Cells["Amu"].Value.ToString() +
                            "-" + row.Cells["Name"].Value.ToString());
                    else 
                        ret.Add (chc.ToString() + ":" + row.Cells["Amu"].Value.ToString());
        }
        return ret;
    }

    //float seconds to collect all mass (+2sec in real)
    public float masstableOpTime(){
        float ret = 0;
        foreach (DataGridViewRow row in massTable.Rows){
            var chc = row.Cells["ChCol"].Value;
            if (chc != null && chc.ToString() != "" && 
                Convert.ToBoolean(row.Cells["Enable"].Value) )
                    ret+= (float)(row.Cells["Dwell"].Value)/1000 ;       
        }
        return ret;
    }

    //<Ch,LoLoHihi>
    public List<List<float?>> masstableAlarmTable(){
           List<List<float?>> ret1 = [];

        IFormatProvider ip = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        List<int> plot_list = [];
        foreach (DataGridViewRow row in massTable.Rows){
            var chc = row.Cells["ChCol"].Value;
            if (chc != null && chc.ToString() != "" && 
                Convert.ToBoolean(row.Cells["Enable"].Value) ){
                    plot_list.Add(System.Convert.ToInt32(chc));
                    ret1.Add(
                        new List<float?>{
                            float.Parse( row.Cells["ChCol"].Value.ToString(), ip),
                            (float)row.Index,

                            (row.Cells["LoAlarm"].Value == null || row.Cells["LoAlarm"].Value == "") 
                                 ? null : float.Parse( row.Cells["LoAlarm"].Value.ToString(), ip),

                            (row.Cells["LoWarn"].Value == null || row.Cells["LoWarn"].Value == "") 
                                 ? null : float.Parse( row.Cells["LoWarn"].Value.ToString(), ip),

                            (row.Cells["HiWarn"].Value == null || row.Cells["HiWarn"].Value == "") 
                                 ? null : float.Parse( row.Cells["HiWarn"].Value.ToString(), ip),

                            (row.Cells["HiAlarm"].Value == null || row.Cells["HiAlarm"].Value == "") 
                                 ? null : float.Parse( row.Cells["HiAlarm"].Value.ToString(), ip),
                            0,0,0,0 //alarm flags
                            });
                    }
        }
        //sorted by channel as trend data flow
        plot_list.Sort();
        List<List<float?>> ret2 = [];
        foreach (int c in plot_list){
            List<float?> lf = ret1.Where(x => x[0] == c).First();
            ret2.Add(lf);
        }

        return ret2;
    }
}
