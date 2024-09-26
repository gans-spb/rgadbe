/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Data Classes - Measurement DB structure
 */

//Extorr measurements bind settings to data
[Table("Measurements")]
public class ExtorMeasurements //: INotifyPropertyChanged
{
    [Key] public int Id { get; set; }

    public string Name  { get; set; }

    public string? Desc { get; set; }

    public DateTime  StartTime   { get; set; } = System.DateTime.Now;
    public DateTime? StopTime    { get; set; }
    public TimeSpan? ProcessTime { get; set; }

    public List<List<string>> DataSet  {get;set; } = null!;

    public String Type { get; set; }   //Moni-Sweep-Trend

    public int  PollTime { get; set; } //polling data every PollTime sec
    public int  TimeLim { get; set; }  //finish polling after TimeLim min, or forever

    public int  Samples { get; set; }  //samples got
    public string Final { get; set; }  //manual, auto, timer, (fatal)

    public int OutputsId { get; set; } 
    public ExtorOutputTable Outputs { get; set; } 

    public int OperateId { get; set; } 
    public ExtorOperateTable Operate { get; set; } 

    public int MassChId { get; set; } 
    public ExtorMassChannels MassCh { get; set; } 

    public int MoniSetId { get; set; } 
    public ExtorMoniTable MoniSet { get; set; } 

    public int CalibId { get; set; } 
    public ExtorCalibTable Calib { get; set; } 
}