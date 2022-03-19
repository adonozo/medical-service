namespace QMUL.DiabetesBackend.Model
{
    using System;
    using System.Collections.Generic;
    using Enums;

    /// <summary>
    /// A Patient with personal information. A Patient is unique in the system and can be identified by ID or email.
    /// </summary>
    public class Patient
    {
        /// <summary>
        /// This is usually a GUID string. This is not a <see cref="Guid"/> object to keep compatibility with FHIR objects.
        /// </summary>
        public string Id { get; set; }

        public string AlexaUserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public PatientGender Gender { get; set; }

        public DateTime BirthDate { get; set; }

        public IList<PatientPhoneContact> PhoneContacts { get; set; } = new List<PatientPhoneContact>();

        /// <summary>
        /// Holds the exact time the patient has set a specific event in the day: i.e., breakfast, diner, sleep. The date
        /// in the datetime is ignored. 
        /// </summary>
        public Dictionary<CustomEventTiming, DateTime> ExactEventTimes { get; set; } = new();

        /// <summary>
        /// Holds the exact date for a resource to start. Should be used when the resource has a frequency rather than a
        /// period. For example, a medication that must be taken for 14 days. The key is the related resource ID, i.e.,
        /// the dosage ID for a medication request, or the service request ID for a measurement.  
        /// </summary>
        public Dictionary<string, DateTime> ResourceStartDate { get; set; } = new();

        // ReSharper disable once ClassNeverInstantiated.Global
        public class PatientPhoneContact
        {
            // ReSharper disable once UnusedMember.Global
            public string Number { get; set; }

            // ReSharper disable once UnusedMember.Global
            public ContactPointUse Use { get; set; }
        }
    }
}