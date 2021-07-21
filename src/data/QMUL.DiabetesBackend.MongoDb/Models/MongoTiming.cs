using System;
using System.Collections.Generic;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoTiming
    {
        public int Frequency { get; set; }
        
        public int Period { get; set; }
        
        public string PeriodUnit { get; set; }
        
        public IEnumerable<string> When { get; set; }
        
        public int Offset { get; set; }
        
        public IEnumerable<string> DayOfWeek { get; set; }
        
        public IEnumerable<string> TimeOfDay { get; set; }
        
        public DateTime PeriodStartTime { get; set; }
        
        public DateTime PeriodEndTime { get; set; }
    }
}