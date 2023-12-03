namespace QMUL.DiabetesBackend.MongoDb.Models;

using Model.Alexa;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

/// <summary>
/// A simple class to handle <see cref="AlexaRequest"/> objects as MongoDB ones.
/// </summary>
public class AlexaRequestMongo
{
    public ObjectId Id { get; set; }

    public string? DeviceId { get; init; }

    public string? UserId { get; init; }

    [BsonSerializer(typeof(InstantSerializer))]
    public Instant Timestamp { get; init; }

    public AlexaRequest MapToAlexaRequest()
    {
        return new AlexaRequest
        {
            Id = this.Id.ToString(),
            DeviceId = this.DeviceId,
            UserId = this.UserId,
            Timestamp = this.Timestamp
        };
    }

    public static AlexaRequestMongo MapToAlexaMongoRequest(AlexaRequest request)
    {
        var mongoRequest = new AlexaRequestMongo
        {
            DeviceId = request.DeviceId,
            UserId = request.UserId,
            Timestamp = request.Timestamp
        };

        if (!string.IsNullOrEmpty(request.Id))
        {
            mongoRequest.Id = new ObjectId(request.Id);
        }

        return mongoRequest;
    }
}
