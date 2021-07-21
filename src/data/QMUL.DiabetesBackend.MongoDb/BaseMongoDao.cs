using MongoDB.Driver;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.MongoDb
{
    public abstract class BaseMongoDao
    {
        protected readonly IMongoDatabase Database;
        
        protected BaseMongoDao(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.DatabaseConnectionString);
            this.Database = client.GetDatabase(settings.DatabaseName);
        }
    }
}