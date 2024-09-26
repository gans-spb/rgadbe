/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr and Ioffe inst., Igor Bocharov
 * Plot GUI Control
 */


using System.Drawing.Drawing2D;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// This is our own implementation of the graphing
// facility, which does not use any third-party
// graphing library.

class InstrumentGraph : Control
{
    PlotSurface plot;

        // Specifies placement of the PlotSurface within the InstrumentGraph
        // control.  The following are margins around the PlotSurface.
    int topMargin = 30;
    int leftMargin = 60;
    int rightMargin = 20;
    int bottomMargin = 40;
            
    Brush textBrush;
    Font titleFont;
    Font axisFont;

    public void SetTraceNames (List<string> ls)
        => plot.SetTraceNames(ls);

    public double MinimumY
    {
        get { return plot.MinimumY; }
        set {
            plot.MinimumY = value;
            updateGrid();
        }
    }

    public double MaximumY
    {
        get { return plot.MaximumY; }
        set {
            plot.MaximumY = value;
        }
    }


    public double YScaleFactor
    {
        get { return plot.YScaleFactor; }
        set { plot.YScaleFactor = value; Refresh(); }
    }


    void updateGrid()
    {
        plot.getGridX(out gridX);
        plot.getGridY(out gridY);
        Refresh();
    }

    public void SetRangeY(double minY, double maxY)
    {
        plot.MinimumY = minY;
        plot.MaximumY = maxY;
        updateGrid();
    }

    GridLines gridX, gridY;

    public void SetRangeX(double lowX, double highX)
    {
        //plot.SetRangeX(lowX, highX); //ClearGraph() here
        plot.MinimumX = lowX;
        plot.MaximumX = highX;
        updateGrid();
    }

    
    //view limit from measurement tack
    public void SetRangeXlimit(double lowX, double highX){
        //App.con(highX.ToString());
        plot.minXlim  = lowX;
        plot.maxXlim  = highX;
        plot.MinimumX = lowX;
        plot.MaximumX = highX;
        updateGrid();
    }


    void onInstrumentResize(object sender, EventArgs e)
    {
        plot.Width = Width - rightMargin - leftMargin; 
        plot.Height = Height - topMargin - bottomMargin;
        updateGrid();
    }

    public InstrumentGraph()
    {
        Size = new Size(740, 400);
        DoubleBuffered = true;
        TabStop = false;
        BackColor = Color.FromArgb(64, 64, 64);
        plot = new PlotSurface();

        plot.Width -= (leftMargin + rightMargin);
        plot.Height -= (topMargin + bottomMargin);
        plot.Left += leftMargin;
        plot.Top += topMargin;

        MinimumY = -3e-13;
        MaximumY = 3 * 2.5e-12;
        Controls.Add(plot);

        textBrush = new SolidBrush(Color.White);
        titleFont = new Font( "Arial", 14);
        axisFont = new Font( "Arial", 8);

        Resize += new EventHandler(onInstrumentResize);

        plot.mouseResize = MousePozWheel;
    }

    
    //Graph mouse wheel and buts handler   

    public int yScale = 1;  //get from FormTab
    public int xScale = 100;
    int xOld;
    int yOld;
    
