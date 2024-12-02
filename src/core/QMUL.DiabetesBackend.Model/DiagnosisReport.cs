namespace QMUL.DiabetesBackend.Model;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Enums;
using FHIR;
using NodaTime;

public class DiagnosisReport
{
    public string Id { get; set; }

    public Status Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DiagnosisCategory Category { get; set; }

    public Code Code { get; set; }

    public Instant EffectiveTimestamp { get; set; }

    public Instant Issued { get; set; }

    public Reference Subject { get; set; }

    public Reference ResultsInterpreter { get; set; } // TODO 'interpreters service'

    public IReadOnlyList<Reference> Results { get; set; }

    public string Conclusion { get; set; }
}