namespace QMUL.DiabetesBackend.Model.Alexa;

using NodaTime;

/// <summary>
/// Keeps the record of a patient using the Alexa Skill. Useful to personalize welcome prompts taking into consideration
/// if the patient has used the skill before.
/// </summary>
public record AlexaRequest
{
    public string Id { get; init; }

    public string DeviceId { get; init; }

    public string UserId { get; init; }

    public Instant Timestamp { get; init; }
}