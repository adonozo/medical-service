namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model.Extensions;
using Stubs;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

[Collection(TestFixture.IntegrationTestCollection)]
public class MedicationRequestTests : IntegrationTestBase
{
    public MedicationRequestTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateMedicationRequest_RecordsData()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var medicationId = await this.CreateLorazepamMedication();
        var medicationRequest = MedicationRequestStubs.MetforminRequest(patientId, medicationId);

        // Act
        var createResponse = await this.HttpClient.PostResource("medication-requests", medicationRequest);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var parsedResponse = await HttpUtils.ParseResourceResult<MedicationRequest>(createResponse.Content);
        parsedResponse.Id.Should().NotBeNull();

        var resource = await this.GetMedicationRequest(parsedResponse.Id);
        resource.Status.Should().Be(MedicationRequest.MedicationrequestStatus.Draft);
        resource.Priority.Should().Be(medicationRequest.Priority);
        resource.Intent.Should().Be(medicationRequest.Intent);
        resource.AuthoredOn.Should().NotBeNull();
        resource.Subject.Should().BeEquivalentTo(medicationRequest.Subject);
        resource.Medication.Should().BeEquivalentTo(medicationRequest.Medication);
        resource.DosageInstruction[0].ElementId.Should().NotBeNull();
        resource.DosageInstruction[0].Timing.Repeat.Should().BeEquivalentTo(medicationRequest.DosageInstruction[0].Timing.Repeat);
        resource.DosageInstruction[0].Timing.NeedsStartDate().Should().BeTrue("Duration is 4 weeks without start date");
        resource.DosageInstruction[0].DoseAndRate.Should().BeEquivalentTo(medicationRequest.DosageInstruction[0].DoseAndRate);
    }

    [Fact]
    public async Task UpdateMedicationRequest_RecordsData()
    {
        // Arrange
        var createdRequest = await this.CreateMedicationRequest();
        var updatedRequest = createdRequest;
        createdRequest.DosageInstruction[0].Timing = TimingStubs.DatedDuration;

        // Act
        var updateResponse = await this.HttpClient.PutResource($"medication-requests/{createdRequest.Id}", updatedRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedResource = await this.GetMedicationRequest(createdRequest.Id);
        updatedResource.DosageInstruction.Should().BeEquivalentTo(updatedRequest.DosageInstruction);
    }

    [Fact]
    public async Task DeleteMedicationRequest_RemovesResource()
    {
        // Arrange
        var createdRequest = await this.CreateMedicationRequest();

        // Act
        var deleteResponse = await this.HttpClient.DeleteAsync($"medication-requests/{createdRequest.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var resourceRequest = await this.HttpClient.GetAsync($"medication-requests/{createdRequest.Id}");
        resourceRequest.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<MedicationRequest> CreateMedicationRequest()
    {
        var patientId = await this.CreatePatient();
        var medicationId = await this.CreateLorazepamMedication();
        var medicationRequest = MedicationRequestStubs.MetforminRequest(patientId, medicationId);
        var createResponse = await this.HttpClient.PostResource($"medication-requests", medicationRequest);
        return await HttpUtils.ParseResourceResult<MedicationRequest>(createResponse.Content);
    }
}