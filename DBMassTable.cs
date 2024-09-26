/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Data Classes - Mass Table
 */

//Extorr one mass trend properties
[Table("MassTable")]
public class ExtorMassTable{
    [Key] public int Id { get; set; }
        
    //Atom mass proerties for device settings
    public bool   Enable { get; set; }
    public string Name   { get; set; }
    public int    Amu    { get; set; }
    public float  Dwell  { get; set; }
        
    //Alarm and warnings for iternal algo
    public float? LoAlarm { get; set; }
    public float? LoWarn  { get; set; }
    public float? HiWarn  { get; set; }
    public float? HiAlarm { get; set; }
}

//Extorr channel-to-masstable binding
[Table("MassChannels")]
public class ExtorMassChannels{
    [Key] public int Id { get; set; }
    public string Name  { get; set; }

    public List<int?> GetChList(){
    return new List<int?> {
        Mass1Id, Mass2Id, Mass3Id, Mass4Id, 
        Mass5Id, Mass6Id, Mass7Id, Mass8Id, 
        Mass9Id, Mass10Id, Mass11Id, Mass12Id, 
        };
    }
    
    public void SetChList(List<int?> l){
        Mass1Id = l[0]; Mass2Id = l[1]; Mass3Id = l[2]; Mass4Id = l[3];
        Mass5Id = l[4]; Mass6Id = l[5]; Mass7Id = l[6]; Mass8Id = l[7];
        Mass9Id = l[8]; Mass10Id = l[9]; Mass11Id = l[10]; Mass12Id = l[11];
    }

    //12 channels for trend mode
    //2Do ugly code, but EF style
    public int? Mass1Id { get; set; } 
    public ExtorMassTable? Mass1 { get; set; } 

    public int? Mass2Id { get; set; } 
    public ExtorMassTable? Mass2 { get; set; } 

    public int? Mass3Id { get; set; } 
    public ExtorMassTable? Mass3 { get; set; } 

    public int? Mass4Id { get; set; } 
    public ExtorMassTable? Mass4 { get; set; } 

    public int? Mass5Id { get; set; } 
    public ExtorMassTable? Mass5 { get; set; } 

    public int? Mass6Id { get; set; } 
    public ExtorMassTable? Mass6 { get; set; } 

    public int? Mass7Id { get; set; } 
    public ExtorMassTable? Mass7 { get; set; } 

    public int? Mass8Id { get; set; } 
    public ExtorMassTable? Mass8 { get; set; } 

    public int? Mass9Id { get; set; } 
    public ExtorMassTable? Mass9 { get; set; } 

    public int? Mass10Id { get; set; } 
    public ExtorMassTable? Mass10 { get; set; } 

    public int? Mass11Id { get; set; } 
    public ExtorMassTable? Mass11 { get; set; } 

    public int? Mass12Id { get; set; } 
    public ExtorMassTable? Mass12 { get; set; } 
}
