namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using System.Collections.Generic;
using Hl7.Fhir.Model;

public static class PatientStubs
{
    public static readonly Patient PatientWithoutEmail = new()
    {
        Active = true,
        Name = new List<HumanName>
        {
            new()
            {
                Use = HumanName.NameUse.Official,
                Family = "Smith",
                Given = new[] { "Johnny" }
            }
        },
        Gender = AdministrativeGender.Male,
        BirthDate = "1990-01-01",
        Telecom = new List<ContactPoint>
        {
            new()
            {
                System = ContactPoint.ContactPointSystem.Phone,
                Use = ContactPoint.ContactPointUse.Home,
                Rank = 0,
                Value = "+44 074280874481"
            }
        }
    };

    public static readonly Patient Patient = new()
    {
        Active = true,
        Name = new List<HumanName>
        {
            new()
            {
                Use = HumanName.NameUse.Official,
                Family = "Smith",
                Given = new[] { "Johnny" }
            }
        },
        Gender = AdministrativeGender.Male,
        BirthDate = "1990-01-01",
        Telecom = new List<ContactPoint>
        {
            new()
            {
                System = ContactPoint.ContactPointSystem.Phone,
                Use = ContactPoint.ContactPointUse.Home,
                Rank = 0,
                Value = "+44 074280874481"
            }
        },
        Extension = new List<Extension>
        {
            new()
            {
                Url = "http://hl7.org/fhir/StructureDefinition/Email",
                Value = new FhirString("johnDoe@mail.com")
            }
        }
    };
}