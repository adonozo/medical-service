using System;
using System.Collections.Generic;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoTiming
    {
        public int Frequency { get; set; }
        
        public decimal Period { get; set; }
        
        public string PeriodUnit { get; set; }
        
        public IEnumerable<string> When { get; set; }
        
        public int Offset { get; set; }
        
        public IEnumerable<string> DaysOfWeek { get; set; }
        
        /// <summary>
        /// Can be used to hold other meal times i.e., snack. This processing should be done in its respective service.
        /// </summary>
        public IEnumerable<string> TimesOfDay { get; set; }
        
        public DateTime PeriodStartTime { get; set; }
        
        public DateTime PeriodEndTime { get; set; }
    }
}