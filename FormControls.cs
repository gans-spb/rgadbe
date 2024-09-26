/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Custom WinForm controls
 */

//Base WinForms control class
class NumBoxBase : Control
{
    Label leftLabel;
    Label rightLabel;

    public NumBoxBase(String lhs, String rhs)
    {
        leftLabel = new Label();
        leftLabel.AutoSize = true;
        leftLabel.Text = lhs;

        rightLabel = new Label();
        rightLabel.AutoSize = true;
        rightLabel.Text = rhs;

        TabStop = false;
    }
    
    public void SetLabelColor(Color c){
        leftLabel.ForeColor = c;
    }

    protected Control rodata;
    protected void setDataControl(Control ctl) {rodata = ctl;}

    string sval; //main value
    public string Value { get => this.sval;  set => this.sval = value; }
    protected bool userSet = true;
    
    public event StringEventHandler ValueChanged;
    protected void RaiseEvent(string val)
    {
        sval = val;
        if (userSet && (ValueChanged != null))
            ValueChanged(this, new StringEventArgs(val));  //=> control value changed
    }

    protected virtual void doSetValue(double v)
    {
        userSet = false;
        rodata.Text = String.Format("{0:0.000}", v);
        sval = rodata.Text;
        userSet = true;
    }

    protected virtual void doSetValue(int v)
    {
        userSet = false;
        rodata.Text = String.Format("{0}", v);
        sval = rodata.Text;
        userSet = true;
    }

    protected virtual void doSetValue(string v)
    {
        userSet = false;
        rodata.Text = v;
        sval = rodata.Text;
        userSet = true;
    }

    public void setValue(double v)
    {
        if (InvokeRequired){
            EventHandler ev = new EventHandler(
                delegate (object o, EventArgs a){
                    doSetValue(v);
                }
            ); 
            this.BeginInvoke(ev);
        } else {
            doSetValue(v);
        }
    }

    public void setValue(int v)
    {
        if (InvokeRequired){
            EventHandler ev = new EventHandler(
                delegate (object o, EventArgs a){
                    doSetValue(v);
                }
            ); 
            this.BeginInvoke(ev);
        } else {
            doSetValue(v);
        }
    }

    public void setValue(string v)
    {
        if (InvokeRequired){
            EventHandler ev = new EventHandler(
                delegate (object o, EventArgs a){
                    doSetValue(v);
                }
            ); 
            this.BeginInvoke(ev);
        } else {
            doSetValue(v);
        }
    }

    public void Disable()
    {
        rodata.Enabled = false;
    }

    public void Enable()
    {
        rodata.Enabled = true;
    }

    public new bool Enabled {
        get => rodata.Enabled;
        set => rodata.Enabled = value;
    }

    public void setFocus()
    {
        rodata.Focus();
    }

    protected override void OnMove( EventArgs e )
    {
        Parent.Controls.Add(leftLabel);
        leftLabel.Location = Location;
        leftLabel.Location -= new Size(leftLabel.Width, -3);

        Parent.Controls.Add(rodata);
        rodata.Location = Location;

        Parent.Controls.Add(rightLabel);
        rightLabel.Location = rodata.Location;
        rightLabel.Location += new Size(rodata.Width + 3, 3);
    }
}

//-----------------------------------------------------------------------------
//Custom Controls from Extorr programmer
//Every control must set NumBoxBase.sval by Value = data.Text; 

class NumericUpDownNoBeep : NumericUpDown
{
    protected override bool ProcessDialogKey(Keys keyData)
    {
        if (keyData == Keys.Return){
            SendKeys.Send("{TAB}");
            return true;
        }
        // return the key to the base class if not used.
        return base.ProcessDialogKey(keyData);
    }
}

class TextBoxNoBeep : TextBox
{
    void onKeyPress( object sender, KeyPressEventArgs e )
    {
        switch (e.KeyChar)
        {
            case '\r':
                // perform necessary action
                e.Handled = true;
                break;
        }
    }

    public TextBoxNoBeep()
    {
        KeyPress += new KeyPressEventHandler(onKeyPress);
    }
}

class OutputNumBox : NumBoxBase
{
    TextBox data;
    
    public OutputNumBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new TextBox();
        data.Width = 60;
        data.TextAlign = HorizontalAlignment.Right;
        data.ReadOnly = true;
        data.Text = "0.000";
        setDataControl(data);
    }
}

