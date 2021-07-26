namespace QMUL.DiabetesBackend.Model
{
    public class MongoDatabaseSettings : IDatabaseSettings
    {
        public string DatabaseName { get; set; }
        
        public string DatabaseConnectionString { get; set; }
    }
    
    public interface IDatabaseSettings
    {
        public string DatabaseName { get; set; }
        
        public string DatabaseConnectionString { get; set; }
    }
}