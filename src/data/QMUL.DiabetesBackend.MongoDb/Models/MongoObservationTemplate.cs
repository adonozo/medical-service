namespace QMUL.DiabetesBackend.MongoDb.Models;

using System.Collections.Generic;
using Model;
using Model.FHIR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

public class MongoObservationTemplate
{
    [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
    public ObjectId? Id { get; set; }

    public Code Code { get; set; }

    public ValueQuantity? ValueTemplate { get; set; }

    public Coding? CodeValue { get; set; }

    public IList<Reference> ReferenceRange { get; set; }

    public MongoObservationMetadata Metadata { get; set; }
}