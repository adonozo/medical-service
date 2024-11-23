namespace QMUL.DiabetesBackend.MongoDb.Models;

using Model;
using MongoDB.Bson;

public static class MappingExtensions
{
    public static ObservationTemplate? ToObservationTemplate(this MongoObservationTemplate? template) =>
        template is null
            ? null
            : new ObservationTemplate
            {
                Id = template.Id.ToString(),
                Code = template.Code,
                ValueTemplate = template.ValueTemplate,
                CodeValue = template.CodeValue,
                ReferenceRange = template.ReferenceRange,
                Metadata = template.Metadata.ToObservationMetadata()
            };

    public static MongoObservationTemplate? ToMongoObservationTemplate(this ObservationTemplate? template) =>
        template is null
            ? null
            : new MongoObservationTemplate
            {
                Id = template.Id is null ? null : ObjectId.Parse(template.Id),
                Code = template.Code,
                ValueTemplate = template.ValueTemplate,
                CodeValue = template.CodeValue,
                ReferenceRange = template.ReferenceRange,
                Metadata = template.Metadata.ToMongoObservationMetadata()
            };

    public static MongoObservationMetadata ToMongoObservationMetadata(this ObservationMetadata metadata) =>
        new() { ObservationType = metadata.ObservationType };

    public static ObservationMetadata ToObservationMetadata(this MongoObservationMetadata metadata) =>
        new() { ObservationType = metadata.ObservationType };
}