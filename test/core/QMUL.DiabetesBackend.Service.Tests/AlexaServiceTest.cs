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
using NodaTime.Testing;
using NSubstitute;
using Service;
using Stubs;
using Xunit;
using Duration = Hl7.Fhir.Model.Duration;
using Instant = NodaTime.Instant;
using Period = Hl7.Fhir.Model.Period;
using Task = System.Threading.Tasks.Task;

public class AlexaServiceTest
{
    private readonly Instant testDate = Instant.FromUtc(2023, 10, 10, 10, 00, 00);

    [Fact]
    public async Task SearchMedicationRequests_WhenRequestIsSuccessful_ReturnsSuccessResult()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var alexaDao = Substitute.For<IAlexaDao>();
        var clock = new FakeClock(this.testDate);

        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao,
            medicationRequestDao,
            serviceRequestDao,
            alexaDao,
            logger,
            clock);

        var expectedRequest = MedicationRequestStubs.ValidMedicationRequestAtFixedTime(
            period: new Period { Start = "2023-01-01", End = "2023-01-10" });
        var paginatedResult = new PaginatedResult<IEnumerable<MedicationRequest>>
        {
            Results = new[] { expectedRequest }
        };
        medicationRequestDao.GetActiveMedicationRequests(Arg.Any<string>(), Arg.Any<PaginationRequest>(), false)
            .Returns(paginatedResult);
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());

        // Act
        var result = await alexaService.SearchMedicationRequests(Guid.NewGuid().ToString(),
            new LocalDate(2023, 01, 02),
            false,
            CustomEventTiming.ALL_DAY);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Results.Entry.Should().ContainSingle()
            .Which.Resource.Should().BeOfType<MedicationRequest>()
            .Which.Should().BeEquivalentTo(expectedRequest);
    }

    [Fact]
    public async Task UpsertDosageStartDate_WhenRequestIsSuccessful_ReturnsTrue()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var alexaDao = Substitute.For<IAlexaDao>();
        var clock = new FakeClock(this.testDate);

        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao,
            medicationRequestDao,
            serviceRequestDao,
            alexaDao,
            logger,
            clock);

        var dosageId = Guid.NewGuid().ToString();
        var medicationRequest = MedicationRequestStubs.ValidMedicationRequestAtFixedTime(dosageId: dosageId,
            period: new Duration { Value = 10, Unit = "d" });

        var patient = TestUtils.GetStubPatient();
        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(patient);
        patientDao.UpdatePatient(Arg.Any<Patient>()).Returns(Task.FromResult(true));

        medicationRequestDao.GetMedicationRequestForDosage(Arg.Any<string>(), Arg.Any<string>())
            .Returns(medicationRequest);
        medicationRequestDao.UpdateMedicationRequest(Arg.Any<string>(),
                Arg.Do<MedicationRequest>(med => medicationRequest = med))
            .Returns(Task.FromResult(true));
        var expectedDate = new LocalDate(2023, 01, 01);
        var expectedTime = new LocalTime(10, 00);

        // Act
        var result = await alexaService.UpsertDosageStartDateTime(Guid.NewGuid().ToString(),
            dosageId,
            expectedDate,
            expectedTime);
        var dosage = medicationRequest.DosageInstruction.Single(dosage => dosage.ElementId == dosageId);

        // Assert
        result.Should().Be(true);
        dosage.Should().NotBeNull();
        dosage.Timing.GetPatientStartDate().Should().NotBeNull().And.Be(expectedDate);
        dosage.Timing.GetPatientStartTime().Should().NotBeNull().And.Be(expectedTime);
    }

    [Fact]
    public async Task SearchMedicationRequests_WhenMedicationRequestHasMultipleDosages_ReturnsSingleMedicationDosage()
    {
        // Arrange
        var patientDao = Substitute.For<IPatientDao>();
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var serviceRequestDao = Substitute.For<IServiceRequestDao>();
        var alexaDao = Substitute.For<IAlexaDao>();
        var clock = new FakeClock(this.testDate);

        var logger = Substitute.For<ILogger<AlexaService>>();
        var alexaService = new AlexaService(patientDao,
            medicationRequestDao,
            serviceRequestDao,
            alexaDao,
            logger,
            clock);

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
        result.IsSuccess.Should().BeTrue();
        var entryResult = result.Results.Entry.Should().ContainSingle().Subject;
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