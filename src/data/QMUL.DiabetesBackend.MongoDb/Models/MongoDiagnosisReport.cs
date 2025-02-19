namespace QMUL.DiabetesBackend.MongoDb.Models;

using System.Collections.Generic;
using Model.Enums;
using Model.FHIR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using NodaTime;

public class MongoDiagnosisReport
{
    [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
    public ObjectId? Id { get; set; }

    public Status Status { get; set; }

    public DiagnosisCategory Category { get; set; }

    public Code Code { get; set; }

    [BsonSerializer(typeof(InstantSerializer))]
    public Instant EffectiveTimestamp { get; set; }

    [BsonSerializer(typeof(InstantSerializer))]
    public Instant Issued { get; set; }

    public ReferenceValueRange Subject { get; set; }

    public ReferenceValueRange ResultsInterpreter { get; set; }

    public IReadOnlyList<ReferenceValueRange> Results { get; set; }

    public string Conclusion { get; set; }
}