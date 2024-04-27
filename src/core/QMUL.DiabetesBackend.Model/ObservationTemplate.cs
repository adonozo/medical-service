namespace QMUL.DiabetesBackend.Model;

using System.Collections.Generic;
using FHIR;
using Code = FHIR.Code;

public class ObservationTemplate
{
    public Code Code { get; init; }

    #nullable enable
    public ValueQuantity? ValueTemplate { get; init; }
    #nullable disable

    public IEnumerable<Reference> ReferenceRange { get; init; }
}