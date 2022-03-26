namespace QMUL.DiabetesBackend.Model
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Hl7.Fhir.Model;

    /// <summary>
    /// A lightweight Patient object. It is used to avoid the more complex FHIR Patient object
    /// </summary>
    public class InternalPatient
    {
        /// <summary>
        /// This is usually a GUID string. This is not a <see cref="Guid"/> object to keep compatibility with FHIR objects.
        /// </summary>
        public string Id { get; set; }

        public string AlexaUserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public AdministrativeGender? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public IList<Patient.ContactComponent> PhoneContacts { get; set; } = new List<Patient.ContactComponent>();

        /// <summary>
        /// Holds the exact time the patient has set a specific event in the day: i.e., breakfast, diner, sleep. The date
        /// in the datetime is ignored. 
        /// </summary>
        public Dictionary<CustomEventTiming, DateTimeOffset> ExactEventTimes { get; set; } = new();
    }
}