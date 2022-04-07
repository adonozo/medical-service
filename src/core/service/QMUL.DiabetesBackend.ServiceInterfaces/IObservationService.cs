namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using Hl7.Fhir.Model;
    using Model;
    using Model.Enums;

    /// <summary>
    /// The Observation Service Interface. Observations are glucose self-measurements for a patient. 
    /// </summary>
    public interface IObservationService
    {
        /// <summary>
        /// Creates an <see cref="Observation"/>.
        /// </summary>
        /// <param name="patientId">The patient's ID or email.</param>
        /// <param name="newObservation">The <see cref="Observation"/> to create.</param>
        /// <returns>The <see cref="Observation"/> created with a new ID.</returns>
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        /// <exception cref="CreateException">If the observation could not be created.</exception>
        public Task<Observation> CreateObservation(string patientId, Observation newObservation);

        /// <summary>
        /// Gets a single <see cref="Observation"/>.
        /// </summary>
        /// <param name="observationId">The observation ID to look for.</param>
        /// <returns>A single <see cref="Observation"/> object if found. An error otherwise.</returns>
        /// <exception cref="NotFoundException">If the observation was not found.</exception>
        public Task<Observation> GetSingleObservation(string observationId);

        /// <summary>
        /// Gets all the observations for a given patient.
        /// </summary>
        /// <param name="patientId">The patient ID who owns the observations.</param>
        /// <param name="paginationRequest">The paginated request parameters.</param>
        /// <returns>The list of <see cref="Observation"/> for a patient in a paginated <see cref="Bundle"/> object.</returns>
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        public Task<PaginatedResult<Bundle>> GetAllObservationsFor(string patientId, PaginationRequest paginationRequest);

        /// <summary>
        /// Gets the <see cref="Observation"/> for a patient given a <see cref="CustomEventTiming"/> event and a datetime.
        /// It considers the patient's timezone (as a string) to calculate the time interval.
        /// </summary>
        /// <param name="patientId">The patient's ID or email who owns the observations.</param>
        /// <param name="timing">A <see cref="CustomEventTiming"/> where the observation(s) happened.</param>
        /// <param name="dateTime">The <see cref="DateTime"/> when the observation(s) happened.</param>
        /// <param name="paginationRequest">The paginated request parameters.</param>
        /// <param name="patientTimezone">The patient's timezone.</param>
        /// <returns>A <see cref="PaginatedResult{T}"/> <see cref="Bundle"/> object with the list of observations.</returns>
        /// <exception cref="NotFoundException">If the patient was not found.</exception>
        public Task<PaginatedResult<Bundle>> GetObservationsFor(string patientId, CustomEventTiming timing, DateTime dateTime,
            PaginationRequest paginationRequest, string patientTimezone = "UTC");
    }
}