/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Extorr, Ioffe
 * Plot serial data parcer and quie
 */

using System.IO.Ports;
using System.Threading;

partial class RgaWindow : Form
{
    public enum MeasMode {Monitor, Sweep, Trend};
    MeasMode measMode;

    public static string MeasModeToString(MeasMode mode) => mode switch
    {
        MeasMode.Monitor => "Monitoring",
        MeasMode.Sweep   => "Sweep",
        MeasMode.Trend   => "Trend",
        _ => "Unknown"
    };

    public struct GraphPoint
    {
        public int index;
        public double x;
        public double y;
        public int trace;
        public MeasMode mode;
    }

    //Plot trace-point array. Que at, dequ at TabPlot by timer
    Queue<GraphPoint> graphPoints = new Queue<GraphPoint>();

    int streamLowMass=1, streamHighMass=100, streamSamplesPerAmu;
    int lastSamplesPerAmu = -1, lastHighMass = -1, lastLowMass = -1;

    bool sweepParametersChanged()
    {
        if ( (lastSamplesPerAmu != streamSamplesPerAmu) || (lastLowMass != streamLowMass) ||
                     (lastHighMass != streamHighMass)){
            lastSamplesPerAmu = streamSamplesPerAmu;
            lastHighMass = streamHighMass;
            lastLowMass = streamLowMass;
            return true;
        }
        return false;
    }


    bool isSweeping = false;

    void setupSweep(string fromQp)
    {
        //Log.Information(fromQp);

        if (scanFor(fromQp, "LowMass", out streamLowMass) == false)
            return;
        if (scanFor(fromQp, "HighMass", out streamHighMass) == false)
            return;
        if (scanFor(fromQp, "SamplesPerAmu", out streamSamplesPerAmu) == false)
            return;
        isSweeping = true;
    }

    void terminateSweep()
    {
        isSweeping = false;
    }

    float[] trendDomain = new float[12];
    
    void setupTrend(string fromQp)
    {
        //Log.Information(fromQp);
        int sweepnum = 0;

        if (scanFor(fromQp, "sweep", out sweepnum) == false)
            return;

        string[] fields = fromQp.Split(':');

        if (fields.Length < 3)
            return;
        if (fields[1] != "sweep")
            return;
        if (Int32.TryParse(fields[2], out sweepnum) == false)
            return;

        int samples = 0;
        for (int i = 3; (i < fields.Length) && (i < 12); i++){
            float val;
            if (float.TryParse(fields[i], System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out val) == false)
                break;
            trendDomain[samples++] = val;
        }

        trendSamples = samples;
    }
    int trendSamples = 1;

    void terminateTrend()
    {
        // nothing to do
    }


    bool newDataToPlot = false;

    //Plot en-que main entry point -------------
    //Trace
    void enqueueDataPoint(MeasMode mode, int trace, int index, float x, float y)
    {
        newDataToPlot = true;
        GraphPoint g = new GraphPoint();
        g.index = index;
        g.x = x;
        g.y = y;
        g.trace = trace;
        g.mode = mode;

        graphPoints.Enqueue(g);
        newDataToPlot = true;
    }

    public void enqueuePlotDataPoint(GraphPoint g){
        newDataToPlot = true;
        graphPoints.Enqueue(g);
        newDataToPlot = true;
    }

    //Sweep
    void enqueueDataPoint(int index, float x, float y)
    {
        //sweep_trace=0
        enqueueDataPoint(MeasMode.Sweep, 0, index, x, y);
        //App.con( String.Format("plot.eequ-"+index +"-"+ x +"-"+ y) );
    }

    void emitSample(int s, float y)
    {   
        int left = (streamSamplesPerAmu - 1) / 2;  // integer round down
        float x = ((float) streamLowMass) + ((float) (s - left)) / ((float) streamSamplesPerAmu);
        enqueueDataPoint(s, x, y);
    }

    //Line processors

    //SweepLine example
    //BeginStream:LowMass:1:HighMass:45:SamplesPerAmu:6:sweep:61
    //s10:0:5.308e-14, s10:1:5.467e-14, s10:2:5.397e-14

    void processSweep64Line(string start, string fromQp)
    {
        int startSample;
        if (Int32.TryParse(start, out startSample) == false){
            // unable to parse start sample.
            return;
        }

        byte[] data;
        try {
            data = Convert.FromBase64String(fromQp);
        } catch {
            // silent return is graceful here.
            return;
        }

        if ( (data.Length % 4) != 0){
            // unexpected length in array of floats
            return;
        }
        int numfloats = data.Length / 4;
        for (int i = 0; i < numfloats; i++){
            float f = BitConverter.ToSingle(data, 4 * i);
            emitSample(startSample + i, f);
        }
    }

