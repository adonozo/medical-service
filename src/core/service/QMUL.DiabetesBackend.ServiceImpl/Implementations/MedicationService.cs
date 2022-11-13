namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using ServiceInterfaces;
    using Utils;

    /// <summary>
    /// Manages Medications
    /// </summary>
    public class MedicationService : IMedicationService
    {
        private readonly IMedicationDao medicationDao;
        private readonly ILogger<MedicationService> logger;

        public MedicationService(IMedicationDao medicationDao, ILogger<MedicationService> logger)
        {
            this.medicationDao = medicationDao;
            this.logger = logger;
        }

        /// <inheritdoc/>>
        public async Task<PaginatedResult<Bundle>> GetMedicationList(PaginationRequest paginationRequest, string name = null)
        {
            var paginatedMedications = await this.medicationDao.GetMedicationList(paginationRequest, name);
            return paginatedMedications.ToBundleResult();
        }

        /// <inheritdoc/>>
        public Task<Medication> GetMedication(string id)
        {
            return this.medicationDao.GetSingleMedication(id);
        }

        /// <inheritdoc/>>
        public async Task<Medication> CreateMedication(Medication newMedication)
        {
            var medication = await this.medicationDao.CreateMedication(newMedication);
            this.logger.LogDebug("Medication created with ID: {Id}", medication.Id);
            return medication;
        }
    }
}