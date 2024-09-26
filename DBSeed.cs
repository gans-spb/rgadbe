/* Extorr Residual Gas Analyzers - DataBase Edition
 * (C) Ioffe inst., Igor Bocharov
 * Data Classes - Init data seeder
 */

public static class ModelBuilderExtensions
{
     public static void Seed(this ModelBuilder modelBuilder){
        
        modelBuilder.Entity<ExtorMeasurements>().HasData(
            //Don`t delete! Uses as link to foreighn IDs after first start
            new ExtorMeasurements { Id=1, Type="Seed", Name = "Init record 1", Desc = "This is first init record seeder", 
                StartTime = new DateTime(2024, 1, 1, 0, 0, 0),
                StopTime  = new DateTime(2024, 1, 1, 0, 0, 0), 
                ProcessTime = new TimeSpan(0),
                DataSet = new List<List<string>>(), //empty data set
                PollTime=10, TimeLim=15, Samples = 0,
                Final = "manual",
                OutputsId = 1, OperateId=1, MassChId=1,  MoniSetId=1, CalibId=1 }
        );

        modelBuilder.Entity<ExtorCalibTable>().HasData(
            new ExtorCalibTable {Id=1, Name = "sn6463 factory calib",
                ParamDic = new Dictionary<String,String>{
                    {"HighCalMass","300"}, {"HighCalResolution","2015"}, {"HighCalIonEnergy","5.0"}, {"HighCalPosition","0.82"},
                    {"LowCalMass","1"}, {"LowCalResolution","640"}, {"LowCalIonEnergy","4.5"}, {"LowCalPosition","0.34"},
                    {"PartialCapPf","3.0"},{"PartialSensitivity","6.00e-4"},{"TotalCapPf","10.0"},{"TotalSensitivity","1.0E+1"},
                    {"Pirani1ATM","2.019"},{"PiraniZero","0.305"},{"Pirani1ATMTemp","8.544E-1"},{"PiraniZeroTemp","1.949"},
                    {"SwSettleTicks","10"},{"RwSettleTicks","50"},/*,{"TotalAmpOffset","2000"},{"PartialAmpOffset","2100"}*/
                    {"ModelNumber", "1300"}, {"SerialNumber", "6463"}, {"VersionMajor", "0"}, {"VersionMinor", "12"}
                    } 
            },
            new ExtorCalibTable {Id=2, Name = "from device settings", 
                ParamDic = new Dictionary<String,String>{
                    {"HighCalMass","300"}, {"HighCalResolution","1650"}, {"HighCalIonEnergy","5.0"}, {"HighCalPosition","1.37"},
                    {"LowCalMass","1"}, {"LowCalResolution","630"}, {"LowCalIonEnergy","5.0"}, {"LowCalPosition","0.3"},
                    {"PartialCapPf","3.0"},{"PartialSensitivity","0.6"},{"TotalCapPf","10.0"},{"TotalSensitivity","1.0E+1"},
                    {"Pirani1ATM","1.966"},{"PiraniZero","0.342"},{"Pirani1ATMTemp","8.54E-1"},{"PiraniZeroTemp","1.949"},
                    {"SwSettleTicks","10"},{"RwSettleTicks","50"},/*,{"TotalAmpOffset","2000"},{"PartialAmpOffset","2100"}*/
                    {"ModelNumber", "1300"}, {"SerialNumber", "6463"}, {"VersionMajor", "0"}, {"VersionMinor", "12"}
                    } 
            }
        );

        modelBuilder.Entity<ExtorOutputTable>().HasData(
             new ExtorOutputTable {Id=1, Name = "Seed output", ParamDic = new Dictionary<String,String>{} }
        );
        
        modelBuilder.Entity<ExtorOperateTable>().HasData(
            new ExtorOperateTable {Id=1, Name = "Seed",
                ParamDic = new Dictionary<String,String>{
                    {"ScanSpeed","3.0"}, {"LowMass" ,"1"}, {"HighMass" ,"10"}, {"SamplesPerAmu" ,"6"}, {"AutoStream" ,"1"},
                    {"Focus1Volts" ,"-20"}, {"ElectronVolts" ,"70.000"}, {"FilamentEmissionMa" ,"2.000"}, {"MultiplierVolts" ,"0"},
                    {"AutoZero" ,"0"}, {"Encoding" ,"10"}, {"SamplesPerLine" ,"1"}, {"PressureUnits" ,"1"} /*{"Filament" ,"0"},*/
                }
            }
        );

        //EVERY monitoring parameter MUST be previously check by extest.py at least 15min long
        //cause SOME of parameters STUCK device after approx 5 minutes of polling
        //and device need to be firmware uploaded again
        //as example - "IonizerOhms" and seems any Ionizer parameters
        int id=1;
        modelBuilder.Entity<ExtorMoniTable>().HasData(
            new ExtorMoniTable {Id=id++, Name = "Pressure set",
            ParamList = new List<string> {"PiraniTorr", "PiraniVolts", "PressureTorr", "PressureAmps", "PressurePascal"} },

            new ExtorMoniTable {Id=id++, Name = "Pirani set",
            ParamList = new List<string> {"PiraniTorr", "PiraniVolts", "PiraniOhms", "PiraniCorrVolts", "PiraniTempVolts"} },

            new ExtorMoniTable {Id=id++, Name = "Statistic set",
            ParamList = new List<string> {"PiraniTorr", "FilamentStatus", "IsIdle", "LastSweep", "FirstSweep"} },

            new ExtorMoniTable {Id=id++, Name = "Ioniser set",
            ParamList = new List<string> {"FilamentPowerPct", "IonizerVolts", "IonizerAmps", "SourceGrid1Ma", "SourceGrid2Ma"} },/*, "IonizerOhms"*/

            new ExtorMoniTable {Id=id++, Name = "Temperature set",
            ParamList = new List<string> {"QuadrupoleDegC", "InteriorDegC", "SupplyVolts", "DegasMa", "RepellerVolts"} },

            new ExtorMoniTable {Id=id++, Name = "Hlam set",
            ParamList = new List<string> {"RfAmpVolts", "ReferenceVolts", "GroundVolts", "FbPlus", "FbMinus"} }           
        );
       
        modelBuilder.Entity<ExtorMassChannels>().HasData(
            new ExtorMassChannels {Id=1, Name = "init data seed"}
        );

        id=1;
        modelBuilder.Entity<ExtorMassTable>().HasData(
            new ExtorMassTable { Id=id++, Name = "PP-Pirani presure", Amu = 998, Dwell =42.0F, Enable = true, LoAlarm = 1E-7F, LoWarn = 2E-7F, HiWarn = 7E-5F, HiAlarm = 8E-5F},
            new ExtorMassTable { Id=id++, Name = "TP-Total presure",  Amu = 999, Dwell =42.0F, Enable = true},
            new ExtorMassTable { Id=id++, Name = "H-Hydrogen",        Amu = 1,  Dwell =100.0F, Enable = true},
            new ExtorMassTable { Id=id++, Name = "H2-DiHydrogen",     Amu = 2,  Dwell =100.0F, Enable = true},
            new ExtorMassTable { Id=id++, Name = "C-Carbon",          Amu = 12, Dwell =100.0F, Enable = false},
            new ExtorMassTable { Id=id++, Name = "N-Nitrogen",        Amu = 14, Dwell =100.0F, Enable = false},            
            new ExtorMassTable { Id=id++, Name = "O-Oxygen",          Amu = 16, Dwell =100.0F, Enable = true},
            new ExtorMassTable { Id=id++, Name = "H20-Water",         Amu = 18, Dwell =100.0F, Enable = true, LoAlarm = 1E-7F, LoWarn = 2E-7F, HiWarn = 4E-7F, HiAlarm = 5E-7F},
            new ExtorMassTable { Id=id++, Name = "N2-Dinitrogen",     Amu = 28, Dwell =100.0F, Enable = false},
            new ExtorMassTable { Id=id++, Name = "O2-Dioxygen",       Amu = 32, Dwell =100.0F, Enable = false},
            new ExtorMassTable { Id=id++, Name = "Ar-Argon",          Amu = 40, Dwell =100.0F, Enable = true},            
            new ExtorMassTable { Id=id++, Name = "CO2-Carbon dioxide",Amu = 44, Dwell =100.0F, Enable = true},
            new ExtorMassTable { Id=id++, Name = "V-Vanadium",        Amu = 50, Dwell =100.0F, Enable = false}
        );
    }
}
