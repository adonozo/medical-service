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
            TryParse<RequestPriority>(request.Priority, out var priority);
            TryParse<MedicationRequest.medicationrequestStatus>(request.Status, out var status);

            var result = new MedicationRequest
            {
                Id = request.Id,
                Priority = priority,
                Status = status,
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
            TryParse<Timing.UnitsOfTime>(instruction.Timing.PeriodUnit, out var periodUnit);
            var dayEvent = instruction.Timing.When.Select(value =>
            {
                TryParse<Timing.EventTiming>(value, out var result);
                return result;
            });
            var daysOfWeek = instruction.Timing.DaysOfWeek.Select(day =>
            {
                TryParse<DaysOfWeek>(day, out var result);
                return result;
            });
            
            return new Dosage
            {
                Sequence = instruction.Sequence,
                Text = instruction.Text,
                Timing = new Timing
                {
                    Repeat = new Timing.RepeatComponent
                    {
                        Bounds = new Period
                        {
                            Start = instruction.Timing.PeriodStartTime.ToString("yyyy-MM-dd"),
                            End = instruction.Timing.PeriodEndTime.ToString("yyyy-MM-dd")
                        },
                        Frequency = instruction.Timing.Frequency,
                        Period = instruction.Timing.Period,
                        PeriodUnit = periodUnit,
                        Offset = instruction.Timing.Offset,
                        When = (IEnumerable<Timing.EventTiming?>) dayEvent,
                        TimeOfDay = instruction.Timing.TimesOfDay,
                        DayOfWeek = (IEnumerable<DaysOfWeek?>) daysOfWeek
                    }
                },
                DoseAndRate = instruction.DoseAndRate
                    .Select(dose => new Dosage.DoseAndRateComponent {Dose = dose.ToQuantity()})
                    .ToList()
            };
        }

        public static Quantity ToQuantity(this MongoQuantity doseAndRate)
        {
            // var coding = dose.DoseAndRate.Select(mongoCoding => new Coding
            //     {System = mongoCoding.System, Code = mongoCoding.Code, Display = mongoCoding.Value}).ToList();
            return new()
            {
                System = doseAndRate.System,
                Code = doseAndRate.Code,
                Unit = doseAndRate.Unit,
                Value = doseAndRate.Value,
            };
        }
    }
}