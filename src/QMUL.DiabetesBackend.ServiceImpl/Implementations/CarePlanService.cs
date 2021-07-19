using System.Collections.Generic;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class CarePlanService : ICarePlanService
    {
        private readonly ICarePlanDao carePlanDao;

        public CarePlanService(ICarePlanDao carePlanDao)
        {
            this.carePlanDao = carePlanDao;
        }

        public CarePlan CreateCarePlan(CarePlan carePlan)
        {
            return this.carePlanDao.CreateCarePlan(carePlan);
        }

        public CarePlan GetCarePlan(string id)
        {
            return this.carePlanDao.GetCarePlan(id);
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