namespace QMUL.DiabetesBackend.MongoDb.Models
{
    public class MongoQuantity
    {
        public string System { get; set; }
        
        public string Unit { get; set; }
        
        public string Code { get; set; }
        
        public int Value { get; set; }
    }
}