using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.DataMemory;
using QMUL.DiabetesBackend.Model;
using QMUL.DiabetesBackend.MongoDb;
using QMUL.DiabetesBackend.ServiceImpl;
using QMUL.DiabetesBackend.ServiceImpl.Implementations;
using QMUL.DiabetesBackend.ServiceInterfaces;

namespace QMUL.DiabetesBackend.Api
{
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
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "QMUL.DiabetesBackend.Controllers", Version = "v1"});
            });

            services.Configure<MongoDatabaseSettings>(Configuration.GetSection(nameof(MongoDatabaseSettings)));
            services.AddSingleton<IMedicationDao, MedicationMemory>();
            services.AddSingleton<ITreatmentDosageDao, TreatmentMemory>();
            services.AddSingleton<IDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<MongoDatabaseSettings>>().Value);
            services.AddSingleton<IMedicationRequestDao, MedicationRequestDao>();
            services.AddSingleton<ICarePlanDao, CarePlanMemory>();
            services.AddSingleton<IEventDao, MongoEventDao>();
            services.AddSingleton<IPatientDao, PatientDao>();
            services.AddSingleton<IServiceRequestDao, ServiceRequestDao>();
            services.AddSingleton<IObservationDao, ObservationDao>();

            services.AddSingleton<IMedicationService, MedicationService>();
            services.AddSingleton<IPatientService, PatientService>();
            services.AddSingleton<ITreatmentService, TreatmentService>();
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "QMUL.DiabetesBackend.Controllers v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}