namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using DataInterfaces;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service;
using ServiceInterfaces.Utils;
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
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<ServiceRequestService>>();
        var serviceRequestService = new ServiceRequestService(serviceRequestDao, dataGatherer, logger);

        var patient = TestUtils.GetStubInternalPatient();
        var serviceRequest = this.GetTestServiceRequest(patient.Id);
        dataGatherer.GetReferenceInternalPatientOrThrow(Arg.Any<ResourceReference>()).Returns(patient);
        serviceRequestDao.CreateServiceRequest(Arg.Any<ServiceRequest>()).Returns(serviceRequest);

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
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<ServiceRequestService>>();
        var serviceRequestService = new ServiceRequestService(serviceRequestDao, dataGatherer, logger);

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
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<ServiceRequestService>>();

        var patient = TestUtils.GetStubInternalPatient();
        var serviceRequest = this.GetTestServiceRequest(patient.Id);

        var serviceRequestService = new ServiceRequestService(serviceRequestDao, dataGatherer, logger);
        dataGatherer.GetReferenceInternalPatientOrThrow(Arg.Any<ResourceReference>()).Returns(patient);
        serviceRequestDao.GetServiceRequest(Arg.Any<string>()).Returns(new ServiceRequest());
        serviceRequestDao.UpdateServiceRequest(Arg.Any<string>(), Arg.Any<ServiceRequest>())
            .Returns(Task.FromResult(true));

        // Act
        var result =
            await serviceRequestService.UpdateServiceRequest(Guid.NewGuid().ToString(), serviceRequest);

        // Assert
        result.Should().BeTrue();
        await serviceRequestDao.Received(1).UpdateServiceRequest(Arg.Any<string>(), Arg.Any<ServiceRequest>());
    }

    [Fact]
    public async Task DeleteServiceRequest_WhenRequestIsSuccessful_CallsDaoMethod()
    {
        // Arrange
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<ServiceRequestService>>();

        var serviceRequestService = new ServiceRequestService(serviceRequestDao, dataGatherer, logger);

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