class OutputNumBoxSci : OutputNumBox
{
    protected override void doSetValue(double v)
    {
        userSet = false;
        rodata.Text = String.Format("{0:e2}", v);
        Value = rodata.Text;
        userSet = true;
    }

    public OutputNumBoxSci(String lhs, String rhs) : base(lhs, rhs){}
}

class InputNumBox : NumBoxBase
{
    NumericUpDownNoBeep data;

    public InputNumBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new NumericUpDownNoBeep();
        data.Width = 60;
        data.TextAlign = HorizontalAlignment.Right;
        data.Minimum = -10000;
        data.Maximum = 10000;
        data.Text = "0.000";
        data.DecimalPlaces = 3;
        data.BackColor = Color.PaleGreen;
        setDataControl(data);
        data.ValueChanged += new EventHandler(onValueChanged);
    }

    private void onValueChanged(object sender, EventArgs e)
    {
        RaiseEvent(data.Value.ToString());
    }
}

class InputIntNumBox : NumBoxBase
{
    NumericUpDownNoBeep data;
    
    public int Minimum{
        set => data.Minimum = value;}

    public int Increment{
        set => data.Increment = value;}

    public Color BackColor{
        set => data.BackColor = value;
    }

    public InputIntNumBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new NumericUpDownNoBeep();
        data.Width = 60; 
        data.TextAlign = HorizontalAlignment.Right;
        data.Minimum = -5000000000000;
        data.Maximum = 5000000000000;
        data.Text = "0";
        data.DecimalPlaces = 0;
        data.BackColor = Color.PaleGreen;
        setDataControl(data);
        data.ValueChanged += new EventHandler(onValueChanged);
    }

    public void SetWidth(int w) => data.Width = w;

    private void onValueChanged(object sender, EventArgs e)
    {
        //App.con(sender.GetType().ToString());
        RaiseEvent(data.Value.ToString());
    }
}


class QpCheckBox : NumBoxBase
{
    CheckBox data;

    public QpCheckBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new CheckBox();
        data.Text = ""; 
        data.CheckedChanged += new EventHandler(onCheckedChanged);
        Value = "0"; //default
        setDataControl(data);
    }
    
    void onCheckedChanged(object sender, EventArgs e)
    {
        bool checkedState = ((CheckBox) sender).Checked;
        if (checkedState == true)
            {RaiseEvent("1"); Value = "1"; }
        else
            {RaiseEvent("0"); Value = "0"; }
    }

    protected override void doSetValue(int v)
    {
        userSet = false;
        data.Checked = (v != 0);
        userSet = true;
    }

    public bool Checked {
        get { return data.Checked; }
    }
}

//Port name
class CommPortComboBox : NumBoxBase
{
    ComboBox data;

    void enumeratePorts()
    {
        string[] ports = System.IO.Ports.SerialPort.GetPortNames();
        foreach(string port in ports)
            data.Items.Add(port);
    }

    public CommPortComboBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new ComboBox();
        data.Width = 65;
        data.BackColor = Color.Khaki;
        data.DropDownStyle = ComboBoxStyle.DropDownList;  // DropDown

        data.SelectedIndexChanged += new EventHandler(onSelectionChanged);
        data.DropDown += new EventHandler(onDropDown);

        data.Items.Clear();
        enumeratePorts();

        setDataControl(data);
    }
    
    public void SetDefaultName(string name){
            data.SelectedIndex = data.FindStringExact(name);
    }

    void onSelectionChanged(object sender, EventArgs e){
        RaiseEvent(data.Text);
    }

    void onDropDown(object sender, EventArgs e){
        data.Items.Clear();
        enumeratePorts();
    }
}

//Port speed
class CommSpeedComboBox : NumBoxBase
{
    public ComboBox data;

    public CommSpeedComboBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new ComboBox();
        data.Width = 65;
        data.Items.AddRange(new object[] {"9600", "19200", "38400", "57600", "115200", "230400"});

        data.SelectedIndexChanged += new EventHandler(onSelectionChanged);
        data.DropDownStyle = ComboBoxStyle.DropDownList;  // DropDown

