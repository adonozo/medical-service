namespace QMUL.DiabetesBackend.Model
{
    /// <summary>
    /// An auxiliary plain class to map database settings with a settings file (appsettings.json).
    /// </summary>
    public class MongoDatabaseSettings : IDatabaseSettings
    {
        public string DatabaseName { get; set; }
        
        public string DatabaseConnectionString { get; set; }
    }

    /// <summary>
    /// An auxiliary interface to map database settings.
    /// </summary>
    public interface IDatabaseSettings
    {
        public string DatabaseName { get; set; }
        
        public string DatabaseConnectionString { get; set; }
    }
}