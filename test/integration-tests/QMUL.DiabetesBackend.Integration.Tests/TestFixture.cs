namespace QMUL.DiabetesBackend.Integration.Tests;

using Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Model;

public class TestFixture : WebApplicationFactory<Startup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.Configure<MongoDatabaseSettings>(options =>
            {
                options.DatabaseName = "diabetes.test";
                options.DatabaseConnectionString = "mongodb+srv://localhost:27017/diabetes-test?retryWrites=true&w=majority";
            });
        });
    }
}