        data.BackColor = Color.Khaki;
        setDataControl(data);
    }
    
    public void SetDefaultSpeed(){
        data.SelectedIndex = data.FindStringExact(AppConst.COMSpeed.ToString());
    }

    void onSelectionChanged(object sender, EventArgs e)
    {
        RaiseEvent(data.Text);
    }
}

class CommEncodingComboBox : NumBoxBase
{
    ComboBox data;

    public CommEncodingComboBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new ComboBox();
        data.Width = 65;
        data.Items.AddRange(new object[] {"Base10", "Base16", "Base64"}
        );

        data.SelectedIndexChanged += new EventHandler(onSelectionChanged);
        data.DropDownStyle = ComboBoxStyle.DropDownList;  // DropDown

        data.BackColor = Color.PaleGreen;
        setDataControl(data);
    }

    void onSelectionChanged(object sender, EventArgs e)
    {
        int v = 0;
        if (data.Text == "Base10")
            v = 10;
        else if (data.Text == "Base16")
            v = 16;
        else if (data.Text == "Base64")
            v = 64;
        RaiseEvent(v.ToString());
    }

    protected override void doSetValue(int v)
    {
        userSet = false;
        rodata.Text = String.Format("Base{0}", v);
        userSet = true;
    }


}

class ScanSpeedComboBox : NumBoxBase
{
    ComboBox data;

    public ScanSpeedComboBox(String lhs, String rhs) : base(lhs, rhs)
    {
        data = new ComboBox();
        data.Width = 60;
        data.Items.AddRange(new object[] {
                        "1000", "500", "288", "144", "72", "48", "24",
                        "20", "12", "10", "6", "5", "3", "2", "1",
                        "0.5", "0.2", "0.1"}
        );

        data.SelectedIndexChanged += new EventHandler(onSelectionChanged);
        data.MaxDropDownItems = 18;
        data.DropDownStyle = ComboBoxStyle.DropDown;  // DropDownList

        data.BackColor = Color.PaleGreen;
        setDataControl(data);
    }

    void onSelectionChanged(object sender, EventArgs e)
    {
        Value = data.Text; 
        RaiseEvent(data.Text);
    }

    protected override void doSetValue(double v)
    {

        userSet = false;
        rodata.Text = String.Format("{0:0.0}", v);
        Value = rodata.Text;
        userSet = true;
    }

}

//DB select Year+Months combo box
class YearMonthComboBox : NumBoxBase
{
    ComboBox cBox;
    
    public int Width{
        get => cBox.Width;
        set => cBox.Width = value;
    }

    void enumerateItems(){
        /*_strings = App.dbc.Measurements.FromSql($"select * from Measurements order by StartTime").
            Select(p => p.StartTime.Year.ToString() + "-" + p.StartTime.Month.ToString()).Distinct().ToList();*/

        var q = App.DBcon.Measurements
            .Select(t => new { t.StartTime.Year, t.StartTime.Month })
            .Distinct()
            .OrderBy(t => t.Year).ThenBy(t => t.Month)
            .ToList() 
            .Select(t => t.Year.ToString() + "-" + t.Month.ToString());

        //Console.WriteLine( String.Join(", ",q));
        cBox.Items.Clear();
        cBox.Items.AddRange(q.ToArray());
    }

    //callback about selected value
    //FormDatabase dateSelBox.cbonSelCh = onSelDateTime;

    public delegate void CbonSelCh(string result);
    public CbonSelCh cbonSelCh;

    public void selectLastRecord(){
        cBox.SelectedIndex = cBox.Items.Count - 1;
        cbonSelCh(cBox.Text);
    }

    void onSelectionChanged(object sender, EventArgs e) {
        RaiseEvent(cBox.Text);
        cbonSelCh(cBox.Text);
    }

    public YearMonthComboBox(String lhs, String rhs) : base(lhs, rhs){
        cBox = new ComboBox();
        cBox.Width = 80;
        cBox.DropDownStyle = ComboBoxStyle.DropDownList;  // DropDown
        cBox.BackColor = Color.LightCyan;

        cBox.SelectedIndexChanged += new EventHandler(onSelectionChanged);
        cBox.DropDown += new EventHandler(onDropDown);

        enumerateItems();
        setDataControl(cBox);
    }

    void onDropDown(object sender, EventArgs e){
        enumerateItems();
    }
}

