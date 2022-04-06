namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Exceptions;
    using Hl7.Fhir.Model;
    using Model;

    /// <summary>
    /// The Medication Service Interface.
    /// </summary>
    public interface IMedicationService
    {
        /// <summary>
        /// Gets all the <see cref="Medication"/> objects stored. 
        /// </summary>
        /// <returns>A <see cref="Bundle"/> object with the medication list.</returns>
        public Task<PaginatedResult<Bundle>> GetMedicationList(PaginationRequest paginationRequest);

        /// <summary>
        /// Gets a single <see cref="Medication"/> given an ID.
        /// </summary>
        /// <param name="id">The <see cref="Medication"/> ID to look for.</param>
        /// <returns>A <see cref="Medication"/> object if found. An error otherwise.</returns>
        /// <exception cref="NotFoundException">If the medication was not found.</exception>
        public Task<Medication> GetSingleMedication(string id);

        /// <summary>
        /// Creates a <see cref="Medication"/>.
        /// </summary>
        /// <param name="newMedication">The <see cref="Medication"/> object to create.</param>
        /// <returns>The created <see cref="Medication"/> with a new ID.</returns>
        /// <exception cref="CreateException">If the medication was not created.</exception>
        public Task<Medication> CreateMedication(Medication newMedication);
    }
}