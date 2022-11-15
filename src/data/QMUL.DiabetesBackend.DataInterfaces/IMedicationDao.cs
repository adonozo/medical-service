namespace QMUL.DiabetesBackend.DataInterfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;
    using Model;

    /// <summary>
    /// The Medication Dao interface.
    /// </summary>
    public interface IMedicationDao
    {
        /// <summary>
        /// Gets the medications from the database as a paginated <see cref="Medication"/> list.
        /// </summary>
        /// <param name="paginationRequest">The pagination request parameter.</param>
        /// <param name="name">The medication display name to look for. Maps to the Coding property of the
        /// <see cref="Medication"/> object.</param>
        /// <returns>The paginated list of <see cref="Medication"/>, in a <see cref="PaginatedResult{T}"/> object.</returns>
        public Task<PaginatedResult<IEnumerable<Resource>>> GetMedicationList(PaginationRequest paginationRequest,
            string name = null);

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