namespace QMUL.DiabetesBackend.ServiceImpl.Tests
{
    using System;
    using Hl7.Fhir.Model;

    public static class TestUtils
    {
        public static Patient GetDummyPatient()
        {
            return new Patient
            {
                Id = Guid.NewGuid().ToString()
            };
        }
    }
}