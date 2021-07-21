using System.Collections;
using System.Collections.Generic;

namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoDosageInstruction
    {
        public int Sequence { get; set; }
        
        public string Text { get; set; }
        
        public MongoTiming Timing { get; set; }
        
        public IEnumerable<MongoCoding> DoseAndRate { get; set; }
    }
}