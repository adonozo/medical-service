﻿namespace QMUL.DiabetesBackend.Model
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

        public IEnumerable<PatientPhone> Phones { get; set; }

        /// <summary>
        /// Holds the exact time the patient has set a specific event in the day: i.e., breakfast, diner, sleep. The date
        /// in the datetime is ignored. 
        /// </summary>
        public Dictionary<CustomEventTiming, DateTimeOffset> ExactEventTimes { get; set; } = new();

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
}