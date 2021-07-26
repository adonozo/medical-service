using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.Model.Enums;
using Patient = QMUL.DiabetesBackend.Model.Patient;

namespace QMUL.DiabetesBackend.ServiceInterfaces
{
    public interface IPatientService
    {
        public Task<List<Patient>> GetPatientList();

        public Task<Patient> CreatePatient(Patient newPatient);

        public Task<Patient> GetPatient(string idOrEmail);

        /// <summary>
        /// Get the list of the patient's care plan. Useful to display as a list to select from.
        /// </summary>
        /// <param name="patientIdOrEmail">The patient's email or ID.</param>
        /// <returns>The list of care plans for the patient.</returns>
        public Task<List<CarePlan>> GetPatientCarePlans(string patientIdOrEmail);
    }
}