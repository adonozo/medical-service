namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DataInterfaces;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Enums;
using Model.Exceptions;
using Model.Extensions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Service;
using Xunit;
using ResourceReference = Model.ResourceReference;
using Task = System.Threading.Tasks.Task;

public class AlexaServiceTest
{
    [Fact]
    public async Task ProcessMedicationRequest_WhenRequestIsSuccessful_ReturnsBundle()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        // Act
        var result = await alexaService.SearchMedicationRequests(Guid.NewGuid().ToString(), DateTime.Now, false,
            CustomEventTiming.ALL_DAY);

        // Assert
        result.Should().BeOfType<Bundle>();
    }

    [Fact]
    public async Task ProcessGlucoseServiceRequest_WhenRequestIsSuccessful_ReturnsBundle()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        // Act
        var result = await alexaService.ProcessGlucoseServiceRequest(Guid.NewGuid().ToString(), DateTime.Now,
            CustomEventTiming.ALL_DAY);

        // Assert
        result.Should().BeOfType<Bundle>();
    }

    [Fact]
    public async Task GetNextRequests_WhenArgumentHasRequestType_CallsDaoMethodAndReturnsBundle()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());
        eventDao.GetNextEvents(Arg.Any<string>(), Arg.Any<EventType>()).Returns(Array.Empty<HealthEvent>());

        // Act
        var result = await alexaService.GetNextRequests(Guid.NewGuid().ToString(), AlexaRequestType.Glucose);

        // Assert
        await eventDao.Received(1).GetNextEvents(Arg.Any<string>(), Arg.Any<EventType>());
        result.Should().BeOfType<Bundle>();
    }

    [Fact]
    public async Task GetNextRequests_WhenRequestTypeIsAppointment_ReturnsEmptyBundle()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        // Act
        var result = await alexaService.GetNextRequests(Guid.NewGuid().ToString(), AlexaRequestType.Appointment);

        // Assert
        await eventDao.Received(0).GetNextEvents(Arg.Any<string>(), Arg.Any<EventType>());
        result.Should().BeOfType<Bundle>();
        result?.Entry.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNextRequests_WhenArgumentDoesNotHaveRequestType_ReturnsBundleAndCallsMethod()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());
        eventDao.GetNextEvents(Arg.Any<string>(), Arg.Any<EventType[]>()).Returns(Array.Empty<HealthEvent>());

        // Act
        var result = await alexaService.GetNextRequests(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeOfType<Bundle>();
        await eventDao.Received(1).GetNextEvents(Arg.Any<string>(), Arg.Any<EventType[]>());
    }

    [Fact]
    public async Task UpsertTimingEvent_WhenTimingIsMealRelated_UpdatesPatient()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);

        var patient = TestUtils.GetStubPatient();
        var expectedTimingKeys = new[] { CustomEventTiming.CM, CustomEventTiming.ACM, CustomEventTiming.PCM };
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
        patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(Task.FromResult(true));

        // Act
        var result =
            await alexaService.UpsertTimingEvent(Guid.NewGuid().ToString(), CustomEventTiming.CM, DateTime.Now);
        var patientTimings = patient.GetTimingPreference();

        // Assert
        result.Should().Be(true);
        patientTimings.Should().ContainKeys(expectedTimingKeys);
    }

    [Fact]
    public async Task UpsertTimingEvent_WhenTimingIsNotMealRelated_UpdatesPatient()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);

        var patient = TestUtils.GetStubPatient();
        var expectedDate = DateTime.Now;
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
        patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(Task.FromResult(true));

        // Act
        var result =
            await alexaService.UpsertTimingEvent(Guid.NewGuid().ToString(), CustomEventTiming.SNACK, expectedDate);
        var patientTimings = patient.GetTimingPreference();

        // Assert
        result.Should().Be(true);
        patientTimings.Should().ContainKey(CustomEventTiming.SNACK).And.ContainValue(expectedDate);
    }

    [Fact]
    public async Task UpsertDosageStartDate_WhenRequestIsSuccessful_ReturnsTrue()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, eventDao, logger);
        var dosageId = Guid.NewGuid().ToString();
        var medicationRequest = GetTestMedicationRequest(dosageId);

        var patient = TestUtils.GetStubPatient();
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
        patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(Task.FromResult(true));

        medicationRequestDao.GetMedicationRequestForDosage(Arg.Any<string>(), Arg.Any<string>())
            .Returns(medicationRequest);
        medicationRequestDao.UpdateMedicationRequest(Arg.Any<string>(),
                Arg.Do<MedicationRequest>(med => medicationRequest = med))
            .Returns(Task.FromResult(true));
        var expectedDate = DateTime.Now;

        // Act
        var result = await alexaService.UpsertDosageStartDate(Guid.NewGuid().ToString(), dosageId, expectedDate);
        var dosage = medicationRequest.DosageInstruction.FirstOrDefault(dosage => dosage.ElementId == dosageId);

        // Assert
        result.Should().Be(true);
        dosage.Should().NotBeNull();
        dosage?.Timing.GetStartDate().Should().NotBeNull().And.Be(expectedDate);
    }

    [Fact]
    public async Task GetMedicationBundle_WhenMedicationRequestHasMultipleDosages_ReturnsSingleMedicationDosage()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaServiceType = typeof(AlexaService);
        var alexaService = Activator.CreateInstance(alexaServiceType, patientDao, medicationRequestDao,
            serviceRequestDao, eventDao, logger);
        var privateMethod = alexaServiceType
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(method => method.Name == "GetMedicationBundle");

        var dosageId = Guid.NewGuid().ToString();
        var medicationId = Guid.NewGuid().ToString();
        var events = new List<HealthEvent>
        {
            new()
            {
                ResourceReference = new ResourceReference
                {
                    DomainResourceId = medicationId,
                    EventReferenceId = dosageId
                }
            }
        };
        var medicationRequest = this.GetTestMedicationRequest(dosageId);
        medicationRequest.DosageInstruction.Add(new Dosage { ElementId = Guid.NewGuid().ToString() });
        medicationRequestDao.GetMedicationRequestsByIds(Arg.Any<string[]>())
            .Returns(new List<MedicationRequest> { medicationRequest });

        // Act
        var result =
            await (Task<IList<MedicationRequest>>)privateMethod.Invoke(alexaService, new object?[] { events });

        // Assert
        result.Count.Should().Be(1);
        result[0].DosageInstruction.Count.Should().Be(1);
        result[0].DosageInstruction[0].ElementId.Should().Be(dosageId);
    }

    [Fact]
    public async Task GetServiceBundle_WhenEventListHasMultipleServices_MethodCallsUniqueIds()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var eventDao = Substitute.For<IEventDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaServiceType = typeof(AlexaService);
        var alexaService = Activator.CreateInstance(alexaServiceType, patientDao, medicationRequestDao,
            serviceRequestDao, eventDao, logger);
        var privateMethod = alexaServiceType
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(method => method.Name == "GetServiceBundle");

        var serviceId1 = Guid.NewGuid().ToString();
        var serviceId2 = Guid.NewGuid().ToString();
        var events = new List<HealthEvent>
        {
            new() { ResourceReference = new ResourceReference { DomainResourceId = serviceId1 } },
            new() { ResourceReference = new ResourceReference { DomainResourceId = serviceId2 } },
            new() { ResourceReference = new ResourceReference { DomainResourceId = serviceId1 } },
        };
        var expectedIds = Array.Empty<string>();
        serviceRequestDao.GetServiceRequestsByIds(Arg.Do<string[]>(ids => expectedIds = ids))
            .Returns(new List<ServiceRequest>());

        // Act
        await (Task<IList<ServiceRequest>>)privateMethod.Invoke(alexaService, new object?[] { events });

        // Assert
        await serviceRequestDao.Received(1).GetServiceRequestsByIds(Arg.Any<string[]>());
        expectedIds.Length.Should().Be(2);
        expectedIds.Should().Contain(serviceId1).And.Contain(serviceId2);
    }

    #region Private methods

    private MedicationRequest GetTestMedicationRequest(string dosageId, string medicationRequestId = null)
    {
        return new()
        {
            Id = medicationRequestId,
            DosageInstruction = new List<Dosage>
            {
                new()
                {
                    ElementId = dosageId,
                    Timing = new Timing
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
                }
            }
        };
    }

    #endregion
}