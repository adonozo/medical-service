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
            return this.treatmentDao.CreatePatientTreatment(newTreatment);
        }

        public PatientTreatmentDosage GetSinglePatientTreatment(Guid treatmentId)
        {
            return this.treatmentDao.GetSinglePatientTreatment(treatmentId);
        }

        public List<PatientTreatmentDosage> GetPatientTreatments(Guid patientId)
        {
            return this.treatmentDao.GetPatientTreatments(patientId);
        }

        public PatientTreatmentDosage UpdatePatientTreatment(Guid treatmentId, PatientTreatmentDosage treatment)
        {
            return this.treatmentDao.UpdateTreatment(treatment);
        }

        public bool DeletePatientTreatment(Guid treatmentId)
        {
            return this.treatmentDao.DeletePatientTreatment(treatmentId);
        }
    }
}