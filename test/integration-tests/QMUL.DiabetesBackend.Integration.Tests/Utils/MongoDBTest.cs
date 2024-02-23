namespace QMUL.DiabetesBackend.Integration.Tests.Utils;

using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDb;

public class MongoDBTest : MongoDaoBase
{
    public MongoDBTest(IMongoDatabase database) : base(database)
    {
    }

    public async Task ResetDatabase(string database)
    {
        await this.Database.Client.DropDatabaseAsync(database);
    }
}