    public void MousePozWheel(string mpoz){
        
        Keys mkeyz = ModifierKeys;
        string[] mpozf = mpoz.Split(" ");
        //App.con("mouseZoom " + mpoz + " m." + mkeyz.ToString()); //TEST
        if (mpozf.Length<3) return;

        const int step = 5;    //wheel sens
        int d=0;
        if (mpozf[2] == "Up") d=+step;
        if (mpozf[2] == "Dw") d=-step;
        int xPoz = int.Parse(mpozf[0]);
        int yPoz = int.Parse(mpozf[1]);

        //zoom
        if (d!=0){
            //Y zoom
            if (mkeyz == Keys.Control || 
                mkeyz == Keys.None ){
                yScale += d;
                if (yScale>=0 && yScale<100){

                    double fyscale = yScale/100.0;
                    double zoomp = fyscale * (AppConst.loGraphP - AppConst.hiGraphP) + AppConst.hiGraphP;

                    double diap = Math.Pow(10, zoomp);
                    double yp   = (plot.MaximumY-plot.MinimumY) * yPoz/100.0 + plot.MinimumY;

                    double bp = yp - diap/2;
                    double tp = yp + diap/2;

                    if (yp-diap/2 < - diap/10){
                        bp = -diap/10;
                        tp = bp+diap;
                    }else
                    if (yp+diap/2 > Math.Pow(10, AppConst.hiGraphP)){
                        bp = 0;
                        tp = Math.Pow(10, AppConst.hiGraphP);
                    }

                    SetRangeY(bp, tp);
                    //App.con(yp +" "+ diap + ", " +bp+"-"+tp +" zp"+zoomp + " fys" +fyscale); //TEST
                }
                else if (yScale<0) yScale=0;
                else if (yScale>100) yScale=100;
            }
            
            //X zoom
            if (mkeyz == Keys.Shift || 
                mkeyz == Keys.None ){
                xScale -= d;
                if (xScale>0 && xScale<=100){
               
                    int diap = (int) (plot.maxXlim * (xScale/100.0));
                    int xp   = (int) ((plot.MaximumX-plot.MinimumX) * xPoz/100.0 + plot.MinimumX);

                    int pmx = (int)plot.maxXlim;
                    
                    int lp = xp - diap/2;
                    int rp = xp + diap/2;
                    
                    if (xp-diap/2 < 0){
                        lp = 0;
                        rp = lp + diap;
                    } else 
                    if (xp+diap/2 > pmx){
                        rp =  pmx;
                        lp = rp - diap;
                    }

                    SetRangeX( lp, rp);
                    //Console.WriteLine(xp +" "+ diap + ", " +lp+"-"+rp); //TEST
                } 
                else if (xScale<0) xScale=0;
                else if (xScale>100) xScale=100;
            } 
            
            //Line thickness
            if (mkeyz.ToString() == "Alt"){
               double w = plot.PenWidth + d/20.0;
               if (w<1.0) w=1.0;
               if (w>5.0) w=5.0;
               plot.PenWidth = (float)w;
               Refresh();
            }
        }
        
        //press but
        if (mpozf[2] == "MiceDw"){
            
            //start moving
            if (mpozf[3] == "Left"){
                xOld = xPoz;
                yOld = yPoz;
            }

            //drop zoom parameters
            if ( mpozf[3] == "Right")
                DropMousePoz();
        }
        
        //stop moving
        if (mpozf[2] == "MiceUp" && mpozf[3] == "Left"){
            int dx = xOld-xPoz;
            int dy = yOld-yPoz;

            double dxplot = (plot.MaximumX - plot.MinimumX) * dx/100;
            plot.MinimumX += dxplot;
            plot.MaximumX += dxplot;

            double dyplot = (plot.MaximumY - plot.MinimumY)* dy/100;
            plot.MinimumY += dyplot;
            plot.MaximumY += dyplot;

            updateGrid();
        }
        
    }

    public void DropMousePoz(){
        double ydiap = Math.Pow(10, AppConst.hiGraphP);
        SetRangeY(-ydiap/10, ydiap);

        SetRangeX(plot.minXlim, plot.maxXlim);
        xScale = 100;
        yScale = 1;
    }

    string graphTitle = "";
    string xAxisTitle = "";
    string yAxisTitle = "";

    public string YAxisTitle
    {
        get { return yAxisTitle; }
        set { yAxisTitle = value; Refresh(); }
    }

    public string XAxisTitle
    {
        get { return xAxisTitle; }
        set { xAxisTitle = value; Refresh(); }
    }

    public string GraphTitle
    {
        get { return graphTitle; }
        set { graphTitle = value; Refresh(); }
    }


