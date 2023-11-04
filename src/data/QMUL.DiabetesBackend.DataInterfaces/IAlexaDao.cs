namespace QMUL.DiabetesBackend.DataInterfaces;

using System.Threading.Tasks;
using Model.Alexa;

public interface IAlexaDao
{
    /// <summary>
    /// Inserts a patient's Alexa request
    /// </summary>
    /// <param name="request">The patient's <see cref="AlexaRequest"/></param>
    /// <returns>A boolean value to know if the insert was successful</returns>
    Task<bool> InsertRequest(AlexaRequest request);

    /// <summary>
    /// Gets a patient's last Alexa request for a given device ID.
    /// </summary>
    /// <param name="deviceId">The patient's Alexa device ID</param>
    /// <returns>The patient's last <see cref="AlexaRequest"/>, or null if there isn't any previous request.</returns>
    Task<AlexaRequest?> GetLastRequest(string deviceId);
}
