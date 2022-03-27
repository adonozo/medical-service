namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Exceptions;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Observation Dao interface.
    /// </summary>
    public interface IObservationDao
    {
        /// <summary>
        /// Creates a given <see cref="Observation"/>. It generates an ID.
        /// </summary>
        /// <param name="observation">The <see cref="Observation"/> to insert to the Database</param>
        /// <returns>The inserted <see cref="Observation"/> with a new ID.</returns>
        /// <exception cref="CreateException">If the observation is not created.</exception>
        public Task<Observation> CreateObservation(Observation observation);

        /// <summary>
        /// Gets a single observation given an ID.
        /// </summary>
        /// <param name="observationId">The observation ID to look for.</param>
        /// <returns>A <see cref="Observation"/></returns>
        /// <exception cref="NotFoundException">If the observation is not found.</exception>
        public Task<Observation> GetObservation(string observationId);

        /// <summary>
        /// Gets all the observations for a given patient.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <returns>The patient's list of <see cref="Observation"/></returns>
        public Task<List<Observation>> GetAllObservationsFor(string patientId);

        /// <summary>
        /// Gets the list of <see cref="Observation"/> for a given patient in a defined time range.
        /// </summary>
        /// <param name="patientId">The patient ID.</param>
        /// <param name="start">The range start datetime.</param>
        /// <param name="end">The range end datetime.</param>
        /// <returns>An <see cref="Observation"/> list within the start and end dates.</returns>
        public Task<List<Observation>> GetObservationsFor(string patientId, DateTime start, DateTime end);
    }
}