namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Implementations
{
    using System;
    using System.Collections.Generic;
    using DataInterfaces;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using NSubstitute;
    using ServiceImpl.Implementations;
    using Xunit;
    using Task = System.Threading.Tasks.Task;

    public class CarePlanServiceTest
    {
        [Fact]
        public async Task GetActiveCarePlans_WhenRequestIsSuccessful_ReturnsBundleWithMedicationAndServices()
        {
            // Arrange
            var serviceRequestDao = Substitute.For<IServiceRequestDao>();
            var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
            var patientDao = Substitute.For<IPatientDao>();
            var logger = Substitute.For<ILogger<CarePlanService>>();
            var carePlanService = new CarePlanService(serviceRequestDao, medicationRequestDao, patientDao, logger);

            medicationRequestDao.GetAllActiveMedicationRequests(Arg.Any<string>())
                .Returns(new List<MedicationRequest> { new() });
            serviceRequestDao.GetActiveServiceRequests(Arg.Any<string>())
                .Returns(new List<ServiceRequest> { new() });
            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(this.GetDummyPatient());

            // Act
            var result = await carePlanService.GetActiveCarePlans(Guid.NewGuid().ToString());

            // Assert
            result.Entry.Count.Should().Be(2);
            result.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(MedicationRequest));
            result.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(ServiceRequest));
        }
        
        [Fact]
        public async Task GetCarePlanFor_WhenRequestIsSuccessful_ReturnsBundleWithMedicationAndServices()
        {
            // Arrange
            var serviceRequestDao = Substitute.For<IServiceRequestDao>();
            var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
            var patientDao = Substitute.For<IPatientDao>();
            var logger = Substitute.For<ILogger<CarePlanService>>();
            var carePlanService = new CarePlanService(serviceRequestDao, medicationRequestDao, patientDao, logger);

            medicationRequestDao.GetMedicationRequestFor(Arg.Any<string>())
                .Returns(new List<MedicationRequest> { new() });
            serviceRequestDao.GetServiceRequestsFor(Arg.Any<string>())
                .Returns(new List<ServiceRequest> { new() });
            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(this.GetDummyPatient());

            // Act
            var result = await carePlanService.GetCarePlanFor(Guid.NewGuid().ToString());

            // Assert
            result.Entry.Count.Should().Be(2);
            result.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(MedicationRequest));
            result.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(ServiceRequest));
        }

        #region Private methods

        private Patient GetDummyPatient()
        {
            return new Patient
            {
                Id = Guid.NewGuid().ToString()
            };
        }
        
        #endregion
    }
}