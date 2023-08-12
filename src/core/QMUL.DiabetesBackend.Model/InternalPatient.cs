namespace QMUL.DiabetesBackend.Model;

using System.Collections.Generic;
using Hl7.Fhir.Model;
using NodaTime;

/// <summary>
/// A lightweight Patient object. It is used to avoid the more complex FHIR Patient object
/// </summary>
public class InternalPatient
{
    public string Id { get; set; }

    public string AlexaUserId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public AdministrativeGender? Gender { get; set; }

    public LocalDate? BirthDate { get; set; }

    public IEnumerable<PatientPhone> Phones { get; set; }

    /// <summary>
    /// This class models a FHIR <see cref="ContactPoint"/>.
    /// </summary>
    public class PatientPhone
    {
        public string System { get; set; }

        public string Value { get; set; }

        public string Use { get; set; }

        public int Rank { get; set; }
    }
}