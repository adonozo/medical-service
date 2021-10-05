namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataInterfaces;
    using Hl7.Fhir.Model;
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

        public CarePlanService(IServiceRequestDao serviceRequestDao,
            IMedicationRequestDao medicationRequestDao, IPatientDao patientDao)
        {
            this.serviceRequestDao = serviceRequestDao;
            this.medicationRequestDao = medicationRequestDao;
            this.patientDao = patientDao;
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetActiveCarePlans(string patientIdOrEmail)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientIdOrEmail);
            var medicationRequests = await this.medicationRequestDao.GetAllActiveMedicationRequests(patient.Id);
            var serviceRequests = await this.serviceRequestDao.GetActiveServiceRequests(patient.Id);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            bundle.Entry.AddRange(serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList());
            return bundle;
        }

        /// <inheritdoc/>>
        public async Task<Bundle> GetCarePlanFor(string patientEmailOrId)
        {
            var patient = await ResourceUtils.ValidateObject(
                () => this.patientDao.GetPatientByIdOrEmail(patientEmailOrId),
                "Unable to find patient for the Observation", new KeyNotFoundException());
            var medicationRequests = await this.medicationRequestDao.GetMedicationRequestFor(patient.Id);
            var serviceRequests = await this.serviceRequestDao.GetServiceRequestsFor(patient.Id);
            var bundle = ResourceUtils.GenerateEmptyBundle();
            bundle.Entry = medicationRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList();
            bundle.Entry.AddRange(serviceRequests.Select(request => new Bundle.EntryComponent {Resource = request})
                .ToList());
            return bundle;
        }
    }
}