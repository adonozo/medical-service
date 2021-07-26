using System;

namespace QMUL.DiabetesBackend.Model
{
    public class Patient
    {
        public Guid Id { get; set; }
        
        public string AlexaUserId { get; set; }
        
        public string AccessToken { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }
    }
}