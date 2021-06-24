using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class PatientMemory : IPatientDao
    {
        private readonly List<Patient> patients;

        private readonly List<Patient> samplePatients = new()
        {
            new Patient
            {
                Id = Guid.Parse("fb85c38d-5ea5-4263-ba00-3b9528d4c4b3"),
                AlexaUserId = "78860fa4-ec3a-4c2c-a0cf-c06a363f927b",
                AccessToken = "3d7a29c6ca5d4ed58fa153bcdc9f2fa3",
                FirstName = "John",
                LastName = "Doe",
                Email = "j.doe@gmail.com"
            }
        };

        public PatientMemory()
        {
            this.patients = samplePatients;
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