    protected override void OnPaint( PaintEventArgs e )
    {
        Graphics g = e.Graphics;
        
        // Draw the graph title.
        SizeF titleSize = g.MeasureString(graphTitle, titleFont);
        float left = leftMargin + (plot.Width - titleSize.Width) / 2;
        float top = (topMargin - titleSize.Height) / 2;
        g.DrawString(graphTitle, titleFont, textBrush, left, top);

        // Draw the x-axis title.
        SizeF size = g.MeasureString(xAxisTitle, axisFont);
        left = leftMargin + (plot.Width - size.Width) / 2;
        top = Height - 1.5f * size.Height;
        g.DrawString(xAxisTitle, axisFont, textBrush, left, top); 

        // Draw the y-axis title.
        size = g.MeasureString(yAxisTitle, axisFont);
        float deg = -90.0f;
//        g.TranslateTransform(size.Height, topMargin + plot.Height - size.Width );
        g.TranslateTransform( size.Height / 2, topMargin + (plot.Height + size.Width) / 2);

        g.RotateTransform(deg);
        g.DrawString(yAxisTitle, axisFont, textBrush, 0, 0);
        g.ResetTransform();

        // Draw the x-axis labels.
        for (int i = 0; i < gridX.number; i++){
            float y = plot.Bottom + 2;
            float x = plot.Left + (float) (gridX.firstCoord + (i *  gridX.deltaCoord) );
            double val = gridX.firstValue + (i * gridX.deltaValue);
            string s = String.Format("{0}", val );
            SizeF labelSize = g.MeasureString(s, axisFont);
            x -= labelSize.Width / 2;
            g.DrawString(s, axisFont, textBrush, x, y);
        }

        // Draw the y-axis labels.
        for (int i = 0; i < gridY.number; i++){
            double val = gridY.firstValue + (i * gridY.deltaValue);
            //string s = String.Format("{0}", val );
            string s = val.ToString("G4");
            SizeF labelSize = g.MeasureString(s, axisFont);

            float x = plot.Left - labelSize.Width;
            float y = plot.Top + (float) (gridY.firstCoord + (i *  gridY.deltaCoord) );
            y -= labelSize.Height / 2;
            g.DrawString(s, axisFont, textBrush, x, y);
        }

        //Pen pen = new Pen(Color.MediumAquamarine);
        //g.DrawLine(pen, new PointF(0.0f, 0.0f), new PointF(15.0f, 15.0f));
    }


    public void ClearGraph()
    {
        plot.ClearGraph();
    }
    
    public void ClearTrace(int tracenum)
    {
        plot.ClearTrace(tracenum);
    }


    //Plot draw point
    public void DataPoint(int tracenum, int index, double x, double y)
    {
#if false
        // for testing
        double dy = 0.3e-12;
        for (int t = 0; t < 12; t++)
            plot.DataPoint(t, index, x, t * dy + y);
#else
        plot.DataPoint(tracenum, index, x, y);
#endif
    }
} //InstrumentGraph

struct GridLines
{
    public float firstCoord;
    public float deltaCoord;
    public double firstValue;
    public double deltaValue;
    public int number;
}

class PlotSurface : Panel
{
    public const int numberOfTraces = 12;

    Pen[] tracePen;
    String[] traceName;
    Pen gridPen;

    public void SetTraceNames (List<string> ls){

        for (int i=0; i<numberOfTraces; i++)
            if (i<ls.Count)
                traceName[i] = ls[i];
            else 
                traceName[i] = null;
    }

    public Action<string> mouseResize;

    public PlotSurface()
    {
        DoubleBuffered = true;
        BorderStyle = BorderStyle.Fixed3D;

        Size = new Size(740, 400);
        TabStop = false;
        BackColor = Color.Black;
        
        tracePen = new Pen[numberOfTraces];
        traceName = new string[numberOfTraces];
        
        tracePen[0] = new Pen(Color.Yellow);
        tracePen[1] = new Pen(Color.LimeGreen);
        tracePen[2] = new Pen(Color.Cyan);
        tracePen[3] = new Pen(Color.DodgerBlue);
        tracePen[4] = new Pen(Color.Violet);
        tracePen[5] = new Pen(Color.OrangeRed);
        tracePen[6] = new Pen(Color.Chartreuse);
        tracePen[7] = new Pen(Color.DarkSeaGreen);
        tracePen[8] = new Pen(Color.Azure);
        tracePen[9] = new Pen(Color.BlueViolet);
        tracePen[10] = new Pen(Color.Salmon);
        tracePen[11] = new Pen(Color.Orange);

        gridPen = new Pen(Color.DarkOliveGreen);
        gridPen.DashStyle = DashStyle.Dash; // DashStyle.Dot;

        traces = new Datum[numberOfTraces][];
        for (int i = 0; i < numberOfTraces; i++){
            traces[i] = new Datum[0];
        }
        
        this.MouseHover += (o,e)=>{
            //App.con("m-hover");
        };

        this.MouseWheel += (o,e)=>{
            mouseResize( EvtToPoz(e) + ((e.Delta>0)? "Up" : "Dw"));
        };
        
        this.MouseClick  += (o,e)=>{
            mouseResize( EvtToPoz(e) + "MiceUp " + e.Button );
        };
        
        this.MouseDown  += (o,e)=>{
            mouseResize( EvtToPoz(e) + "MiceDw " + e.Button );
        };


        this.MouseMove += (o,e)=>{
            MouseEventArgs mea = (MouseEventArgs)e;
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                ;//mouseResize( EvtToPoz(e) + "Move" );
        };


        ClearGraph();
    }

