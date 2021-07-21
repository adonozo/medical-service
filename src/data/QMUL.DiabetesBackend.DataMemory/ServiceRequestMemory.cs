using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using QMUL.DiabetesBackend.DataInterfaces;

namespace QMUL.DiabetesBackend.DataMemory
{
    public class ServiceRequestMemory : IServiceRequestDao
    {
        #region SampleData
        private List<ServiceRequest> sampleRequests = new()
        {
            new ServiceRequest
            {
                Id = "0507447e-7e69-4e8a-89b5-4012791ecb9b",
                Status = RequestStatus.Active,
                Intent = RequestIntent.Plan,
                Code = new()
                {
                    Coding = new List<Coding>
                    {
                        new()
                        {
                            System = "http://snomed.info/sct",
                            Code = "36048009",
                            Display = "Glucose measurement"
                        }
                    }
                },
                Subject = new ResourceReference
                {
                    Reference = "/patients/fb85c38d-5ea5-4263-ba00-3b9528d4c4b3",
                    ElementId = "fb85c38d-5ea5-4263-ba00-3b9528d4c4b3",
                    Display = "John Doe"
                },
                Occurrence = new Timing
                {
                    Repeat = new Timing.RepeatComponent
                    {
                        Bounds = new Period
                        {
                            Start = "2021-07-01",
                            End = "2021-07-31"
                        },
                        Period = 1,
                        PeriodUnit = Timing.UnitsOfTime.D,
                        Frequency = 1,
                        When = new Timing.EventTiming?[] {Timing.EventTiming.ACM},
                        DayOfWeek = new DaysOfWeek?[] {DaysOfWeek.Mon, DaysOfWeek.Sun}
                    }
                }
            }
        };
        #endregion
        
        public ServiceRequest CreateServiceRequest(ServiceRequest newRequest)
        {
            newRequest.Id = Guid.NewGuid().ToString();
            this.sampleRequests.Add(newRequest);
            return newRequest;
        }

        public ServiceRequest GetServiceRequest(string id)
        {
            return this.sampleRequests.FirstOrDefault(request => request.Id.Equals(id));
        }

        public ServiceRequest UpdateServiceRequest(string id, ServiceRequest actualRequest)
        {
            var index = this.sampleRequests.FindIndex(0, request => request.Id.Equals(id));
            if (index >= 0)
            {
                this.sampleRequests[index] = actualRequest;
                return actualRequest;
            }

            throw new KeyNotFoundException();
        }

        public bool DeleteServiceRequest(string id)
        {
            var index = this.sampleRequests.FindIndex(0, request => request.Id.Equals(id));
            if (index >= 0)
            {
                this.sampleRequests.RemoveAt(index);
                return true;
            }

            return false;
        }
    }
}