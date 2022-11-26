namespace QMUL.DiabetesBackend.MongoDb;

using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Model.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using Utils;

public class CarePlanDao : MongoDaoBase, ICarePlanDao
{
    private const string CollectionName = "carePlan";

    private readonly IMongoCollection<BsonDocument> carePlanCollection;

    public CarePlanDao(IMongoDatabase database) : base(database)
    {
        this.carePlanCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
    }

    public async Task<CarePlan> CreateCarePlan(CarePlan carePlan)
    {
        var document = await Helpers.ToBsonDocumentAsync(carePlan);
        await this.carePlanCollection.InsertOneAsync(document);

        var newId = this.GetIdFromDocument(document);
        var cursor = this.carePlanCollection.Find(Helpers.GetByIdFilter(newId));
        document = await this.GetSingleOrThrow(cursor, new WriteResourceException("Could not create the care plan"));
        return await Helpers.ToResourceAsync<CarePlan>(document);
    }

    public async Task<CarePlan?> GetCarePlan(string id)
    {
        var document = await this.carePlanCollection.Find(Helpers.GetByIdFilter(id)).FirstOrDefaultAsync();
        if (document == null)
        {
            return null;
        }

        return await Helpers.ToResourceAsync<CarePlan>(document);
    }

    public async Task<bool> UpdateCarePlan(string id, CarePlan carePlan)
    {
        var document = await Helpers.ToBsonDocumentAsync(carePlan);
        var result = await this.carePlanCollection.ReplaceOneAsync(Helpers.GetByIdFilter(id), document);

        return result.IsAcknowledged;
    }
}