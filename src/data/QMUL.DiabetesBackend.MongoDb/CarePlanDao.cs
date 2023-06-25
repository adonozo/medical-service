namespace QMUL.DiabetesBackend.MongoDb;

using System.Collections.Generic;
using System.Threading.Tasks;
using DataInterfaces;
using Hl7.Fhir.Model;
using Model;
using Model.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using Utils;
using Task = System.Threading.Tasks.Task;

public class CarePlanDao : MongoDaoBase, ICarePlanDao
{
    private const string CollectionName = "carePlan";

    private readonly IMongoCollection<BsonDocument> carePlanCollection;

    /// <inheritdoc/>
    public CarePlanDao(IMongoDatabase database) : base(database)
    {
        this.carePlanCollection = this.Database.GetCollection<BsonDocument>(CollectionName);
    }

    /// <inheritdoc/>
    public async Task<CarePlan> CreateCarePlan(CarePlan carePlan)
    {
        var document = await Helpers.ToBsonDocumentAsync(carePlan);
        await this.carePlanCollection.InsertOneAsync(document);

        var newId = this.GetIdFromDocument(document);
        var cursor = this.carePlanCollection.Find(Helpers.GetByIdFilter(newId));
        document = await this.GetSingleOrThrow(cursor, new WriteResourceException("Could not create the care plan"));
        return await Helpers.ToResourceAsync<CarePlan>(document);
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<IEnumerable<Resource>>> GetCarePlans(string patientId,
        PaginationRequest paginationRequest)
    {
        var searchFilter = Helpers.GetPatientReferenceFilter(patientId);
        var resultsFilter = Helpers.GetPaginationFilter(searchFilter, paginationRequest.LastCursorId);

        var results = await this.carePlanCollection.Find(resultsFilter)
            .Limit(paginationRequest.Limit)
            .Project(document => Helpers.ToResourceAsync<CarePlan>(document))
            .ToListAsync();
        Resource[] carePlans = await Task.WhenAll(results);

        if (carePlans.Length == 0)
        {
            return new PaginatedResult<IEnumerable<Resource>> { Results = carePlans };
        }

        return await Helpers.GetPaginatedResult(this.carePlanCollection, searchFilter, carePlans);
    }

    /// <inheritdoc/>
    public async Task<CarePlan?> GetCarePlan(string id)
    {
        var document = await this.carePlanCollection.Find(Helpers.GetByIdFilter(id)).FirstOrDefaultAsync();
        if (document == null)
        {
            return null;
        }

        return await Helpers.ToResourceAsync<CarePlan>(document);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateCarePlan(string id, CarePlan carePlan)
    {
        var document = await Helpers.ToBsonDocumentAsync(carePlan);
        var result = await this.carePlanCollection.ReplaceOneAsync(Helpers.GetByIdFilter(id), document);

        return result.IsAcknowledged;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteCarePlan(string id)
    {
        var result = await this.carePlanCollection.DeleteOneAsync(Helpers.GetByIdFilter(id));
        return result.IsAcknowledged;
    }
}