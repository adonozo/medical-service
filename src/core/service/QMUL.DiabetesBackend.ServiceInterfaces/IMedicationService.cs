namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    using System.Threading.Tasks;
    using Hl7.Fhir.Model;

    /// <summary>
    /// The Medication Service Interface.
    /// </summary>
    public interface IMedicationService
    {
        /// <summary>
        /// Gets all the <see cref="Medication"/> objects stored. 
        /// </summary>
        /// <returns>A <see cref="Bundle"/> object with the medication list.</returns>
        public Task<Bundle> GetMedicationList();

        /// <summary>
        /// Gets a single <see cref="Medication"/> given an ID.
        /// </summary>
        /// <param name="id">The <see cref="Medication"/> ID to look for.</param>
        /// <returns>A <see cref="Medication"/> object if found. An error otherwise.</returns>
        public Task<Medication> GetSingleMedication(string id);

        /// <summary>
        /// Creates a <see cref="Medication"/>.
        /// </summary>
        /// <param name="newMedication">The <see cref="Medication"/> object to create.</param>
        /// <returns>The created <see cref="Medication"/> with a new ID.</returns>
        public Task<Medication> CreateMedication(Medication newMedication);
    }
}