    string EvtToPoz (MouseEventArgs e){
        MouseEventArgs mea = (MouseEventArgs)e;
        int xpos  = 100 - 100*(Width -mea.X)/Width;
        int ypos  =       100*(Height-mea.Y)/Height;
        return xpos.ToString() + " " +ypos.ToString() + " ";
    }

    float penWidth = 1;

    public float PenWidth{
        set {
            penWidth = value;
            for (int i=0; i<numberOfTraces; i++)
                    tracePen[i].Width = value;
            }
        get => penWidth;
    }
    
    double minimumY = 0, maximumY = 100;

    public double MinimumY
    {
        get { return minimumY; }
        set { minimumY = value; }
    }

    public double MaximumY
    {
        get { return maximumY; }
        set { maximumY = value; }
    }

    double yScaleFactor = 1.0;

    public double YScaleFactor
    {
        get { return yScaleFactor; }
        set { yScaleFactor = value; }
    }

    //current zoomde view limit
    double minimumX = 1;
    double maximumX = 100;
    
    //total limit from measurement
    public double minXlim = 1;
    public double maxXlim = 100;

    public double MinimumX
    {
        get { return minimumX; }
        set { minimumX = value; }
    }

    public double MaximumX
    {
        get { return maximumX; }
        set { maximumX = value; }
    }

    float bound(float r)
    {
        if (r < -100000)
            r = -100000;
        else if (r > 100000)
            r = 100000;
        return r;
    }

    public float toGdiX(double x)
    {
        float r = (float) ( Width * (x - minimumX) / (maximumX - minimumX) );
        r = bound(r);
        return r;
    }

    public float toGdiY(double y)
    {
        float r = (float) (Height - Height * (y - minimumY) / (maximumY - minimumY) );
        r = bound(r);
        return r;
    }


    //Chart line created here by DrawLine, call from onPaint event, call from Refresh
    void renderTrace(Graphics g, int tracenum)
    {
        g.SmoothingMode = SmoothingMode.None;  // SmoothingMode.AntiAlias;

        Datum last = new Datum(double.NaN, double.NaN);

        foreach (Datum pt in traces[tracenum]){
            if ( (double.IsNaN(pt.X) == false) &&  (double.IsNaN(last.X) == false)  )
                g.DrawLine(tracePen[tracenum], toGdiX(last.X), toGdiY(yScaleFactor * last.Y),
                                      toGdiX(pt.X), toGdiY(yScaleFactor * pt.Y));
            last = pt;
        }
    }

    // Returns a reasonable grid spacing based on the range of data to
    // be plotted. The spacing will always be either 1*10^x, 2*10^x, or
    // 5*10^x, where x is some integer.  It attempts to choose the
    // increment so there are approximately 8 grid points in the range
    // using: range/(n*10^x)~=8 where n is 1, 2, or 5.
    double gridSpacing(double range)
    {
        int gridPoints = 8;
        double x1 = Math.Log10(range / gridPoints);
        double x2 = Math.Log10(range / (2 * gridPoints));
        double x5 = Math.Log10(range / (5 * gridPoints));
        double xBest;
        int n;
        if (Math.Abs(x1 - Math.Round(x1)) < Math.Abs(x2 - Math.Round(x2))) {
            xBest = x1;
            n = 1;
        } else {
            xBest = x2;
            n = 2;
        }
        if (Math.Abs(xBest - Math.Round(xBest)) > Math.Abs(x5 - Math.Round(x5))) {
            xBest = x5;
            n = 5;
        }
        xBest = Math.Round(xBest);
        return n * Math.Pow(10.0, xBest);
    }

