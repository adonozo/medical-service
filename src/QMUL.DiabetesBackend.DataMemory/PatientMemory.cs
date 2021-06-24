using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class PatientMemory : IPatientDao
    {
        private readonly List<Patient> patients;

        public PatientMemory()
        {
            this.patients = new List<Patient>();
        }

        public List<Patient> GetPatients()
        {
            return this.patients;
        }

        public Patient CreatePatient(Patient newPatient)
        {
            newPatient.Id = Guid.NewGuid();
            this.patients.Add(newPatient);
            return newPatient;
        }
    }
}