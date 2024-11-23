namespace QMUL.DiabetesBackend.Model;

using System.Text.Json.Serialization;
using Enums;

public class ObservationMetadata
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ObservationType ObservationType { get; set; }
}