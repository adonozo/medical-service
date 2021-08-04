using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.Model.Enums;
using Task = System.Threading.Tasks.Task;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class MedicationRequestMemory : IMedicationRequestDao
    {
        #region SampleData
        private readonly List<MedicationRequest> sampleRequests = new()
        {
            new MedicationRequest
            {
                Id = "3ca91535-8f78-4bc8-b8ca-f95b21d23c8c",
                Priority = RequestPriority.Routine,
                Status = MedicationRequest.medicationrequestStatus.Active,
                Intent = MedicationRequest.medicationRequestIntent.Order,
                Subject = new ResourceReference
                {
                    ElementId = "fb85c38d-5ea5-4263-ba00-3b9528d4c4b3",
                    Display = "John Doe",
                    Reference = "/patients/fb85c38d-5ea5-4263-ba00-3b9528d4c4b3"
                },
                Note = new List<Annotation>
                {
                    new()
                    {
                        Text = new Markdown { Value = "Check blood sugar levels before taking insulin"}
                    }
                },
                Contained = new List<Resource>
                {
                    MedicationMemory.Medications[0]
                },
                DosageInstruction = new List<Dosage>
                {
                    new()
                    {
                        Sequence = 1,
                        Text = "inject 10 units subcut 10 minutes before breakfast",
                        Timing = new Timing
                        {
                            Repeat = new Timing.RepeatComponent
                            {
                                Bounds = new Period
                                {
                                    Start = "2021-07-01",
                                    End = "2021-07-31"
                                },
                                Frequency = 1,
                                Period = 1,
                                PeriodUnit = Timing.UnitsOfTime.D,
                                When = new Timing.EventTiming?[] {Timing.EventTiming.ACM},
                                Offset = 10
                            }
                        },
                        DoseAndRate = new List<Dosage.DoseAndRateComponent>
                        {
                            new()
                            {
                                Type = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new()
                                        {
                                            System = "http://terminology.hl7.org/CodeSystem/dose-rate-type",
                                            Code = "ordered",
                                            Display = "Ordered"
                                        }
                                    }
                                },
                                Dose = new Quantity
                                {
                                    Value = 10,
                                    Unit = "U",
                                    System = "http://unitsofmeasure.org",
                                    Code = "U"
                                }
                            }
                        }
                    },
                    new()
                    {
                        Sequence = 2,
                        Text = "15 units before lunch",
                        Timing = new Timing
                        {
                            Repeat = new Timing.RepeatComponent
                            {
                                Bounds = new Period
                                {
                                    Start = "2021-07-01",
                                    End = "2021-07-31"
                                },
                                Frequency = 1,
                                Period = 1,
                                PeriodUnit = Timing.UnitsOfTime.D,
                                When = new Timing.EventTiming?[] {Timing.EventTiming.ACD}
                            }
                        },
                        DoseAndRate = new List<Dosage.DoseAndRateComponent>
                        {
                            new()
                            {
                                Type = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new()
                                        {
                                            System = "http://terminology.hl7.org/CodeSystem/dose-rate-type",
                                            Code = "ordered",
                                            Display = "Ordered"
                                        }
                                    }
                                },
                                Dose = new Quantity
                                {
                                    Value = 15,
                                    Unit = "U",
                                    System = "http://unitsofmeasure.org",
                                    Code = "U"
                                }
                            }
                        }
                    },
                    new()
                    {
                        Sequence = 3,
                        Text = "20 units before dinner",
                        Timing = new Timing
                        {
                            Repeat = new CustomRepeatComponent
                            {
                                Bounds = new Period
                                {
                                    Start = "2021-07-01",
                                    End = "2021-07-31"
                                },
                                Frequency = 1,
                                Period = 1,
                                PeriodUnit = Timing.UnitsOfTime.D,
                                When = new CustomEventTiming?[] {CustomEventTiming.ACV},
                            }
                        },
                        DoseAndRate = new List<Dosage.DoseAndRateComponent>
                        {
                            new()
                            {
                                Type = new CodeableConcept
                                {
                                    Coding = new List<Coding>
                                    {
                                        new()
                                        {
                                            System = "http://terminology.hl7.org/CodeSystem/dose-rate-type",
                                            Code = "ordered",
                                            Display = "Ordered"
                                        }
                                    }
                                },
                                Dose = new Quantity
                                {
                                    Value = 20,
                                    Unit = "U",
                                    System = "http://unitsofmeasure.org",
                                    Code = "U"
                                }
                            }
                        }
                    }
                }
            }
        };
        #endregion
        
        public Task<MedicationRequest> CreateMedicationRequest(MedicationRequest newRequest)
        {
            newRequest.Id = Guid.NewGuid().ToString();
            this.sampleRequests.Add(newRequest);
            return Task.FromResult(newRequest);
        }

        public Task<MedicationRequest> UpdateMedicationRequest(string id, MedicationRequest actualRequest)
        {
            var index = this.sampleRequests.FindIndex(0, medication => medication.Id.Equals(id));
            if (index < 0)
            {
                throw new KeyNotFoundException();
                
            }

            this.sampleRequests[index] = actualRequest;
            return Task.FromResult(actualRequest);
        }

        public Task<MedicationRequest> GetMedicationRequest(string id)
        {
            var result = this.sampleRequests.FirstOrDefault(medication => medication.Id.Equals(id));
            return Task.FromResult(result);
        }

        public Task<List<MedicationRequest>> GetMedicationRequestsByIds(string[] ids)
        {
            return Task.FromResult(this.sampleRequests);
        }

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime,
            int intervalMin = 10)
        {
            var resultsForPatient = this.sampleRequests.FindAll(medicationRequest =>
                medicationRequest.Subject.ElementId.Equals(patientId)); // Medication requests for the patient
            var result = (from request in resultsForPatient
                from dosage in request.DosageInstruction
                where TimingBetween(dosage.Timing,
                    dateTime,
                    intervalMin)
                select request).ToList();
            return Task.FromResult(result);
        }

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime, Timing.EventTiming timing)
        {
            throw new NotImplementedException();
        }

        public Task<List<MedicationRequest>> GetMedicationRequestFor(string patientId, DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public Task<List<MedicationRequest>> GetNextMedicationRequestFor(string patientId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMedicationRequest(string id)
        {
            var index = this.sampleRequests.FindIndex(0, medication => medication.Id.Equals(id));
            if (index < 0)
            {
                return Task.FromResult(false);
            }
            
            this.sampleRequests.RemoveAt(index);
            return Task.FromResult(true);
        }

        public Task<MedicationRequest> GetMedicationRequestForDosage(string patientId, string dosageId)
        {
            return Task.FromResult(this.sampleRequests[0]);
        }

        public Task<List<MedicationRequest>> GetActiveMedicationRequests(string patientId)
        {
            return Task.FromResult(this.sampleRequests);
        }

        public Task<List<MedicationRequest>> GetAllActiveMedicationRequests(string patientId)
        {
            return Task.FromResult(this.sampleRequests);
        }

        private static bool TimingBetween(Timing timing, DateTime consultedDateTime, int intervalMin)
        {
            // Medication request has a period
            if (timing.Repeat.Bounds is not Period period || !timing.Event.Any())
            {
                return false;
            }

            // Consulted date is between period dates
            var timingStart = DateTime.Parse(period.Start);
            var timingEnd = DateTime.Parse(period.End);

            if (!(consultedDateTime.Ticks > timingStart.Ticks && consultedDateTime.Ticks < timingEnd.Ticks))
            {
                return false;
            }

            // Consulted dateTime is between medication request times
            foreach (var timeEvent in timing.Repeat.TimeOfDay)
            {
                var medicationDateTime = DateTime.Parse($"{consultedDateTime:yyyy-MM-dd}T{timeEvent}");
                if (consultedDateTime.Ticks > medicationDateTime.AddMinutes(intervalMin * -1).Ticks &&
                    consultedDateTime.Ticks < medicationDateTime.AddMinutes(10).Ticks)
                {
                    return true;
                }
            }

            return false;
        }
    }
}