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

    public IList<Reference> ReferenceRange { get; set; }

    public ObservationMetadata Metadata { get; set; }
}