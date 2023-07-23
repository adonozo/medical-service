namespace QMUL.DiabetesBackend.Service.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DataInterfaces;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;
using Model;
using Model.Constants;
using Model.Extensions;
using NSubstitute;
using Service;
using ServiceInterfaces.Utils;
using Xunit;
using ResourceReference = Hl7.Fhir.Model.ResourceReference;
using Task = System.Threading.Tasks.Task;

public class MedicationRequestServiceTest
{
    [Fact]
    public async Task CreateMedicationRequest_WhenMedicationRequestIsCreated_ReturnsNewMedicationRequest()
    {
        // Arrange
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var medicationDao = Substitute.For<IMedicationDao>();
        var eventDao = Substitute.For<IEventDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<MedicationRequestService>>();
        var medicationRequestService =
            new MedicationRequestService(medicationRequestDao, eventDao, patientDao, medicationDao, dataGatherer, logger);

        var medicationRequest = this.GetTestMedicationRequest(Guid.NewGuid().ToString());
        dataGatherer.GetReferenceInternalPatientOrThrow(Arg.Any<ResourceReference>()).Returns(TestUtils.GetStubInternalPatient());
        medicationRequestDao.CreateMedicationRequest(Arg.Any<MedicationRequest>()).Returns(medicationRequest);

        // Act
        var result = await medicationRequestService.CreateMedicationRequest(medicationRequest);

        // Assert
        result.Should().BeOfType<MedicationRequest>();
        await medicationRequestDao.Received(1).CreateMedicationRequest(Arg.Any<MedicationRequest>());
    }

