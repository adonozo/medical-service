namespace QMUL.DiabetesBackend.MongoDb;

using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;

public abstract class MongoMultiLingualBase : MongoDaoBase
{
    protected MongoMultiLingualBase(IMongoDatabase database) : base(database)
    {
    }

    protected IMongoCollection<BsonDocument> GetLocalizedCollection(string collectionName)
    {
        return this.Database.GetCollection<BsonDocument>(MultiLingualCollectionName(collectionName));
    }

    private static string MultiLingualCollectionName(string collectionName)
    {
        var culture = CultureInfo.CurrentCulture;
        if (!culture.IsNeutralCulture)
        {
            culture = culture.Parent;
        }

        return $"{collectionName}-{culture.Name}";
    }
}