namespace QMUL.DiabetesBackend.MongoDb;

using System.Globalization;
using MongoDB.Bson;
using MongoDB.Driver;

/// <summary>
/// A multi-lingual collection will use a suffix for the configured culture, e.g., medication-es. Depends on the
/// current <see cref="CultureInfo"/>.
/// </summary>
public abstract class MongoMultiLingualBase : MongoDaoBase
{
    protected MongoMultiLingualBase(IMongoDatabase database) : base(database)
    {
    }

    /// <summary>
    /// Builds the localized collection by suffixing the culture name to the collection name
    /// </summary>
    /// <remarks>Only the neutral culture is used</remarks>
    /// <param name="collectionName"></param>
    /// <returns>The <see cref="IMongoCollection{TDocument}"/> for <see cref="BsonDocument"/></returns>
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