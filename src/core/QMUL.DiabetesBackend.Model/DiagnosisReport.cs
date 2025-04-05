namespace QMUL.DiabetesBackend.Model;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Enums;
using FHIR;
using NodaTime;

public class DiagnosisReport
{
    public string Id { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Status Status { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DiagnosisCategory Category { get; set; }

    public Code Code { get; set; }

    public Instant EffectiveTimestamp { get; set; }

    public Instant Issued { get; set; }

    public ResourceReference Subject { get; set; }

    public ResourceReference ResultsInterpreter { get; set; } // TODO 'interpreters service'

    public IReadOnlyList<ResourceReference> Results { get; set; }

    public string Conclusion { get; set; }
}