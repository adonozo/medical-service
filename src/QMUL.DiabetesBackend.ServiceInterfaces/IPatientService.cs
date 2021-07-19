using System.Collections.Generic;
using Hl7.Fhir.Model;
using Patient = QMUL.DiabetesBackend.Model.Patient;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IPatientService
    {
        public List<Patient> GetPatientList();

        public Patient CreatePatient(Patient newPatient);

        public Patient GetPatient(string idOrEmail);

        public List<CarePlan> GetPatientCarePlans(string patientIdOrEmail);
    }
}