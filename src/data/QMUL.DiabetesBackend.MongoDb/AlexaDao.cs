namespace QMUL.DiabetesBackend.MongoDb;

using System;
using System.Threading.Tasks;
using DataInterfaces;
using Microsoft.Extensions.Logging;
using Model.Alexa;
using Models;
using MongoDB.Driver;

public class AlexaDao : MongoDaoBase, IAlexaDao
{
    private const string CollectionName = "alexaRequest";

    private readonly IMongoCollection<AlexaRequestMongo> alexaCollection;
    private readonly ILogger<AlexaDao> logger;

    public AlexaDao(IMongoDatabase database, ILogger<AlexaDao> logger) : base(database)
    {
        this.alexaCollection = this.Database.GetCollection<AlexaRequestMongo>(CollectionName);
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> InsertRequest(AlexaRequest request)
    {
        try
        {
            var mongoRequest = AlexaRequestMongo.MapToAlexaMongoRequest(request);
            await this.alexaCollection.InsertOneAsync(mongoRequest);
            return true;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Error trying to insert alexa request");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<AlexaRequest?> GetLastRequest(string deviceId)
    {
        var filter = Builders<AlexaRequestMongo>.Filter.Eq(request => request.DeviceId, deviceId);
        var request = await this.alexaCollection.Find(filter)
            .Limit(1)
            .FirstOrDefaultAsync();
        return request?.MapToAlexaRequest();
    }
}