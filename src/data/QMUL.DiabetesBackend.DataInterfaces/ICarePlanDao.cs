using System.Collections.Generic;
using Hl7.Fhir.Model;

namespace QMUL.DiabetesBackend.DataInterfaces
{
    public interface ICarePlanDao
    {
        public CarePlan CreateCarePlan(CarePlan carePlan);

        public CarePlan GetCarePlan(string id);

        public List<CarePlan> GetCarePlansFor(string patientId);

        public CarePlan UpdateCarePlan(string id, CarePlan carePlan);

        public bool DeleteCarePlan(string id);
    }
}