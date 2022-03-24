namespace QMUL.DiabetesBackend.Api
{
    using System.Diagnostics.CodeAnalysis;
    using DataInterfaces;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.OpenApi.Models;
    using MongoDb;
    using MongoDB.Driver;
    using MongoDb.Utils;
    using ServiceImpl.Implementations;
    using ServiceInterfaces;
    using MongoDatabaseSettings = Model.MongoDatabaseSettings;

    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "QMUL.DiabetesBackend.Controllers", Version = "v1"});
            });

            services.AddAutoMapper(typeof(MapperProfile));
            services.Configure<MongoDatabaseSettings>(Configuration.GetSection(nameof(MongoDatabaseSettings)));
            services.AddSingleton(sp =>
            {
                var databaseSettings = sp.GetRequiredService<IOptions<MongoDatabaseSettings>>().Value;
                var client = new MongoClient(databaseSettings.DatabaseConnectionString);
                return client.GetDatabase(databaseSettings.DatabaseName);
            });
            services.AddSingleton<IMedicationRequestDao, MedicationRequestDao>();
            services.AddSingleton<IMedicationDao, MedicationDao>();
            services.AddSingleton<IEventDao, MongoEventDao>();
            services.AddSingleton<IPatientDao, PatientDao>();
            services.AddSingleton<IServiceRequestDao, ServiceRequestDao>();
            services.AddSingleton<IObservationDao, ObservationDao>();

            services.AddSingleton<IMedicationService, MedicationService>();
            services.AddSingleton<IPatientService, PatientService>();
            services.AddSingleton<IMedicationRequestService, MedicationRequestService>();
            services.AddSingleton<IServiceRequestService, ServiceRequestService>();
            services.AddSingleton<ICarePlanService, CarePlanService>();
            services.AddSingleton<IAlexaService, AlexaService>();
            services.AddSingleton<IObservationService, ObservationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "QMUL.DiabetesBackend.Controllers v1"));
            }

            app.UseRouting();

            app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}