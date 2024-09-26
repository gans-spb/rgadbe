/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Forms - External console, Help and About
 */

using System.Diagnostics;

//Auxulary external copy of console
class FormAuxConsole : Form{
    
    TextBox consoleAuxBox = new TextBox();

    public void AppendText(string s){
        consoleAuxBox.AppendText(DateTime.Now.ToString("HH:mm:ss ") + s);
    }
    
    public FormAuxConsole(Form owner){
        Owner = owner;

        Text = "Auxalury Console";
        Width  = AppConst.FormWidth/2;
        Height = owner.Height;
        FormBorderStyle = FormBorderStyle.Sizable;
        Icon = AppConf.MainIcon;

        WindowState = FormWindowState.Normal;
        StartPosition = FormStartPosition.Manual;
        Point newloc = Owner.Location;
        newloc.X += Owner.Width-30; //slightly slide for full-screen mode
        newloc.Y += 30;
        Location =  newloc;

        consoleAuxBox.Name = "console";
        consoleAuxBox.Font = new Font("Courier New", 8.25f);
        consoleAuxBox.Multiline = true;
        consoleAuxBox.WordWrap = false;
        consoleAuxBox.ReadOnly = true;
        consoleAuxBox.BackColor = Color.Ivory;
        consoleAuxBox.ScrollBars = ScrollBars.Both;
        
        consoleAuxBox.Dock = DockStyle.Fill;

        Controls.Add(consoleAuxBox);

        Show();
    }
}

//About window with doc link
class FormAbout : Form{

    TextBox textBox = new TextBox();
    PictureBox picBox = new PictureBox();
    
    public FormAbout(){
        
        Text = "RGA DB edition help";
        Width  = 450;
        Height = 180+40;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        Icon = AppConf.MainIcon;
        BackColor = Color.White;

        picBox.Image = AppConf.AboutPic;
        picBox.SizeMode = PictureBoxSizeMode.AutoSize;
        picBox.Dock = DockStyle.Left;
        Controls.Add(picBox);

        Label tLabel1 = new Label();
        tLabel1.Location = new Point(190, 10);
        tLabel1.AutoSize = true;
        tLabel1.Font = new Font(SystemFonts.DialogFont.FontFamily, 11.5f, FontStyle.Bold);
        tLabel1.ForeColor = Color.Firebrick;
        tLabel1.Text = "Extorr Residual Gaz Analyzer";
        Controls.Add(tLabel1);

        Label tLabel2 = new Label();
        tLabel2.Location = new Point(190, 35);
        tLabel2.AutoSize = true;
        tLabel2.Font = new Font(SystemFonts.DialogFont.FontFamily, 9);
        tLabel2.Text = "DataBase edition software " + AppConst.Ver;
        Controls.Add(tLabel2);

        Label tLabel3 = new Label();
        tLabel3.Location = new Point(190, 55);
        tLabel3.AutoSize = true;
        tLabel3.Font = new Font(SystemFonts.DialogFont.FontFamily, 9);
        tLabel3.Text = "Ioffe inst., Russia. 2024";
        Controls.Add(tLabel3);

        Label tLabel4 = new Label();
        tLabel4.Location = new Point(190, 75);
        tLabel4.AutoSize = true;
        tLabel4.Font = new Font(SystemFonts.DialogFont.FontFamily, 9);
        tLabel4.ForeColor = Color.DarkGreen;
        tLabel4.Text = "ITER DTS 55.C4";
        Controls.Add(tLabel4);
        
        LinkLabel lLabel1 = new LinkLabel();
        lLabel1.Location = new Point(190, 105);
        lLabel1.AutoSize = true;
        lLabel1.Text = "RGA original API manual (eng)";
        lLabel1.LinkClicked += (o,s)=>{
                Process.Start(new ProcessStartInfo("Extorr ASCII Firmware V0.12.pdf") { UseShellExecute = true });};
        Controls.Add(lLabel1);

        LinkLabel lLabel2 = new LinkLabel();
        lLabel2.Location = new Point(190, 130);
        lLabel2.AutoSize = true;
        lLabel2.Text = "RGA DB edtition manual (rus)";
        lLabel2.LinkClicked += (o,s)=>{
                Process.Start(new ProcessStartInfo("RGA DBE manual.pdf") { UseShellExecute = true });};
        Controls.Add(lLabel2);

        LinkLabel lLabel3 = new LinkLabel();
        lLabel3.Location = new Point(190, 155);
        lLabel3.AutoSize = true;
        lLabel3.Text = "Githib Gans-Spb";
        lLabel3.LinkClicked += (o,s)=>{
                Process.Start(new ProcessStartInfo("https://github.com/gans-spb") { UseShellExecute = true });};
        Controls.Add(lLabel3);

        Show();
    }
}
