namespace QMUL.DiabetesBackend.Model;

using System.Collections.Generic;
using FHIR;
using Code = FHIR.Code;

public class ObservationTemplate
{
    public string Id { get; set; }

    public Code Code { get; set; }

    #nullable enable
    public ValueQuantity? ValueTemplate { get; set; }
    #nullable disable

    public IEnumerable<Reference> ReferenceRange { get; set; }

    // TODO add metadata
    // TODO add type (should be shared with observations too)
}