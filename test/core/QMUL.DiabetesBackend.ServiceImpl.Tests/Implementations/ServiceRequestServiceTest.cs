namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Implementations
{
    using System;
    using System.Collections.Generic;
    using DataInterfaces;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Microsoft.Extensions.Logging;
    using Model;
    using NSubstitute;
    using ServiceImpl.Implementations;
    using Xunit;
    using ResourceReference = Hl7.Fhir.Model.ResourceReference;
    using Task = System.Threading.Tasks.Task;

    public class ServiceRequestServiceTest
    {
        [Fact]
        public async Task CreateServiceRequest_WhenRequestIsSuccessful_ReturnsServiceRequest()
        {
            // Arrange
            var serviceRequestDao = Substitute.For<IServiceRequestDao>();
            var patientDao = Substitute.For<IPatientDao>();
            var eventDao = Substitute.For<IEventDao>();
            var logger = Substitute.For<ILogger<ServiceRequestService>>();
            var serviceRequestService = new ServiceRequestService(serviceRequestDao, patientDao, eventDao, logger);

            var patient = TestUtils.GetStubPatient();
            var serviceRequest = this.GetTestServiceRequest(patient.Id);
            patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
            serviceRequestDao.CreateServiceRequest(Arg.Any<ServiceRequest>()).Returns(serviceRequest);
            eventDao.CreateEvents(Arg.Any<IEnumerable<HealthEvent>>()).Returns(true);

            // Act
            var result = await serviceRequestService.CreateServiceRequest(serviceRequest);

            // Assert
            result.Should().BeOfType<ServiceRequest>();
            await serviceRequestDao.Received(1).CreateServiceRequest(Arg.Any<ServiceRequest>());
        }

        [Fact]
        public async Task GetServiceRequest_WhenServiceRequestExists_ReturnsServiceRequest()
        {
            // Arrange
            var serviceRequestDao = Substitute.For<IServiceRequestDao>();
            var patientDao = Substitute.For<IPatientDao>();
            var eventDao = Substitute.For<IEventDao>();
            var logger = Substitute.For<ILogger<ServiceRequestService>>();
            var serviceRequestService = new ServiceRequestService(serviceRequestDao, patientDao, eventDao, logger);

            serviceRequestDao.GetServiceRequest(Arg.Any<string>()).Returns(new ServiceRequest());
            
            // Act
            var result = await serviceRequestService.GetServiceRequest(Guid.NewGuid().ToString());

            // Assert
            result.Should().BeOfType<ServiceRequest>();
            await serviceRequestDao.Received(1).GetServiceRequest(Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateServiceRequest_WhenRequestIsSuccessful_ReturnsUpdatedServiceRequest()
        {
            // Arrange
            var serviceRequestDao = Substitute.For<IServiceRequestDao>();
            var patientDao = Substitute.For<IPatientDao>();
            var eventDao = Substitute.For<IEventDao>();
            var logger = Substitute.For<ILogger<ServiceRequestService>>();
            var serviceRequestService = new ServiceRequestService(serviceRequestDao, patientDao, eventDao, logger);

            serviceRequestDao.GetServiceRequest(Arg.Any<string>()).Returns(new ServiceRequest());
            serviceRequestDao.UpdateServiceRequest(Arg.Any<string>(), Arg.Any<ServiceRequest>())
                .Returns(new ServiceRequest());

            // Act
            var result =
                await serviceRequestService.UpdateServiceRequest(Guid.NewGuid().ToString(), new ServiceRequest());

            // Assert
            result.Should().BeOfType<ServiceRequest>();
            await serviceRequestDao.Received(1).UpdateServiceRequest(Arg.Any<string>(), Arg.Any<ServiceRequest>());
        }

        [Fact]
        public async Task DeleteServiceRequest_WhenRequestIsSuccessful_CallsDaoMethod()
        {
            // Arrange
            var serviceRequestDao = Substitute.For<IServiceRequestDao>();
            var patientDao = Substitute.For<IPatientDao>();
            var eventDao = Substitute.For<IEventDao>();
            var logger = Substitute.For<ILogger<ServiceRequestService>>();
            var serviceRequestService = new ServiceRequestService(serviceRequestDao, patientDao, eventDao, logger);

            serviceRequestDao.GetServiceRequest(Arg.Any<string>()).Returns(new ServiceRequest());
            serviceRequestDao.DeleteServiceRequest(Arg.Any<string>()).Returns(true);

            // Act
            await serviceRequestService.DeleteServiceRequest(Guid.NewGuid().ToString());

            // Assert
            await serviceRequestDao.Received(1).DeleteServiceRequest(Arg.Any<string>());
        }

        #region Private methods

        private ServiceRequest GetTestServiceRequest(string patientId)
        {
            return new ServiceRequest
            {
                Id = Guid.NewGuid().ToString(),
                Subject = new ResourceReference
                {
                    ElementId = patientId
                },
                Occurrence = new Timing
                {
                    Repeat = new Timing.RepeatComponent
                    {
                        PeriodUnit = Timing.UnitsOfTime.D,
                        Period = 1,
                        Frequency = 2,
                        Bounds = new Duration
                        {
                            Unit = "d",
                            Value = 10
                        }
                    }
                }
            };
        }
        
        #endregion
    }
}