namespace QMUL.DiabetesBackend.Integration.Tests;

using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model;
using Stubs;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

[Collection(TestFixture.IntegrationTestCollection)]
public class ObservationTests : IntegrationTestBase
{
    public ObservationTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetObservations_ReturnsOK()
    {
        // Arrange
        var patientId = await this.CreatePatient();

        // Act
        var observationsResponse = await this.HttpClient.GetAsync($"patients/{patientId}/observations");

        // Assert
        observationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateObservation_ReturnsOk()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var observation = ObservationStubs.BloodGlucoseReading(patientId);

        // Act
        var createResponse = await this.HttpClient.PostResource("observations", observation);
        var parsedResponse = await HttpUtils.ParseResourceResult<Observation>(createResponse.Content);

        // Assert
        parsedResponse.Id.Should().NotBeNull();
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newObservation = await this.GetObservation(parsedResponse.Id);

        newObservation.Code.Should().BeEquivalentTo(observation.Code);
        newObservation.Subject.Reference.Should().BeEquivalentTo($"Patient/{patientId}");
        newObservation.Subject.Display.Should().BeEquivalentTo("Johnny Smith", "Name is taken from the Patient");
        newObservation.Issued.Should().Be(observation.Issued);
        newObservation.Value.Should().BeEquivalentTo(observation.Value);
        newObservation.ReferenceRange.Should().BeEquivalentTo(observation.ReferenceRange);
    }

    [Fact]
    public async Task UpdateObservation_RecordsChanges()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var initialObservation = ObservationStubs.BloodGlucoseReading(patientId);
        var createResponse = await this.HttpClient.PostResource("observations", initialObservation);
        var parsedResponse = await HttpUtils.ParseResourceResult<Observation>(createResponse.Content);
        var observationId = parsedResponse.Id;

        var updatedObservation = initialObservation;
        updatedObservation.Issued = new DateTimeOffset(2021, 06, 01, 10, 00, 00, TimeSpan.Zero);
        updatedObservation.Effective = new FhirDateTime(2021, 06, 01, 10, 00, 00, TimeSpan.Zero);
        ((Quantity)updatedObservation.Value).Value = 5.1m;

        // Act
        var updateResponse = await this.HttpClient.PutResource($"observations/{observationId}", updatedObservation);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var observation = await this.GetObservation(observationId);
        observation.Issued.Should().Be(updatedObservation.Issued);
        observation.Value.Should().BeEquivalentTo(updatedObservation.Value);
    }

    [Fact]
    public async Task UpdateObservationValue_RecordsNewValue()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var initialObservation = ObservationStubs.BloodGlucoseReading(patientId);
        var createResponse = await this.HttpClient.PostResource("observations", initialObservation);
        var parsedResponse = await HttpUtils.ParseResourceResult<Observation>(createResponse.Content);
        var observationId = parsedResponse.Id;

        var newValue = new DataTypeWrapper
        {
            Value = new
            {
                Code = "mmol/L",
                System = "http://unitsofmeasure.org",
                Unit = "mmol/l",
                Value = 3.5m
            },
            Type = nameof(Quantity)
        };

        // Act
        var updateResponse = await this.HttpClient.Patch($"observations/{observationId}/value", newValue);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var observation = await this.GetObservation(observationId);
        observation.Value.Should().BeEquivalentTo(newValue.Value);
    }

    [Fact]
    public async Task DeleteObservation_SavesChanges()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var initialObservation = ObservationStubs.BloodGlucoseReading(patientId);
        var createResponse = await this.HttpClient.PostResource("observations", initialObservation);
        var parsedResponse = await HttpUtils.ParseResourceResult<Observation>(createResponse.Content);
        var observationId = parsedResponse.Id;

        // Act
        var deleteResponse = await this.HttpClient.DeleteAsync($"observations/{observationId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var observationResponse = await this.HttpClient.GetAsync($"observations/{observationId}");
        observationResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Observation> GetObservation(string observationId)
    {
        var observationResponse = await this.HttpClient.GetAsync($"observations/{observationId}");
        return await HttpUtils.ParseResourceResult<Observation>(observationResponse.Content);
    }
}
