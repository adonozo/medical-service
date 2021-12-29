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
    /// The care plan service handles care plans which are the set of medication and service requests for a patients
    /// (a treatment). 
    /// </summary>
    public class CarePlanService : ICarePlanService
    {
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IServiceRequestDao serviceRequestDao;
        private readonly IPatientDao patientDao;
        private readonly ILogger<CarePlanService> logger;

        public CarePlanService(IServiceRequestDao serviceRequestDao,
            IMedicationRequestDao medicationRequestDao, IPatientDao patientDao, ILogger<CarePlanService> logger)
        {
            this.serviceRequestDao = serviceRequestDao;
            this.medicationRequestDao = medicationRequestDao;
            this.patientDao = patientDao;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Bundle> GetActiveCarePlans(string patientIdOrEmail)
        {
            this.logger.LogTrace("Getting active care plans for {IdOrEmail}", patientIdOrEmail);
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            var medicationRequests = await this.medicationRequestDao.GetAllActiveMedicationRequests(patient.Id);
            var serviceRequests = await this.serviceRequestDao.GetActiveServiceRequests(patient.Id);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            this.logger.LogTrace("Found {Count} medication requests", medicationRequests.Count);
            bundle.Entry.AddRange(serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList());
            this.logger.LogTrace("Found {Count} service requests", serviceRequests.Count);
            return bundle;
        }

        /// <inheritdoc/>
        public async Task<Bundle> GetCarePlanFor(string patientEmailOrId)
        {
            this.logger.LogTrace("Getting care plans for {IdOrEmail}", patientEmailOrId);
            var patient = await ResourceUtils.ValidateNullObject(
                () => this.patientDao.GetPatientByIdOrEmail(patientEmailOrId), new KeyNotFoundException("Unable to find patient."));
            var medicationRequests = await this.medicationRequestDao.GetMedicationRequestFor(patient.Id);
            var serviceRequests = await this.serviceRequestDao.GetServiceRequestsFor(patient.Id);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            this.logger.LogTrace("Found {Count} medication requests", medicationRequests.Count);
            bundle.Entry.AddRange(serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList());
            this.logger.LogTrace("Found {Count} service requests", serviceRequests.Count);
            return bundle;
        }
    }
}