namespace QMUL.DiabetesBackend.MongoDb.Models;

using Model.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class MongoObservationMetadata
{
    [BsonRepresentation(BsonType.String)]
    public ObservationType ObservationType { get; set; }
}