//DB select measurement list
class MeasComboBox : NumBoxBase
{
    public int Width{
        get => cBox.Width;
        set => cBox.Width = value;
    }

    ComboBox cBox;
    string dt;//"2023-7";
    
    public void SetDT(String _dt) {dt=_dt;}

    void enumerateMeasComboBox(){
        if (dt == null || dt == "") return;
        
        int y = int.Parse(dt.Split("-")[0]);
        int m = int.Parse(dt.Split("-")[1]);

        var q = App.DBcon.Measurements
            .Where(t => t.StartTime.Year  == y)
            .Where(t => t.StartTime.Month == m)
            .OrderBy(t => t.StartTime)
            .ToList() 
            .Select(t => t.Id +"-"+ t.Name);

        cBox.Items.Clear();
        cBox.Items.AddRange(q.ToArray());
    }

    public void selectLastRecord(){
        enumerateMeasComboBox();
        cBox.SelectedIndex = cBox.Items.Count - 1;
    }

    public MeasComboBox(String lhs, String rhs) : base(lhs, rhs){
        cBox = new ComboBox();
        cBox.Width = 110;
        cBox.DropDownStyle = ComboBoxStyle.DropDownList;  // DropDown
        cBox.BackColor = Color.LightCyan;

        cBox.SelectedIndexChanged += new EventHandler(onSelectionChanged);
        cBox.DropDown += new EventHandler(onDropDown);

        setDataControl(cBox);
    }

    void onDropDown(object sender, EventArgs e){
        enumerateMeasComboBox();
    }

    //callback about selected value
    //FormDatabase measSelBox.cbonSelCh = onSelMeas;
    public delegate void CbonSelCh(string result);
    public CbonSelCh cbonSelCh;

    public void onSelectionChanged(object sender, EventArgs e) {
        RaiseEvent(cBox.Text);
        cbonSelCh(cBox.Text);
    }
    
}

/* Control which selects pressure units for the sweep display. */

// in process...
class PressureUnits : GroupBox
{
    RadioButton ampsButton = new RadioButton();
    RadioButton torrButton = new RadioButton();
    RadioButton pascalButton = new RadioButton();

    public event EventHandler AmpsSelected;
    public event EventHandler TorrSelected;
    public event EventHandler PasSelected;

    bool isSetFromDb = false;

    public string Value { 
        get {
                 if (ampsButton.Checked) return "0";
            else if (torrButton.Checked) return "1";
            else if (pascalButton.Checked) return "2";
            else  return "";
        }
        set { 
            isSetFromDb = true;
            switch (value){
            case "0": ampsButton.Checked = true; break;
            case "1": torrButton.Checked = true; break;
            case "2": pascalButton.Checked = true; break;
            isSetFromDb = false;
            }
        }
    }
    
    void onButtonClicked(object sender, EventArgs e)
    {
        RadioButton b = (RadioButton) sender;
        if (b == ampsButton){
            if (AmpsSelected != null)
                AmpsSelected(this, null);
        } else if (b == torrButton){
            if (TorrSelected != null)
                TorrSelected(this, null);
        } else if(b == pascalButton){
            if (PasSelected != null)
                PasSelected(this, null);
        }
        if (isSetFromDb) 
            Log.Debug( "GUI.PressureUnits = " + this.Value);
    }
   
    public PressureUnits()
    {
        //Text = "Units";
        Width = 70;
        Height = 80;

        ampsButton.AutoSize = true;
        ampsButton.Text = "Amps";
        ampsButton.Location = new Point(5, ampsButton.Height - 12);

        torrButton.Location = ampsButton.Location;
        torrButton.AutoSize = true;
        torrButton.Text = "Torr";
        torrButton.Top += ampsButton.Height - 3;

        pascalButton.Location = torrButton.Location;
        pascalButton.AutoSize = true;
        pascalButton.Text = "Pascal";
        pascalButton.Top += torrButton.Height - 3;

        ampsButton.Click += new EventHandler(onButtonClicked);
        torrButton.Click += new EventHandler(onButtonClicked);
        pascalButton.Click += new EventHandler(onButtonClicked);

        Controls.Add(ampsButton);
        Controls.Add(torrButton);
        Controls.Add(pascalButton);

        ampsButton.PerformClick();
    }
}
