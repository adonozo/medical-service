namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Hl7.Fhir.Model;
    using Models;
    using static System.Enum;

    /// <summary>
    /// Maps FHIR Medication Request objects into custom Mongo objects and vice-versa.
    /// </summary>
    public static class MedicationRequestMapper
    {
        public static MedicationRequest ToMedicationRequest(this MongoMedicationRequest request)
        {
            var hasPriority = TryParse<RequestPriority>(request.Priority, out var priority);
            var hasStatus = TryParse<MedicationRequest.medicationrequestStatus>(request.Status, out var status);
            var insulin = request.IsInsulin
                ? new Extension {Url = "http://diabetesreminder.com/insulin", Value = new FhirBoolean(true)}
                : null;

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
                Subject = request.PatientReference.ToResourceReference(),
                Requester = request.RequesterReference.ToResourceReference(),
                Medication = request.MedicationReference.ToResourceReference(),
                Contained = new List<Resource>
                {
                    new Medication
                    {
                        Id = request.MedicationReference.ReferenceId,
                    }
                },
                DosageInstruction = request.DosageInstructions.Select(ToDosage).ToList(),
                Extension = new List<Extension> { insulin }
            };

            return result;
        }

        public static Dosage ToDosage(this MongoDosageInstruction instruction)
        {
            return new()
            {
                ElementId = instruction.Id,
                Sequence = instruction.Sequence,
                Text = instruction.Text,
                Timing = instruction.Timing.ToTiming(),
                DoseAndRate = instruction.DoseAndRate
                    .Select(dose => new Dosage.DoseAndRateComponent {Dose = dose.ToQuantity()})
                    .ToList()
            };
        }

        public static MongoMedicationRequest ToMongoMedicationRequest(this MedicationRequest request)
        {
            var isInsulin = false;
            var extensions = request.Extension;
            if (extensions != null && extensions.Any(extension => extension.Url.ToLower().Contains("insulin")))
            {
                isInsulin = true;
            }

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
                IsInsulin = isInsulin,
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
                DoseAndRate = dosage.DoseAndRate.Where(dose => dose.Dose is Quantity).Select(dose => ((Quantity)dose.Dose).ToMongoQuantity())
            };
        }
    }
}
