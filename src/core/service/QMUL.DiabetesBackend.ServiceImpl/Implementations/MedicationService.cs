namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
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
        public async Task<Bundle> GetMedicationList()
        {
            var bundle = ResourceUtils.GenerateEmptyBundle();
            var medications = await this.medicationDao.GetMedicationList();
            bundle.Entry = medications.Select(medication => new Bundle.EntryComponent {Resource = medication})
                .ToList();
            return bundle;
        }

        /// <inheritdoc/>>
        public async Task<Medication> GetSingleMedication(string id)
        {
            return await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationDao.GetSingleMedication(id), this.logger);
        }

        /// <inheritdoc/>>
        public async Task<Medication> CreateMedication(Medication newMedication)
        {
            var medication = await ExceptionHandler.ExecuteAndHandleAsync(async () =>
                await this.medicationDao.CreateMedication(newMedication), this.logger);
            this.logger.LogDebug("Medication created with ID: {Id}", medication.Id);
            return medication;
        }
    }
}