    [Fact]
    public async Task GetMedicationRequest_WhenMedicationRequestExists_ReturnsMedicationRequest()
    {
        // Arrange
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var medicationDao = Substitute.For<IMedicationDao>();
        var eventDao = Substitute.For<IEventDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<MedicationRequestService>>();
        var medicationRequestService =
            new MedicationRequestService(medicationRequestDao, eventDao, patientDao, medicationDao, dataGatherer, logger);

        medicationRequestDao.GetMedicationRequest(Arg.Any<string>()).Returns(new MedicationRequest());

        // Act
        var result = await medicationRequestService.GetMedicationRequest(Guid.NewGuid().ToString());

        // Assert
        result.Should().BeOfType<MedicationRequest>();
        await medicationRequestDao.Received(1).GetMedicationRequest(Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateMedicationRequest_WhenMedicationRequestExists_CallsDaoMethod()
    {
        // Arrange
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var medicationDao = Substitute.For<IMedicationDao>();
        var eventDao = Substitute.For<IEventDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<MedicationRequestService>>();
        var medicationRequestService =
            new MedicationRequestService(medicationRequestDao, eventDao, patientDao, medicationDao, dataGatherer, logger);

        medicationRequestDao.GetMedicationRequest(Arg.Any<string>()).Returns(new MedicationRequest());
        medicationRequestDao.UpdateMedicationRequest(Arg.Any<string>(), Arg.Any<MedicationRequest>())
            .Returns(Task.FromResult(true));

        // Act
        await medicationRequestService.UpdateMedicationRequest(Guid.NewGuid().ToString(),
            new MedicationRequest());

        // Assert
        await medicationRequestDao.Received(1)
            .UpdateMedicationRequest(Arg.Any<string>(), Arg.Any<MedicationRequest>());
    }

    [Fact]
    public async Task DeleteMedicationRequest_WhenMedicationExists_ReturnsTrue()
    {
        // Arrange
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var medicationDao = Substitute.For<IMedicationDao>();
        var eventDao = Substitute.For<IEventDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<MedicationRequestService>>();
        var medicationRequestService =
            new MedicationRequestService(medicationRequestDao, eventDao, patientDao, medicationDao, dataGatherer, logger);

        medicationRequestDao.GetMedicationRequest(Arg.Any<string>()).Returns(new MedicationRequest());
        medicationRequestDao.DeleteMedicationRequest(Arg.Any<string>()).Returns(true);

        // Act
        var result = await medicationRequestService.DeleteMedicationRequest(Guid.NewGuid().ToString());

        // Assert
        result.Should().Be(true);
        await medicationRequestDao.Received(1).DeleteMedicationRequest(Arg.Any<string>());
    }

    [Fact]
    public async Task GetActiveMedicationRequests_WhenPatientExists_ReturnsBundle()
    {
        // Arrange
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var medicationDao = Substitute.For<IMedicationDao>();
        var eventDao = Substitute.For<IEventDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<MedicationRequestService>>();
        var medicationRequestService =
            new MedicationRequestService(medicationRequestDao, eventDao, patientDao, medicationDao, dataGatherer, logger);
        var paginatedResult = new PaginatedResult<IEnumerable<MedicationRequest>>
        {
            Results = new Collection<MedicationRequest>()
        };

        patientDao.GetPatientByIdOrEmail(Arg.Any<string>()).Returns(TestUtils.GetStubPatient());
        medicationRequestDao.GetActiveMedicationRequests(Arg.Any<string>(), Arg.Any<PaginationRequest>(), false)
            .Returns(paginatedResult);

        // Act
        var result = await medicationRequestService.GetActiveMedicationRequests(Guid.NewGuid().ToString(),
            new PaginationRequest(20, null));

        // Assert
        result.Results.Should().BeOfType<Bundle>();
        result.Results.Type.Should().NotBeNull().And.Be(Bundle.BundleType.Searchset);
        await medicationRequestDao.Received(1)
            .GetActiveMedicationRequests(Arg.Any<string>(), Arg.Any<PaginationRequest>(), false);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CheckInsulinRequest_WhenMedicationIsContained_ReturnsExpected(bool isInsulin)
    {
        // Arrange
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var medicationDao = Substitute.For<IMedicationDao>();
        var eventDao = Substitute.For<IEventDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<MedicationRequestService>>();
        var medicationRequestService =
            new MedicationRequestService(medicationRequestDao, eventDao, patientDao, medicationDao, dataGatherer, logger);

        var request = this.GetTestMedicationRequest(Guid.NewGuid().ToString());
        var medication = this.GetMedicationTest();
        medication.SetBoolExtension(isInsulin ? Model.Constants.Extensions.InsulinFlag : string.Empty, isInsulin);
        request.Contained = new List<Resource> { medication };

        // Act
        await medicationRequestService.SetInsulinRequest(request);

        // Assert
        request.HasInsulinFlag().Should().Be(isInsulin);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CheckInsulinRequest_WhenMedicationIsReference_ReturnsExpected(bool isInsulin)
    {
        // Arrange
        var medicationRequestDao = Substitute.For<IMedicationRequestDao>();
        var medicationDao = Substitute.For<IMedicationDao>();
        var eventDao = Substitute.For<IEventDao>();
        var patientDao = Substitute.For<IPatientDao>();
        var dataGatherer = Substitute.For<IDataGatherer>();
        var logger = Substitute.For<ILogger<MedicationRequestService>>();
        var medicationRequestService =
            new MedicationRequestService(medicationRequestDao, eventDao, patientDao, medicationDao, dataGatherer, logger);

        var medicationId = Guid.NewGuid().ToString();
        var medication = this.GetMedicationTest(medicationId);
        medication.SetBoolExtension(isInsulin ? Model.Constants.Extensions.InsulinFlag : string.Empty, isInsulin);
        medicationDao.GetSingleMedication(Arg.Any<string>()).Returns(medication);

        var request = this.GetTestMedicationRequest(Guid.NewGuid().ToString());
        request.Medication = new ResourceReference
        {
            Reference = Constants.MedicationPath + medicationId
        };

        // Act
        await medicationRequestService.SetInsulinRequest(request);

        // Assert
        request.HasInsulinFlag().Should().Be(isInsulin);
    }

    #region Private methods

    private MedicationRequest GetTestMedicationRequest(string id)
    {
        return new MedicationRequest
        {
            Subject = new ResourceReference
            {
                Reference = "Patient/TestPatient"
            },
            Id = id,
            DosageInstruction = new List<Dosage>
            {
                new()
                {
                    ElementId = Guid.NewGuid().ToString(),
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
            },
            Medication = new ResourceReference("#medication-id"),
            Contained = new List<Resource> { this.GetMedicationTest() }
        };
    }

    private Medication GetMedicationTest(string id = "medication-id")
    {
        return new Medication
        {
            Id = id
        };
    }

    #endregion
}