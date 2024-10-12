namespace QMUL.DiabetesBackend.Model.FHIR;

using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;

public record Reference(ValueQuantity Low, ValueQuantity High, IList<Code> AppliesTo = null)
{
    public IList<Code> AppliesTo { get; init; } = new List<Code>();

    public Observation.ReferenceRangeComponent ToFhirReferenceRangeComponent() =>
        new()
        {
            High = this.High.ToFhirQuantity(),
            Low = this.Low.ToFhirQuantity(),
            AppliesTo = this.AppliesTo.Select(a => a.ToFhirCodeableConcept()).ToList()
        };

    public Range ToFhirRange() =>
        new(this.Low.ToFhirQuantity(), this.High.ToFhirQuantity());
}