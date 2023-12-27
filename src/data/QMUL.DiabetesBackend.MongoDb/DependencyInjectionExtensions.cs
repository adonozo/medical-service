namespace QMUL.DiabetesBackend.MongoDb;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDatabaseSettings = Model.MongoDatabaseSettings;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddMongoDB(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var databaseSettings = sp.GetRequiredService<IOptions<MongoDatabaseSettings>>().Value;
            var client = new MongoClient(databaseSettings.DatabaseConnectionString);
            return client.GetDatabase(databaseSettings.DatabaseName);
        });

        return services;
    }
}