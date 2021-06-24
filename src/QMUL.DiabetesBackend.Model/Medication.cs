using System;
using System.Collections.Generic;

namespace QMUL.DiabetesBackend.Model
{
    public class Medication
    {
        public Guid Id { get; set; }
        
        public string Name { get; set; }
        
        public List<string> BrandNames { get; set; }
        
        public string Description { get; set; }
        
        public string DrugAdministrationRoute { get; set; }
    }
}