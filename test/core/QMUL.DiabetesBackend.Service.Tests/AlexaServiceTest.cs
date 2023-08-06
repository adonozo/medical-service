namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using DataInterfaces;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Enums;
using Model.Extensions;
using NodaTime;
using NSubstitute;
using Service;
using Stubs;
using Xunit;
using Period = Hl7.Fhir.Model.Period;
using Task = System.Threading.Tasks.Task;

public class AlexaServiceTest
{
    [Fact]
    public async Task SearchMedicationRequests_WhenRequestIsSuccessful_ReturnsBundle()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, logger);

        var paginatedResult = new PaginatedResult<IEnumerable<MedicationRequest>>
        {
            Results = new[] { GetTestMedicationRequest(Guid.NewGuid().ToString()) }
        };
        medicationRequestDao.GetActiveMedicationRequests(Arg.Any<string>(), Arg.Any<PaginationRequest>(), false)
            .Returns(paginatedResult);
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        // Act
        var result = await alexaService.SearchMedicationRequests(Guid.NewGuid().ToString(),
            new LocalDate(2023, 01, 01),
            false,
            CustomEventTiming.ALL_DAY);

        // Assert
        result.Should().BeOfType<Bundle>();
    }

    [Fact]
    public async Task SearchServiceRequests_WhenRequestIsSuccessful_ReturnsBundle()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, logger);

        var requestPeriod = new Period{ Start = "2023-01-01", End = "2023-01-10"};
        var expectedRequest = ServiceRequestStubs.ValidPeriodAtFixedTime(requestPeriod);
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());
        serviceRequestDao.GetActiveServiceRequests(Arg.Any<string>())
            .Returns(new List<ServiceRequest> { expectedRequest });

        // Act
        var result = await alexaService.SearchServiceRequests(Guid.NewGuid().ToString(),
            new LocalDate(2023, 01, 02),
            CustomEventTiming.ALL_DAY);

        // Assert
        result.Should().BeOfType<Bundle>();
        var entry = result?.Entry.Should().ContainSingle().Subject;
        var returnedRequest = entry?.Resource.Should().BeOfType<ServiceRequest>().Subject;
        returnedRequest.Should().BeEquivalentTo(expectedRequest);
    }

    [Fact]
    public async Task UpsertTimingEvent_WhenTimingIsMealRelated_UpdatesPatient()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, logger);

        var patient = TestUtils.GetStubPatient();
        var expectedTimingKeys = new[] { CustomEventTiming.CM, CustomEventTiming.ACM, CustomEventTiming.PCM };
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
        patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(Task.FromResult(true));

        // Act
        var result =
            await alexaService.UpsertTimingEvent(Guid.NewGuid().ToString(), CustomEventTiming.CM, new LocalTime(10, 00));
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
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, logger);

        var patient = TestUtils.GetStubPatient();
        var patientLocalTime = new LocalTime(10, 00);
        var expectedLocalTime = new LocalTime(10, 00);
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
        patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(Task.FromResult(true));

        // Act
        var result =
            await alexaService.UpsertTimingEvent(Guid.NewGuid().ToString(), CustomEventTiming.SNACK, patientLocalTime);
        var patientTimings = patient.GetTimingPreference();

        // Assert
        result.Should().Be(true);
        patientTimings.Should().ContainKey(CustomEventTiming.SNACK).And.ContainValue(expectedLocalTime);
    }

    [Fact]
    public async Task UpsertDosageStartDate_WhenRequestIsSuccessful_ReturnsTrue()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, logger);
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
        var expectedDate = new LocalDate(2023, 01, 01);

        // Act
        var result = await alexaService.UpsertDosageStartDateTime(Guid.NewGuid().ToString(), dosageId, expectedDate);
        var dosage = medicationRequest.DosageInstruction.FirstOrDefault(dosage => dosage.ElementId == dosageId);

        // Assert
        result.Should().Be(true);
        dosage.Should().NotBeNull();
        dosage?.Timing.GetStartDate().Should().NotBeNull().And.Be(expectedDate);
    }

    [Fact]
    public async Task SearchMedicationRequests_WhenMedicationRequestHasMultipleDosages_ReturnsSingleMedicationDosage()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao, medicationRequestDao, serviceRequestDao, logger);

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        var dosageId = Guid.NewGuid().ToString();
        
        var medicationRequest = this.GetTestMedicationRequest(dosageId, period: new Period
        {
            Start = "2023-01-01",
            End = "2023-01-10"
        });

        var ignoredDosage = new Dosage
        {
            ElementId = Guid.NewGuid().ToString(),
            Timing = TestUtils.ValidPeriodDurationTiming(new Period { Start = "2023-01-10", End = "2023-01-20"})
        };
        medicationRequest.DosageInstruction.Add(ignoredDosage);

        medicationRequestDao.GetActiveMedicationRequests(Arg.Any<string>(), Arg.Any<PaginationRequest>(), false)
            .Returns(new PaginatedResult<IEnumerable<MedicationRequest>>
            {
                Results = new []{ medicationRequest }
            });

        // Act
        var result = await alexaService.SearchMedicationRequests(Guid.NewGuid().ToString(),
            new LocalDate(2023, 01, 02),
            false);

        // Assert
        var entryResult = result.Entry.Should().ContainSingle().Subject;
        var requestReturned = entryResult.Resource.Should().BeOfType<MedicationRequest>().Subject;
        var dosageReturned = requestReturned.DosageInstruction.Should().ContainSingle().Subject;
        dosageReturned.ElementId.Should().Be(dosageId);
    }

    #region Private methods

    private MedicationRequest GetTestMedicationRequest(string dosageId,
        string medicationRequestId = null,
        Period period = null)
    {
        return new MedicationRequest
        {
            Id = medicationRequestId ?? Guid.NewGuid().ToString(),
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
                            Frequency = 1,
                            Bounds = period ?? new Period
                            {
                                Start = "2023-01-01",
                                End = "2023-01-10"
                            },
                            TimeOfDay = new[] { "10:00" }
                        }
                    }
                }
            }
        };
    }

    #endregion
}