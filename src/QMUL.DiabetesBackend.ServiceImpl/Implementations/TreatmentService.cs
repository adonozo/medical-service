using System;
using System.Collections.Generic;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class TreatmentService : ITreatmentService
    {
        private readonly ITreatmentDosageDao treatmentDao;

        public TreatmentService(ITreatmentDosageDao treatmentDao)
        {
            this.treatmentDao = treatmentDao;
        }

        public PatientTreatmentDosage CreateTreatment(PatientTreatmentDosage newTreatment)
        {
            throw new NotImplementedException();
        }

        public PatientTreatmentDosage GetSinglePatientTreatment(Guid treatmentId)
        {
            throw new NotImplementedException();
        }

        public List<PatientTreatmentDosage> GetPatientTreatments(Guid patientId)
        {
            throw new NotImplementedException();
        }

        public PatientTreatmentDosage UpdatePatientTreatment(Guid treatmentId, PatientTreatmentDosage treatment)
        {
            throw new NotImplementedException();
        }

        public bool DeletePatientTreatment(Guid treatmentId)
        {
            throw new NotImplementedException();
        }
    }
}