    // Compute point where first grid line will be drawn (greater than
    // or equal to leftOrBottom). Based on the fact that our choice of
    // spacing in gridSpacing will always produce a grid line at
    // zero (whether or not such line is actually drawn).
    double firstGridPoint(double leftOrBottom, double spacing)
    {
        // -offset to include points that are very close to meeting the criterion
        double nFirst = Math.Ceiling(leftOrBottom / spacing - .000001f);
        //             return (float)Math.Round(nFirst*spacing, 2);
        return nFirst * spacing;
    }




    void autoGrid(out double spacing, out double first, double min, double max)
    {
        spacing = gridSpacing(max - min);
        first = firstGridPoint(min, spacing);
    }


    public void getGridX(out GridLines g)
    {
        double spacing, first;
        autoGrid(out spacing, out first, minimumX, maximumX);

        g.firstCoord = (float) toGdiX(first);
        g.deltaCoord = toGdiX(first + spacing) - toGdiX(first);
        g.firstValue = first;
        g.deltaValue = spacing;
        g.number = (int) ((maximumX - first) / spacing) + 1;
    }

    public void getGridY(out GridLines g)
    {
        double spacing, first;
        autoGrid(out spacing, out first, minimumY, maximumY);

        g.firstCoord = (float) toGdiY(first);
        g.deltaCoord = toGdiY(first + spacing) - toGdiY(first);
        g.firstValue = first;
        g.deltaValue = spacing;
        g.number = (int) ((maximumY - first) / spacing) + 1;
    }


    void renderGrid(Graphics g)
    {
        double spacing, first;

        autoGrid(out spacing, out first, minimumX, maximumX);
        for (double x = first; x <= maximumX; x += spacing){
            float gdiX = toGdiX(x);
            g.DrawLine(gridPen, gdiX, 0, gdiX, Height);
        }

        int l=0;
        autoGrid(out spacing, out first, minimumY, maximumY);
        for (double y = first; y <= maximumY; y += spacing){
            float gdiY = toGdiY(y);
            g.DrawLine(gridPen, 0, gdiY, Width, gdiY);
            if (l++ >100) break;    //2DO break if spacing to low
        }
    }

    
    //Call draw chart (via renderTrace)
    protected override void OnPaint( PaintEventArgs e )
    {
        e.Graphics.SmoothingMode = SmoothingMode.None; // SmoothingMode.AntiAlias;
        
        Graphics g = e.Graphics;
        renderGrid(g);
        for (int t = 0; t < numberOfTraces; t++)
            renderTrace(g, t);

        //Trace masstable name "ch3-amu998-Pirani Pressure"
        for (int t=0; t<PlotSurface.numberOfTraces; t++){
            if ( traceName[t] != null && traceName[t] != ""){
                Color c;

                if (traceName[t].Split("-").Count() > 1){
                int ch = int.Parse( 
                    System.Text.RegularExpressions.Regex.Match(traceName[t].Split("-")[0], @"\d+").Value);
                    //App.con(traceName[t] + ch.ToString());
                c = tracePen[ch].Color;
                }
                else 
                    c = tracePen[t].Color;

                g.DrawString("--- " + traceName[t],
                    new ( "Arial", 10),
                    new SolidBrush(c),
                    Width-200, 10+17*t);
            }
        }
    }


    public void ClearTrace(int tracenum)
    {
        if (tracenum >= numberOfTraces)
            return;
        traces[tracenum] = new Datum[0];
    }

    public void ClearGraph()
    {
        for (int t = 0; t < numberOfTraces; t++)
            ClearTrace(t);
    }

    struct Datum {
        public double X;
        public double Y;
        public Datum(double x, double y){
            X = x;
            Y = y;
        }            
    }

    Datum[][] traces;

    public void DataPoint(int tracenum, int index, double x, double y)
    {
        if ( (tracenum < 0) || (tracenum >= numberOfTraces) )
            return;
        Datum[] vertices = traces[tracenum];
        int oldsize = vertices.Length;
        if (index >= oldsize){
            int newsize = index + 100;
            Array.Resize(ref vertices, newsize);
            traces[tracenum] = vertices;
            for (int j = oldsize; j < newsize; j++){
                vertices[j] = new Datum(double.NaN, double.NaN);
            }
        }
        vertices[index] = new Datum(x, y);
    }

    public void SetRangeX(double lowX, double highX)
    {
        if ( (lowX != minimumX) || (highX != maximumX)  ){
            ClearGraph();
        }
        minimumX = lowX;
        maximumX = highX;
    }
}
