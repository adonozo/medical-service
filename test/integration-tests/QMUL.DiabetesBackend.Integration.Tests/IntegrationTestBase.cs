namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Utils;
using Xunit;

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
}