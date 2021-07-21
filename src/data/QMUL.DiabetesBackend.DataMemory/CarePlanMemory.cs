using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class CarePlanMemory : ICarePlanDao
    {
        #region SampleData

        private readonly List<CarePlan> sampleCarePlans = new()
        {
            new CarePlan
            {
                Id = "5bca2a9e-c3eb-4e60-a66a-9931f892b123",
                Intent = CarePlan.CarePlanIntent.Plan,
                Subject = new ResourceReference
                {
                    Reference = "/patients/fb85c38d-5ea5-4263-ba00-3b9528d4c4b3",
                    Display = "John Doe",
                    ElementId = "fb85c38d-5ea5-4263-ba00-3b9528d4c4b3"
                },
                Period = new Period
                {
                    Start = "2021-07-01",
                    End = "2021-07-31"
                },
                Created = "2021-07-01 10:06:32",
                Status = RequestStatus.Active,
                Activity = new List<CarePlan.ActivityComponent>
                {
                    new()
                    {
                        Reference = new ResourceReference
                        {
                            Reference = "/medicationRequests/3ca91535-8f78-4bc8-b8ca-f95b21d23c8c",
                            Type = nameof(MedicationRequest),
                            ElementId = "3ca91535-8f78-4bc8-b8ca-f95b21d23c8c"
                        },
                        Detail = new CarePlan.DetailComponent
                        {
                            Kind = CarePlan.CarePlanActivityKind.MedicationRequest,
                            Status = CarePlan.CarePlanActivityStatus.InProgress
                        }
                    },
                    new()
                    {
                        Reference = new ResourceReference
                        {
                            Reference = "/serviceRequests/0507447e-7e69-4e8a-89b5-4012791ecb9b",
                            Type = nameof(ServiceRequest),
                            ElementId = "0507447e-7e69-4e8a-89b5-4012791ecb9b"
                        },
                        Detail = new CarePlan.DetailComponent
                        {
                            Kind = CarePlan.CarePlanActivityKind.ServiceRequest,
                            Status = CarePlan.CarePlanActivityStatus.InProgress
                        }
                    }
                }
            }
        };

        #endregion
        
        public CarePlan CreateCarePlan(CarePlan carePlan)
        {
            carePlan.Id = Guid.NewGuid().ToString();
            this.sampleCarePlans.Add(carePlan);
            return carePlan;
        }

        public CarePlan GetCarePlan(string id)
        {
            return this.sampleCarePlans.FirstOrDefault(carePlan => carePlan.Id.Equals(id));
        }

        public List<CarePlan> GetCarePlansFor(string patientId)
        {
            return this.sampleCarePlans.FindAll(carePlan => carePlan.Subject.ElementId.Equals(patientId));
        }

        public CarePlan UpdateCarePlan(string id, CarePlan actualCarePlan)
        {
            var index = this.sampleCarePlans.FindIndex(0, carePlan => carePlan.Id.Equals(id));
            if (index >= 0)
            {
                this.sampleCarePlans[index] = actualCarePlan;
                return actualCarePlan;
            }

            throw new KeyNotFoundException();
        }

        public bool DeleteCarePlan(string id)
        {
            var index = this.sampleCarePlans.FindIndex(0, carePlan => carePlan.Id.Equals(id));
            if (index >= 0)
            {
                this.sampleCarePlans.RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}