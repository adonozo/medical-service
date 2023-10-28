namespace QMUL.DiabetesBackend.Integration.Tests;

using Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Utils;

public class TestFixture : WebApplicationFactory<Startup>
{
    public const string IntegrationTestCollection = "Tests-Collection";
    public const string TestDatabase = "diabetes-test";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Configure<MongoDatabaseSettings>(options =>
            {
                options.DatabaseName = "diabetes-test";
                options.DatabaseConnectionString = "mongodb://localhost:27017/diabetes-test?retryWrites=true&w=majority";
            });

            services.AddSingleton<MongoDBTest>();
        });
    }
}