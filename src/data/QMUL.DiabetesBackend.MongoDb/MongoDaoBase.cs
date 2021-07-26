using MongoDB.Driver;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.MongoDb
{
    public abstract class MongoDaoBase
    {
        protected readonly IMongoDatabase Database;
        
        protected MongoDaoBase(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.DatabaseConnectionString);
            this.Database = client.GetDatabase(settings.DatabaseName);
        }
    }
}