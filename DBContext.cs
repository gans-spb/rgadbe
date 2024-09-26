/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Data Classes - EF DB Context
 */

public class AppDBContext : DbContext
{
    public string DBname { get; set; }

    //catch except here
    public bool EnsureCreated(string dbname){
        DBname = dbname;
        try{    
            if (Database.EnsureCreated()) 
                Log.Information("DataBase " + dbname + " created and seeded");
            return false; //do not inform if DB already created
        }
        catch(Exception e){
            Log.Fatal("DB seed error! " + App.StringFromEx(e));
            System.IO.File.WriteAllText("coredb" + App.GetDTNow().ToString("yy.MM.dd")  + ".dump", e.ToString());
            return true;
        }
    }

    //TO DO - remove FK_indexes convention
    protected override void OnModelCreating(ModelBuilder modelBuilder){
        modelBuilder.Seed();
        
        modelBuilder.Entity<ExtorMeasurements>()
        .Property(b => b.DataSet)
        .HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
            v => System.Text.Json.JsonSerializer.Deserialize<List<List<string>>>(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) 
        );

        modelBuilder.Entity<ExtorCalibTable>()
        .Property(b => b.ParamDic)
        .HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) 
        );

        modelBuilder.Entity<ExtorOutputTable>()
        .Property(b => b.ParamDic)
        .HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }) 
        );

        modelBuilder.Entity<ExtorMoniTable>()
        .Property(b => b.ParamList)
        .HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
            v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true })         
        );

        modelBuilder.Entity<ExtorOperateTable>()
        .Property(b => b.ParamDic)
        .HasConversion(
            v => System.Text.Json.JsonSerializer.Serialize(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
        );

    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DBname}");
    
    public DbSet<ExtorMeasurements> Measurements {get;set; }
    
    public DbSet<ExtorCalibTable> CalibTable {get;set; } = null!;

    public DbSet<ExtorOperateTable> Operate {get;set; }

    public DbSet<ExtorOutputTable> Outputs {get;set; }

    public DbSet<ExtorMoniTable> MoniTable {get;set; } = null!;

    public DbSet<ExtorMassTable> MassTable {get;set; } = null!;
    
    public DbSet<ExtorMassChannels> MassChannels {get;set; } = null!;
}
