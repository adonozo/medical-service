namespace QMUL.DiabetesBackend.Controllers;

using System.Diagnostics.CodeAnalysis;
using DataInterfaces;
using Hl7.Fhir.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Middlewares;
using Model;
using MongoDb;
using NodaTime;
using Service;
using Service.Utils;
using Service.Validators;
using ServiceInterfaces;
using ServiceInterfaces.Utils;
using ServiceInterfaces.Validators;
using Utils;
using MongoDatabaseSettings = Model.MongoDatabaseSettings;

[ExcludeFromCodeCoverage]
public class Startup
{
    private readonly IConfiguration configuration;

    public Startup(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
        });
        services.AddControllers().AddNewtonsoftJson();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "QMUL.DiabetesBackend.Controllers", Version = "v1" });
        });

        services.AddSingleton<IClock>(SystemClock.Instance);
        services.Configure<MongoDatabaseSettings>(this.configuration.GetSection(nameof(MongoDatabaseSettings)));
        services.AddMongoDB();
        services.AddSingleton<IMedicationRequestDao, MedicationRequestDao>();
        services.AddSingleton<IMedicationDao, MedicationDao>();
        services.AddSingleton<IPatientDao, PatientDao>();
        services.AddSingleton<IServiceRequestDao, ServiceRequestDao>();
        services.AddSingleton<IObservationDao, ObservationDao>();
        services.AddSingleton<ICarePlanDao, CarePlanDao>();
        services.AddSingleton<IAlexaDao, AlexaDao>();
        services.AddSingleton<IObservationTemplateDao, ObservationTemplateDao>();

        services.AddSingleton<IDataGatherer, DataGatherer>();
        services.AddSingleton<IMedicationService, MedicationService>();
        services.AddSingleton<IPatientService, PatientService>();
        services.AddSingleton<IMedicationRequestService, MedicationRequestService>();
        services.AddSingleton<IServiceRequestService, ServiceRequestService>();
        services.AddSingleton<ICarePlanService, CarePlanService>();
        services.AddSingleton<IAlexaService, AlexaService>();
        services.AddSingleton<IObservationService, ObservationService>();
        services.AddSingleton<IObservationTemplateService, ObservationTemplateService>();

        services.AddSingleton<IResourceValidator<Medication>, MedicationValidator>();
        services.AddSingleton<IResourceValidator<CarePlan>, CarePlanValidator>();
        services.AddSingleton<IResourceValidator<MedicationRequest>, MedicationRequestValidator>();
        services.AddSingleton<IResourceValidator<ServiceRequest>, ServiceRequestValidator>();
        services.AddSingleton<IResourceValidator<Observation>, ObservationValidator>();
        services.AddSingleton<ValidatorBase<ObservationTemplate>, ObservationTemplateValidator>();
        services.AddSingleton<IResourceValidator<Patient>, PatientValidator>();
        services.AddSingleton<IDataTypeValidator, DataTypeValidator>();
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

        app.UseRequestCulture();

        app.UseCors(options => options.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders(HttpConstants.LastCursorHeader, HttpConstants.RemainingCountHeader));

        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}