    public static bool hexToFloat(string hexString, out float val)
    {
        int v;
        val = 0;
        try {
            v = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        } catch { return false; }

        byte[] r = new byte[4];
        for (int i = 0; i < 4; i++){
            r[i] = (byte) (0xff & v);
            v = v >> 8;
        }
        val =  BitConverter.ToSingle(r, 0);
        return true;
    }

    void processSweep16Line(string fromQp)
    {
        int startSample;
        string[] fields = fromQp.Split(':');

        if (fields.Length < 3)
            return;

        if (Int32.TryParse(fields[1], out startSample) == false)
            return;

        for (int i = 2; i < fields.Length; i++){
            float val;
            if (hexToFloat(fields[i], out val) == false)
                return;
            emitSample(startSample++, val);
        }
    }

    void processSweep10Line(string fromQp)
    {
        int startSample;
        string[] fields = fromQp.Split(':');

        if (fields.Length < 3) return;

        if (Int32.TryParse(fields[1], out startSample) == false) return;

        for (int i = 2; i < fields.Length; i++){
            float val;
            if (float.TryParse(fields[i], System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out val) == false)
                break;
            emitSample(startSample++, val);
        }
    }


    //Trend example
    //-> trend, <- inf:FirstSweep:54, <- inf:LastSweep:54
    //<- BeginTrend:sweep:54:1:12
    //<- t10:0:6.216e-14, <- t10:1:5.903e-14, 
    //<- EndTrend

    int nextTrendSample = 0;        // x-coordinate on trend plot.
    public int plotSamples; // number of horizontal items to plot.

    void incrementTrendSample(){
        nextTrendSample = ( (nextTrendSample + 1) % plotSamples);
    }


    List<int> plot_ch;  //sorted list of trend channels, as in data stream

    void processTrend10Line(string fromQp)
    {
        float current;
        int startSample;

        string[] fields = fromQp.Split(':');
        if (Int32.TryParse(fields[1], out startSample) == false)
            return;

        int nextTrendTrace = startSample % trendSamples;

        if (fields.Length < 3)
            return;
        for (int i = 2; i < fields.Length; i++){
            if (float.TryParse(fields[i], System.Globalization.NumberStyles.Any, 
                    System.Globalization.CultureInfo.InvariantCulture, out current) == false)
                break;

            enqueueDataPoint(MeasMode.Trend, 
                            plot_ch[nextTrendTrace],
                            nextTrendSample,
                            nextTrendSample, current);

            if ( (++nextTrendTrace % trendSamples) == 0){
                incrementTrendSample();
                nextTrendTrace = 0;
            }
        }
    }

    void processTrend16Line(string fromQp)
    {
        float current;
        int startSample;

        string[] fields = fromQp.Split(':');
        if (Int32.TryParse(fields[1], out startSample) == false)
            return;

        int nextTrendTrace = startSample % trendSamples;

        if (fields.Length < 3)
            return;
        for (int i = 2; i < fields.Length; i++){
            if (hexToFloat(fields[i], out current) == false)
                return;

            enqueueDataPoint(MeasMode.Trend, 
                            plot_ch[nextTrendTrace],
                            nextTrendSample,
                            nextTrendSample,
                            current
                            );
            if ( (++nextTrendTrace % trendSamples) == 0){
                incrementTrendSample();
                nextTrendTrace = 0;
            }
        }
    }

    void processTrend64Line(string start, string fromQp)
    {
        int startSample;
        if (Int32.TryParse(start, out startSample) == false){
            // unable to parse start sample.
            return;
        }

        int nextTrendTrace = startSample % trendSamples;

        byte[] data;
        try {
            data = Convert.FromBase64String(fromQp);
        } catch {
            // silent return is graceful here.
            return;
        }

        if ( (data.Length % 4) != 0){
            // unexpected length in array of floats
            return;
        }
        int numfloats = data.Length / 4;
        for (int i = 0; i < numfloats; i++){
            float current = BitConverter.ToSingle(data, 4 * i);

            enqueueDataPoint(MeasMode.Trend, nextTrendTrace,
                             nextTrendSample,
                             nextTrendSample, current);

            if ( (++nextTrendTrace % trendSamples) == 0){
                incrementTrendSample();
                nextTrendTrace = 0;
            }

        }
    }

}
