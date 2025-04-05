namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model.Extensions;
using Stubs;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

[Collection(TestFixture.IntegrationTestCollection)]
public class CarePlanTests : IntegrationTestBase
{
    public CarePlanTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateCarePlan_RecordsData()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var carePlan = CarePlanStubs.CarePlan(patientId);

        // Act
        var createResponse = await this.HttpClient.PostResource("care-plans", carePlan);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdResource = await HttpUtils.ParseResourceResult<CarePlan>(createResponse.Content);
        createdResource.Id.Should().NotBeNull();

        var carePlanCreated = await this.GetCarePlan(createdResource.Id);
        carePlanCreated.Activity.Should().BeEmpty();
        carePlanCreated.Status.Should().Be(RequestStatus.Draft);
        carePlanCreated.Subject.Should().BeEquivalentTo(carePlan.Subject);
        carePlanCreated.Created.Should().NotBeNull();
    }

    [Fact]
    public async Task AddServiceRequestToCarePlan_RecordsData()
    {
      // Arrange
      var carePlan = await this.CreateCarePLan();
      var serviceRequest = ServiceRequestStubs.GlucoseMeasureRequest(string.Empty);

      // Act
      var addResponse = await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/service-requests", serviceRequest);

      // Assert
      addResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

      var createdCreatePlan = await this.GetCarePlan(carePlan.Id);
      var detailedCarePlan = await this.GetDetailedCarePlan(carePlan.Id);
      createdCreatePlan.Activity.Should().ContainSingle().Which.PlannedActivityReference.Should().NotBeNull();
      detailedCarePlan.Entry.Count.Should().Be(2, "The new service request plus the care plan");

      var serviceRequestId = createdCreatePlan.Activity[0].PlannedActivityReference.GetIdFromReference();
      var createdServiceRequest = await this.GetServiceRequest(serviceRequestId);
      createdServiceRequest.Status.Should().Be(RequestStatus.Draft);
      createdServiceRequest.AuthoredOn.Should().NotBeNull();
      createdServiceRequest.Subject.Should().BeEquivalentTo(carePlan.Subject);

      var requestInBundle = detailedCarePlan.Entry.Single(component => component.FullUrl.Contains(serviceRequestId));
      requestInBundle.Resource.Should().BeEquivalentTo(createdServiceRequest);
    }

    [Fact]
    public async Task AddMedicationRequestToCarePlan_RecordsData()
    {
        // Arrange
        var carePlan = await this.CreateCarePLan();
        var medicationId = await this.CreateLorazepamMedication();
        var medicationRequest = MedicationRequestStubs.MetforminRequest(string.Empty, medicationId);

        // Act
        var addResponse = await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/medication-requests", medicationRequest);

        // Assert
        addResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var createdCreatePlan = await this.GetCarePlan(carePlan.Id);
        var detailedCarePlan = await this.GetDetailedCarePlan(carePlan.Id);
        createdCreatePlan.Activity.Should().ContainSingle().Which.PlannedActivityReference.Should().NotBeNull();
        detailedCarePlan.Entry.Count.Should().Be(2, "The new medication request plus the care plan");

        var medicationRequestId = createdCreatePlan.Activity[0].PlannedActivityReference.GetIdFromReference();
        var createdMedicationRequest = await this.GetMedicationRequest(medicationRequestId);
        createdMedicationRequest.Status.Should().Be(MedicationRequest.MedicationrequestStatus.Draft);
        createdMedicationRequest.AuthoredOn.Should().NotBeNull();
        createdMedicationRequest.Subject.Should().BeEquivalentTo(carePlan.Subject);

        var requestInBundle = detailedCarePlan.Entry.Single(component => component.FullUrl.Contains(medicationRequestId));
        requestInBundle.Resource.Should().BeEquivalentTo(createdMedicationRequest);
    }

    [Fact]
    public async Task ActivateCarePan_UpdateStatuses()
    {
        // Arrange
        var carePlan = await this.CreateCarePLan();
        var serviceRequest = ServiceRequestStubs.GlucoseMeasureRequest(string.Empty);
        var medicationId = await this.CreateLorazepamMedication();
        var medicationRequest = MedicationRequestStubs.MetforminRequest(string.Empty, medicationId);

        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/medication-requests", medicationRequest);
        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/service-requests", serviceRequest);

        // Act
        var activateResponse = await this.HttpClient.PutEmpty($"care-plans/{carePlan.Id}/activate");

        // Assert
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var detailedCarePlan = await this.GetDetailedCarePlan(carePlan.Id);
        detailedCarePlan.Entry.Count.Should().Be(3, "The resources plus the care plan");
        var createdCarePlan = detailedCarePlan.Entry
            .Single(component => component.FullUrl.Contains("CarePlan"))
            .Resource;
        var serviceRequestCreated = detailedCarePlan.Entry
            .Single(component => component.FullUrl.Contains("ServiceRequest"))
            .Resource;
        var medicationRequestCreated = detailedCarePlan.Entry
            .Single(component => component.FullUrl.Contains("MedicationRequest"))
            .Resource;

        createdCarePlan.Should().NotBeNull().And.Subject.As<CarePlan>()
            .Status.Should().Be(RequestStatus.Active);
        serviceRequestCreated.Should().NotBeNull().And.Subject.As<ServiceRequest>()
            .Status.Should().Be(RequestStatus.Active);
        medicationRequestCreated.Should().NotBeNull().And.Subject.As<MedicationRequest>()
            .Status.Should().Be(MedicationRequest.MedicationrequestStatus.Active);
    }

    [Fact]
    public async Task RevokeCarePlan_UpdatesStatuses()
    {
        // Arrange
        var carePlan = await this.CreateCarePLan();
        var serviceRequest = ServiceRequestStubs.GlucoseMeasureRequest(string.Empty);
        var medicationId = await this.CreateLorazepamMedication();
        var medicationRequest = MedicationRequestStubs.MetforminRequest(string.Empty, medicationId);

        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/medication-requests", medicationRequest);
        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/service-requests", serviceRequest);

        // Act
        var revokeResponse = await this.HttpClient.PutEmpty($"care-plans/{carePlan.Id}/revoke");

        // Assert
        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var detailedCarePlan = await this.GetDetailedCarePlan(carePlan.Id);
        detailedCarePlan.Entry.Count.Should().Be(3, "The resources plus the care plan");
        var createdCarePlan = detailedCarePlan.Entry
            .Single(component => component.FullUrl.Contains("CarePlan"))
            .Resource;
        var serviceRequestCreated = detailedCarePlan.Entry
            .Single(component => component.FullUrl.Contains("ServiceRequest"))
            .Resource;
        var medicationRequestCreated = detailedCarePlan.Entry
            .Single(component => component.FullUrl.Contains("MedicationRequest"))
            .Resource;

        createdCarePlan.Should().NotBeNull().And.Subject.As<CarePlan>()
            .Status.Should().Be(RequestStatus.Revoked);
        serviceRequestCreated.Should().NotBeNull().And.Subject.As<ServiceRequest>()
            .Status.Should().Be(RequestStatus.Revoked);
        medicationRequestCreated.Should().NotBeNull().And.Subject.As<MedicationRequest>()
            .Status.Should().Be(MedicationRequest.MedicationrequestStatus.Stopped);
    }

    [Fact]
    public async Task DeleteResourceEntries_RemovesThemFromCarePlan()
    {
        // Arrange
        var carePlan = await this.CreateCarePLan();
        var serviceRequest = ServiceRequestStubs.GlucoseMeasureRequest(string.Empty);
        var medicationId = await this.CreateLorazepamMedication();
        var medicationRequest = MedicationRequestStubs.MetforminRequest(string.Empty, medicationId);

        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/medication-requests", medicationRequest);
        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/service-requests", serviceRequest);

        var createdCreatePlan = await this.GetCarePlan(carePlan.Id);
        var serviceRequestId = createdCreatePlan.Activity.Single(activity => activity.PlannedActivityReference.Reference.Contains("ServiceRequest"))
            .PlannedActivityReference.GetIdFromReference();
        var medicationRequestId = createdCreatePlan.Activity.Single(activity => activity.PlannedActivityReference.Reference.Contains("MedicationRequest"))
            .PlannedActivityReference.GetIdFromReference();

        // Act
        var deleteMedicationResponse = await this.HttpClient.DeleteAsync($"care-plans/{carePlan.Id}/medication-requests/{medicationRequestId}");
        var deleteServiceResponse = await this.HttpClient.DeleteAsync($"care-plans/{carePlan.Id}/service-requests/{serviceRequestId}");

        // Assert
        deleteMedicationResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        deleteServiceResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        createdCreatePlan = await this.GetCarePlan(carePlan.Id);
        createdCreatePlan.Activity.Should().BeEmpty();

        var getMedicationResponse = await this.HttpClient.GetAsync($"medication-requests/{medicationRequestId}");
        getMedicationResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var getServiceResponse = await this.HttpClient.GetAsync($"service-requests/{serviceRequestId}");
        getServiceResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCarePlan_DeletesContainedResources()
    {
        // Arrange
        var carePlan = await this.CreateCarePLan();
        var serviceRequest = ServiceRequestStubs.GlucoseMeasureRequest(string.Empty);
        var medicationId = await this.CreateLorazepamMedication();
        var medicationRequest = MedicationRequestStubs.MetforminRequest(string.Empty, medicationId);

        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/medication-requests", medicationRequest);
        await this.HttpClient.PostResource($"care-plans/{carePlan.Id}/service-requests", serviceRequest);

        var createdCreatePlan = await this.GetCarePlan(carePlan.Id);
        var serviceRequestId = createdCreatePlan.Activity.Single(activity => activity.PlannedActivityReference.Reference.Contains("ServiceRequest"))
            .PlannedActivityReference.GetIdFromReference();
        var medicationRequestId = createdCreatePlan.Activity.Single(activity => activity.PlannedActivityReference.Reference.Contains("MedicationRequest"))
            .PlannedActivityReference.GetIdFromReference();

        // Act
        var deleteResponse = await this.HttpClient.DeleteAsync($"care-plans/{carePlan.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getMedicationResponse = await this.HttpClient.GetAsync($"medication-requests/{medicationRequestId}");
        getMedicationResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var getServiceResponse = await this.HttpClient.GetAsync($"service-requests/{serviceRequestId}");
        getServiceResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<CarePlan> CreateCarePLan()
    {
        var patientId = await this.CreatePatient();
        var carePlan = CarePlanStubs.CarePlan(patientId);
        var createResponse = await this.HttpClient.PostResource("care-plans", carePlan);
        return await HttpUtils.ParseResourceResult<CarePlan>(createResponse.Content);
    }

    private async Task<CarePlan> GetCarePlan(string id)
    {
        var getResponse = await this.HttpClient.GetStringAsync($"care-plans/{id}");
        return await HttpUtils.ParseJsonResource<CarePlan>(getResponse);
    }

    private async Task<Bundle> GetDetailedCarePlan(string id)
    {
        var getResponse = await this.HttpClient.GetStringAsync($"care-plans/{id}/details");
        return await HttpUtils.ParseJsonResource<Bundle>(getResponse);
    }
}