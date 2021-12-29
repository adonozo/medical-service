namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Collections.Generic;
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
            this.logger.LogDebug("Found {Count} medications", medications.Count);
            return bundle;
        }

        /// <inheritdoc/>>
        public async Task<Medication> GetSingleMedication(string id)
        {
            return await ResourceUtils.ValidateNullObject(
                () => this.medicationDao.GetSingleMedication(id), new KeyNotFoundException("Unable to find the medication"));
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