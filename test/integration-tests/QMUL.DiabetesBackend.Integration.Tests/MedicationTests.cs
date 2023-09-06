namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Net;
using FluentAssertions;
using Hl7.Fhir.Model;
using Stubs;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

[Collection(TestFixture.IntegrationTestCollection)]
public class MedicationTests : IntegrationTestBase
{
    public MedicationTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetMedications_ReturnsOK()
    {
        var result = await this.HttpClient.GetAsync("medications");
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateMedication_ReturnsObject()
    {
        // Arrange
        var medication = MedicationStubs.GetTestMedication();

        // Act
        var createResponse = await this.HttpClient.PostResource("medications", medication);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResult = await HttpUtils.ParseResult<Medication>(createResponse.Content);
        createResult.Id.Should().NotBeNull();

        var getResponse = await this.HttpClient.GetStringAsync($"medications/{createResult.Id}");
        var savedMedication = await HttpUtils.ParseJson<Medication>(getResponse);
        savedMedication.Code.Should().BeEquivalentTo(medication.Code);
        savedMedication.DoseForm.Should().BeEquivalentTo(medication.DoseForm);
        savedMedication.Ingredient.Should().BeEquivalentTo(medication.Ingredient);
    }
}
