namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Net.Http;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.Extensions.DependencyInjection;
using Stubs;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

public abstract class IntegrationTestBase : IClassFixture<TestFixture>, IAsyncLifetime
{
    private readonly MongoDBTest mongoDbTest;
    protected readonly HttpClient HttpClient;

    protected IntegrationTestBase(TestFixture fixture)
    {
        this.HttpClient = fixture.CreateClient();
        var services = fixture.Services;
        this.mongoDbTest = services.GetRequiredService<MongoDBTest>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await this.mongoDbTest.ResetDatabase(TestFixture.TestDatabase);
    }

    protected async Task<string> CreatePatient()
    {
        var patient = PatientStubs.Patient;
        var createResponse = await this.HttpClient.PostResource("patients", patient);
        var parsedResponse = await HttpUtils.ParseResult<Patient>(createResponse.Content);
        return parsedResponse.Id;
    }

    protected async Task<string> CreateLorazepamMedication()
    {
        var medication = MedicationStubs.Lorazepam;
        var createResponse = await this.HttpClient.PostResource("medications", medication);
        var createResult = await HttpUtils.ParseResult<Medication>(createResponse.Content);
        return createResult.Id;
    }

    protected async Task<ServiceRequest> GetServiceRequest(string id)
    {
        var resourceJson = await this.HttpClient.GetStringAsync($"service-requests/{id}");
        return await HttpUtils.ParseJson<ServiceRequest>(resourceJson);
    }
}