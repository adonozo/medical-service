using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoTiming
    {
        public int Frequency { get; set; }
        
        public decimal Period { get; set; }
        
        public string PeriodUnit { get; set; }

        /// <summary>
        /// The medication dosage duration. Maps to boundsDuration.Value
        /// </summary>
        public int DayDuration { get; set; }
        
        public IEnumerable<string> When { get; set; }
        
        public int Offset { get; set; }
        
        public IEnumerable<string> DaysOfWeek { get; set; }
        
        /// <summary>
        /// Can be used to hold other meal times i.e., snack. This processing should be done in its respective service.
        /// </summary>
        public IEnumerable<string> TimesOfDay { get; set; }
        
        /// <summary>
        /// The medication start date. Maps to boundsPeriod.Start. Null if the medication is not bounded by dates i.e.,
        /// a duration in days.
        /// </summary>
        [BsonDateTimeOptions]
        public DateTime? PeriodStart { get; set; }
        
        /// <summary>
        /// The medication end date. Maps to boundsPeriod.End. Null if the medication is not bounded by dates i.e.,
        /// a duration in days.
        /// </summary>
        [BsonDateTimeOptions]
        public DateTime? PeriodEnd { get; set; }
    }
}