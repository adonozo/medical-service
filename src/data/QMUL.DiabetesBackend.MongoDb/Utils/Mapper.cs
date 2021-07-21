using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.MongoDb.Models;
using static System.Enum;

namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    public static class Mapper
    {
        public static MedicationRequest ToMedicationRequest(this MongoMedicationRequest request)
        {
            var hasPriority = TryParse<RequestPriority>(request.Priority, out var priority);
            var hasStatus = TryParse<MedicationRequest.medicationrequestStatus>(request.Status, out var status);

            var result = new MedicationRequest
            {
                Id = request.Id,
                Priority = hasPriority ? priority : null,
                Status = hasStatus ? status : null,
                Note = new List<Annotation>
                {
                    new()
                    {
                        Text = new Markdown(request.Note)
                    }
                },
                AuthoredOn = request.CreatedAt.ToString(CultureInfo.InvariantCulture),
                Subject = new ResourceReference
                {
                    ElementId = request.PatientReference.ReferenceId,
                    Display = request.PatientReference.ReferenceName,
                    Reference = $"/patients/{request.PatientReference.ReferenceName}"
                },
                Requester = new ResourceReference
                {
                    ElementId = request.RequesterReference.ReferenceId,
                    Display = request.RequesterReference.ReferenceName,
                },
                Medication = new ResourceReference
                {
                    ElementId  = request.MedicationReference.ReferenceId,
                    Display = request.MedicationReference.ReferenceName,
                },
                Contained = new List<Resource>
                {
                    new Medication
                    {
                        Id = request.MedicationReference.ReferenceId,
                        
                    }
                },
                DosageInstruction = request.DosageInstructions.Select(ToDosage).ToList()
            };

            return result;
        }

        public static Dosage ToDosage(this MongoDosageInstruction instruction)
        {
            return new Dosage
            {
                Sequence = instruction.Sequence,
                Text = instruction.Text,
                Timing = instruction.Timing.ToTiming(),
                DoseAndRate = instruction.DoseAndRate
                    .Select(dose => new Dosage.DoseAndRateComponent {Dose = dose.ToQuantity()})
                    .ToList()
            };
        }

        public static Timing ToTiming(this MongoTiming timing)
        {
            var hasPeriodUnit = TryParse<Timing.UnitsOfTime>(timing.PeriodUnit, out var periodUnit);
            var dayEvent = new List<Timing.EventTiming?>();
            foreach (var value in timing.When)
            {
                var canParse = TryParse<Timing.EventTiming>(value, out var result);
                if (canParse)
                {
                    dayEvent.Add(result);
                }
            }

            var daysOfWeek = new List<DaysOfWeek?>();
            foreach (var day in timing.DaysOfWeek)
            {
                var canParse = TryParse<DaysOfWeek>(day, out var result);
                if (canParse)
                {
                    daysOfWeek.Add(result);
                }
            }
            
            return new Timing
            {
                Repeat = new Timing.RepeatComponent
                {
                    Bounds = new Period
                    {
                        Start = timing.PeriodStartTime.ToString("yyyy-MM-dd"),
                        End = timing.PeriodEndTime.ToString("yyyy-MM-dd")
                    },
                    Frequency = timing.Frequency,
                    Period = timing.Period,
                    PeriodUnit = hasPeriodUnit ? periodUnit : null,
                    Offset = timing.Offset,
                    When = dayEvent,
                    TimeOfDay = timing.TimesOfDay,
                    DayOfWeek = daysOfWeek
                }
            };
        }

        public static Quantity ToQuantity(this MongoQuantity doseAndRate)
        {
            return new()
            {
                System = doseAndRate.System,
                Code = doseAndRate.Code,
                Unit = doseAndRate.Unit,
                Value = doseAndRate.Value,
            };
        }

        public static MongoMedicationRequest ToMongoMedicationRequest(this MedicationRequest request)
        {
            return new MongoMedicationRequest
            {
                Id = request.Id,
                Priority = request.Priority.ToString(),
                Status = request.Status.ToString(),
                Note = request.Note.Any() ? request.Note[0].Text.Value : string.Empty,
                PatientReference = new MongoReference
                {
                    ReferenceId = request.Subject.ElementId,
                    ReferenceName = request.Subject.Display
                },
                MedicationReference = new MongoReference
                {
                    ReferenceId = request.Medication.ElementId,
                    ReferenceName = (request.Medication as ResourceReference)?.Display
                },
                RequesterReference = new MongoReference
                {
                    ReferenceId = request.Requester.ElementId,
                    ReferenceName = request.Requester.Display
                },
                DosageInstructions = request.DosageInstruction.Select(ToMongoDosageInstruction)
            };
        }

        public static MongoDosageInstruction ToMongoDosageInstruction(this Dosage dosage)
        {
            return new()
            {
                Sequence = dosage.Sequence ?? 0,
                Text = dosage.Text,
                Timing = dosage.Timing.ToMongoTiming(),
                DoseAndRate = dosage.DoseAndRate.Select(ToMongoQuantity) 
            };
        }

        public static MongoTiming ToMongoTiming(this Timing timing)
        {
            var startTime = (timing.Repeat.Bounds as Period)?.Start;
            var endTime = (timing.Repeat.Bounds as Period)?.End;
            var whenList = from eventTiming in timing.Repeat.When
                where eventTiming != null
                select eventTiming.ToString();
            var daysOfWeek = from day in timing.Repeat.DayOfWeek
                where day != null
                select day.ToString();

            return new MongoTiming
            {
                Frequency = timing.Repeat.Frequency ?? 0,
                Period = timing.Repeat.Period ?? 0,
                PeriodUnit = timing.Repeat.PeriodUnit?.ToString(),
                Offset = timing.Repeat.Offset ?? 0,
                PeriodStartTime = DateTime.Parse(startTime ?? string.Empty),
                PeriodEndTime = DateTime.Parse(endTime ?? string.Empty),
                When = whenList,
                DaysOfWeek = daysOfWeek,
                TimesOfDay = timing.Repeat.TimeOfDay.Select(time => time.ToString())
            };
        }

        public static MongoQuantity ToMongoQuantity(this Dosage.DoseAndRateComponent dose)
        {
            return new()
            {
                Value = ((Quantity) dose.Dose).Value ?? 0,
                System = ((Quantity) dose.Dose).System,
                Unit = ((Quantity) dose.Dose).Unit,
                Code = ((Quantity) dose.Dose).Code
            };
        }
    }
}