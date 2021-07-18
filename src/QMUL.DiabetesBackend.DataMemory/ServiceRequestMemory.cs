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
        private List<ServiceRequest> sampleRequests = new List<ServiceRequest>();
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