using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceImpl.Utils;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class CarePlanService : ICarePlanService
    {
        private readonly ICarePlanDao carePlanDao;
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IServiceRequestDao serviceRequestDao;
        private readonly IPatientDao patientDao;

        public CarePlanService(ICarePlanDao carePlanDao, IServiceRequestDao serviceRequestDao,
            IMedicationRequestDao medicationRequestDao, IPatientDao patientDao)
        {
            this.carePlanDao = carePlanDao;
            this.serviceRequestDao = serviceRequestDao;
            this.medicationRequestDao = medicationRequestDao;
            this.patientDao = patientDao;
        }

        public CarePlan CreateCarePlan(CarePlan carePlan)
        {
            return this.carePlanDao.CreateCarePlan(carePlan);
        }

        public CarePlan GetCarePlan(string id)
        {
            return this.carePlanDao.GetCarePlan(id);
        }

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

        public List<CarePlan> GetCarePlanFor(string patientId)
        {
            return this.carePlanDao.GetCarePlansFor(patientId);
        }

        public CarePlan UpdateCarePlan(string id, CarePlan carePlan)
        {
            var exists = this.carePlanDao.GetCarePlan(id) != null;
            if (exists)
            {
                return this.carePlanDao.UpdateCarePlan(id, carePlan);
            }

            throw new KeyNotFoundException();
        }

        public bool DeleteCarePlan(string id)
        {
            var exists = this.carePlanDao.GetCarePlan(id) != null;
            if (exists)
            {
                return this.carePlanDao.DeleteCarePlan(id);
            }

            throw new KeyNotFoundException();
        }
    }
}