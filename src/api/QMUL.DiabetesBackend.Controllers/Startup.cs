using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using QMUL.DiabetesBackend.DataInterfaces;
using QMUL.DiabetesBackend.DataMemory;
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "QMUL.DiabetesBackend.Controllers", Version = "v1"});
            });

            services.AddSingleton<IMedicationDao, MedicationMemory>();
            services.AddSingleton<IPatientDao, PatientMemory>();
            services.AddSingleton<ITreatmentDosageDao, TreatmentMemory>();
            services.AddSingleton<IMedicationRequestDao, MedicationRequestMemory>();
            services.AddSingleton<IServiceRequestDao, ServiceRequestMemory>();
            services.AddSingleton<IMedicationRequestDao, MedicationRequestMemory>();
            services.AddSingleton<ICarePlanDao, CarePlanMemory>();

            services.AddSingleton<IMedicationService, MedicationService>();
            services.AddSingleton<IPatientService, PatientService>();
            services.AddSingleton<ITreatmentService, TreatmentService>();
            services.AddSingleton<IMedicationRequestService, MedicationRequestService>();
            services.AddSingleton<IServiceRequestService, ServiceRequestService>();
            services.AddSingleton<ICarePlanService, CarePlanService>();
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