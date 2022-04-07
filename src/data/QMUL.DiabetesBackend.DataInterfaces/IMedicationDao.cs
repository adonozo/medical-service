namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Exceptions;
    using Hl7.Fhir.Model;
    using Model;

    /// <summary>
    /// The Medication Dao interface.
    /// </summary>
    public interface IMedicationDao
    {
        /// <summary>
        /// Gets all the medications from the database as a <see cref="Medication"/> list.
        /// </summary>
        /// TODO update this comment (and service)
        /// <returns>A <see cref="Medication"/> list.</returns>
        public Task<PaginatedResult<IEnumerable<Resource>>> GetMedicationList(PaginationRequest paginationRequest);

        /// <summary>
        /// Gets a single medication given an ID.
        /// </summary>
        /// <param name="id">The medication ID.</param>
        /// <returns>A single <see cref="Medication"/></returns>
        /// <exception cref="NotFoundException"> If the medication does not exist.</exception>
        public Task<Medication> GetSingleMedication(string id);

        /// <summary>
        /// Inserts a medication into the database. The database adds an ID to the medication after inserting it. 
        /// </summary>
        /// <param name="newMedication">The <see cref="Medication"/> to insert. The ID should be empty.</param>
        /// <returns>The inserted <see cref="Medication"/> with a new ID.</returns>
        /// <exception cref="CreateException">If the medication could not be inserted.</exception>
        public Task<Medication> CreateMedication(Medication newMedication);
    }
}