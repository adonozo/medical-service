namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Model;
using NodaTime;
using Stubs;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

[Collection(TestFixture.IntegrationTestCollection)]
public class PatientTests : IntegrationTestBase
{
    public PatientTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetPatients_ReturnsOK()
    {
        var result = await this.HttpClient.GetAsync("patients");
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePatientWithoutEmail_ReturnsCreatedPatient()
    {
        // Arrange
        var patient = PatientStubs.PatientWithoutEmail;

        // Act
        var createResponse = await this.HttpClient.PostResource("patients", patient);
        var parsedResponse = await HttpUtils.ParseResourceResult<Patient>(createResponse.Content);

        // Assert
        parsedResponse.Id.Should().NotBeNull();
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var newPatient = await this.GetPatient(parsedResponse.Id);

        newPatient.Active.Should().BeTrue();
        newPatient.Gender.Should().Be(AdministrativeGender.Male);
        newPatient.BirthDate.Should().Be("1990-01-01");
        newPatient.Telecom.Should().BeEquivalentTo(patient.Telecom);
        newPatient.Name.Should().BeEquivalentTo(patient.Name);
    }

    [Fact]
    public async Task AmendPatientData_StoresDataCorrectly()
    {
        // Arrange
        var patient = PatientStubs.Patient;
        var createResponse = await this.HttpClient.PostResource("patients", patient);
        var parsedResponse = await HttpUtils.ParseResourceResult<Patient>(createResponse.Content);
        parsedResponse.Id.Should().NotBeNull();

        var updatedPatient = new InternalPatient
        {
            FirstName = "John Updated",
            LastName = "Doe Updated",
            BirthDate = new LocalDate(1980, 01, 01),
            Gender = AdministrativeGender.Female
        };

        // Act
        var updateResponse = await this.HttpClient.Patch($"patients/{parsedResponse.Id}", updatedPatient);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        patient = await this.GetPatient(parsedResponse.Id);
        patient.Name[0].Family.Should().Be(updatedPatient.LastName);
        string.Join(' ', patient.Name[0].Given).Should().Be(updatedPatient.FirstName);
        patient.BirthDate.Should().Be("1980-01-01");
        patient.Gender.Should().Be(AdministrativeGender.Female);
    }

    [Fact]
    public async Task UpdatePatient_StoresDataCorrectly()
    {
        // Arrange
        var patient = PatientStubs.Patient;
        var createResponse = await this.HttpClient.PostResource("patients", patient);
        var parsedResponse = await HttpUtils.ParseResourceResult<Patient>(createResponse.Content);
        parsedResponse.Id.Should().NotBeNull();

        patient.Id = parsedResponse.Id;
        patient.BirthDate = "1970-01-01";
        patient.Gender = AdministrativeGender.Unknown;
        patient.Telecom = new List<ContactPoint>
        {
            new()
            {
                System = ContactPoint.ContactPointSystem.Phone,
                Use = ContactPoint.ContactPointUse.Work,
                Rank = 0,
                Value = "+44 020990874481"
            }
        };

        // Act
        var updatedResponse = await this.HttpClient.PutResource($"patients/{parsedResponse.Id}", patient);

        // Assert
        updatedResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var updatePatient = await this.GetPatient(parsedResponse.Id);
        updatePatient.ToJObject().Should().BeEquivalentTo(patient.ToJObject());
    }

    private async Task<Patient> GetPatient(string id)
    {
        var patientJson = await this.HttpClient.GetStringAsync($"patients/{id}");
        return await HttpUtils.ParseJsonResource<Patient>(patientJson);
    }
}