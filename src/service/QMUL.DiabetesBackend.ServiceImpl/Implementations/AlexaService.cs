using System;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model.Enums;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.ServiceImpl.Implementations
{
    public class AlexaService : IAlexaService
    {
        private readonly IPatientDao patientDao;
        private readonly IMedicationRequestDao medicationRequestDao;
        private readonly IServiceRequestDao serviceRequestDao;
        private readonly ICarePlanDao carePlanDao;
        private readonly IEventDao eventDao;

        public AlexaService(IPatientDao patientDao, IMedicationRequestDao medicationRequestDao,
            IServiceRequestDao serviceRequestDao, ICarePlanDao carePlanDao, IEventDao eventDao)
        {
            this.patientDao = patientDao;
            this.medicationRequestDao = medicationRequestDao;
            this.serviceRequestDao = serviceRequestDao;
            this.carePlanDao = carePlanDao;
            this.eventDao = eventDao;
        }

        public async Task<Bundle> ProcessRequest(string patientEmailOrId, AlexaRequestType type, DateTime dateTime, AlexaRequestTime requestTime,
            CustomEventTiming timing)
        {
            var patient = await this.patientDao.GetPatientByIdOrEmail(patientEmailOrId);
            switch (requestTime)
            {
                case AlexaRequestTime.ExactTime:
                    break;
                case AlexaRequestTime.AllDay:
                    break;
                case AlexaRequestTime.OnEvent:
                    var events = this.eventDao.GetEvents(patient.Id.ToString(), dateTime, timing);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestTime), requestTime, null);
            }
            
            throw new NotImplementedException();
        }

        public DiagnosticReport SaveGlucoseMeasure(string patientId, DiagnosticReport report)
        {
            throw new NotImplementedException();
        }

        private Tuple<DateTime, DateTime> GetTimeInterval(DateTime dateTime, AlexaRequestTime requestTime,
            Timing.EventTiming timing)
        {
            var startTime = dateTime;
            var endTime = dateTime;
            switch (requestTime)
            {
                case AlexaRequestTime.AllDay:
                    startTime = startTime.Date;
                    endTime = startTime.AddDays(1);
                    break;
                // TODO
                // case AlexaRequestTime.ExactTime:
                //     break;
                // case AlexaRequestTime.OnEvent:
                //     break;
            }

            return new Tuple<DateTime, DateTime>(startTime, endTime);
        }

        private Tuple<DateTime, DateTime> GetIntervalFromEventTiming(DateTime startTime, Timing.EventTiming timing)
        {
            DateTime endTime;
            switch (timing)
            {
                case Timing.EventTiming.MORN:
                    startTime = startTime.Date.AddHours(6);
                    endTime = startTime.Date.AddHours(12);
                    break;
                case Timing.EventTiming.MORN_early:
                    startTime = startTime.Date.AddHours(6);
                    endTime = startTime.Date.AddHours(9);
                    break;
                case Timing.EventTiming.MORN_late:
                    startTime = startTime.Date.AddHours(9);
                    endTime = startTime.Date.AddHours(12);
                    break;
                case Timing.EventTiming.NOON:
                    startTime = startTime.Date.AddHours(11).AddMinutes(30);
                    endTime = startTime.Date.AddHours(12).AddMinutes(30);
                    break;
                case Timing.EventTiming.AFT:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.AFT_early:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(15);
                    break;
                case Timing.EventTiming.AFT_late:
                    startTime = startTime.Date.AddHours(15);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.EVE:
                    startTime = startTime.Date.AddHours(6);
                    endTime = startTime.Date.AddHours(9);
                    break;
                case Timing.EventTiming.EVE_early:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.EVE_late:
                    startTime = startTime.Date.AddHours(9);
                    endTime = startTime.Date.AddHours(12);
                    break;
                case Timing.EventTiming.NIGHT:
                    startTime = startTime.Date.AddHours(18);
                    endTime = startTime.Date.AddHours(30);
                    break;
                case Timing.EventTiming.PHS:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.HS:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.WAKE:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.C:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.CM:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.CD:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.CV:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.AC:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.ACM:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.ACD:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.ACV:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.PC:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.PCM:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.PCD:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                case Timing.EventTiming.PCV:
                    startTime = startTime.Date.AddHours(12);
                    endTime = startTime.Date.AddHours(18);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timing), timing, null);
            }

            return new Tuple<DateTime, DateTime>(startTime, endTime);
        }
    }
}
