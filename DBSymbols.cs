/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Data Classes - EF DB for Device Symbols command
 */

//Extorr calibration table
[Table("CalibTable")]
public class ExtorCalibTable{
    [Key] public int Id { get; set; }
    public string Name { get; set; }

    public Dictionary<String,String> ParamDic { get; set; }
}
    
//Extorr operating (control) parameters table
[Table("OperateTable")]
public class ExtorOperateTable{
    [Key] public int Id { get; set; }
    public string Name { get; set; }

    public Dictionary<String,String> ParamDic { get; set; }
}

//Extorr output parameters table
[Table("OutputTable")]
public class ExtorOutputTable{
    [Key] public int Id { get; set; }
    public string Name { get; set; }

    public Dictionary<String,String> ParamDic { get; set; }
}

//Extorr monitoring parameters table
[Table("MoniTable")]
public class ExtorMoniTable{
    [Key] public int Id { get; set; }
    public string Name { get; set; }

    public List<String> ParamList { get; set; }    
}

