namespace QMUL.DiabetesBackend.MongoDb.Models;

using System.Collections.Generic;
using Model.FHIR;
using MongoDB.Bson;

public class MongoObservationTemplate
{
    public ObjectId Id { get; set; }

    public Code Code { get; set; }

    public ValueQuantity? ValueTemplate { get; set; }

    public IEnumerable<Reference> ReferenceRange { get; set; }
}