namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using DataInterfaces;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using NSubstitute;
using Service;
using ServiceInterfaces;
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
        var carePlanDao = Substitute.For<ICarePlanDao>();
        var eventDao = Substitute.For<IEventDao>();
        var serviceRequestService = Substitute.For<IServiceRequestService>();
        var medicationRequestService = Substitute.For<IMedicationRequestService>();
        var logger = Substitute.For<ILogger<CarePlanService>>();
        var carePlanService = new CarePlanService(serviceRequestDao,
            medicationRequestDao,
            patientDao,
            carePlanDao,
            eventDao,
            serviceRequestService,
            medicationRequestService,
            logger);

        medicationRequestDao.GetAllActiveMedicationRequests(Arg.Any<string>())
            .Returns(new List<MedicationRequest> { new() });
        serviceRequestDao.GetActiveServiceRequests(Arg.Any<string>())
            .Returns(new List<ServiceRequest> { new() });
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        // Act
        var result = await carePlanService.GetActiveCarePlans(Guid.NewGuid().ToString());

        // Assert
        result.Should().NotBeNull();
        result?.Entry.Count.Should().Be(2);
        result?.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(MedicationRequest));
        result?.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(ServiceRequest));
    }

    [Fact(Skip = "Update this test to mock `carePlanDao.GetCarePlans`")]
    public async Task GetCarePlanFor_WhenRequestIsSuccessful_ReturnsBundleWithMedicationAndServices()
    {
        // Arrange
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var carePlanDao = Substitute.For<ICarePlanDao>();
        var eventDao = Substitute.For<IEventDao>();
        var serviceRequestService = Substitute.For<IServiceRequestService>();
        var medicationRequestService = Substitute.For<IMedicationRequestService>();
        var logger = Substitute.For<ILogger<CarePlanService>>();
        var carePlanService = new CarePlanService(serviceRequestDao,
            medicationRequestDao,
            patientDao,
            carePlanDao,
            eventDao,
            serviceRequestService,
            medicationRequestService,
            logger);

        medicationRequestDao.GetMedicationRequestFor(Arg.Any<string>())
            .Returns(new List<MedicationRequest> { new() });
        serviceRequestDao.GetServiceRequestsFor(Arg.Any<string>())
            .Returns(new List<ServiceRequest> { new() });
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        // Act
        var result = await carePlanService.GetCarePlansFor(Guid.NewGuid().ToString(),
            new PaginationRequest(20, null));

        // Assert
        result.Should().NotBeNull();
        result.Results.Entry.Count.Should().Be(2);
        result.Results.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(MedicationRequest));
        result.Results.Entry.Should().Contain(entry => entry.Resource.TypeName == nameof(ServiceRequest));
    }
}