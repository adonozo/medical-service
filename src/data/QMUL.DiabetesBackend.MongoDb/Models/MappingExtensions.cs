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
                Id = template.Id?.ToString(),
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

    public static DiagnosisReport? ToDiagnosisReport(this MongoDiagnosisReport? report) =>
        report is null
            ? null
            : new DiagnosisReport
            {
                Id = report.Id?.ToString(),
                Status = report.Status,
                Category = report.Category,
                Code = report.Code,
                EffectiveTimestamp = report.EffectiveTimestamp,
                Issued = report.Issued,
                Subject = report.Subject,
                ResultsInterpreter = report.ResultsInterpreter,
                Results = report.Results,
                Conclusion = report.Conclusion
            };

    public static MongoDiagnosisReport? ToMongoDiagnosisReport(this DiagnosisReport? report) =>
        report is null
            ? null
            : new MongoDiagnosisReport
            {
                Id = report.Id is null ? null : ObjectId.Parse(report.Id),
                Status = report.Status,
                Category = report.Category,
                Code = report.Code,
                EffectiveTimestamp = report.EffectiveTimestamp,
                Issued = report.Issued,
                Subject = report.Subject,
                ResultsInterpreter = report.ResultsInterpreter,
                Results = report.Results,
                Conclusion = report.Conclusion
            };

    private static MongoObservationMetadata ToMongoObservationMetadata(this ObservationMetadata metadata) =>
        new() { Version = metadata.Version };

    private static ObservationMetadata ToObservationMetadata(this MongoObservationMetadata metadata) =>
        new() { Version = metadata.Version };
}