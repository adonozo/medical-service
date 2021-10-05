namespace QMUL.DiabetesBackend.MongoDb
{
    using MongoDB.Driver;
    using Model;

    /// <summary>
    /// The Mongo Dao Base class. All Mongo classes must inherit from this class.
    /// </summary>
    public abstract class MongoDaoBase
    {
        /// <summary>
        /// A reference to the database.
        /// </summary>
        protected readonly IMongoDatabase Database;
        
        /// <summary>
        /// Set the database connection. Gets the connection string as an argument.
        /// </summary>
        /// <param name="settings">The Database settings. It contains the connection string.</param>
        protected MongoDaoBase(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.DatabaseConnectionString);
            this.Database = client.GetDatabase(settings.DatabaseName);
        }
    }
}