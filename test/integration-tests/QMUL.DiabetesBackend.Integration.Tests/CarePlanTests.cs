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
        var createdResource = await HttpUtils.ParseResult<CarePlan>(createResponse.Content);
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
      var requestInBundle = detailedCarePlan.Entry.Single(component => component.FullUrl.Contains(serviceRequestId));
      requestInBundle.Resource.Should().BeEquivalentTo(createdServiceRequest);
    }

    private async Task<CarePlan> CreateCarePLan()
    {
        var patientId = await this.CreatePatient();
        var carePlan = CarePlanStubs.CarePlan(patientId);
        var createResponse = await this.HttpClient.PostResource("care-plans", carePlan);
        return await HttpUtils.ParseResult<CarePlan>(createResponse.Content);
    }

    private async Task<CarePlan> GetCarePlan(string id)
    {
        var getResponse = await this.HttpClient.GetStringAsync($"care-plans/{id}");
        return await HttpUtils.ParseJson<CarePlan>(getResponse);
    }

    private async Task<Bundle> GetDetailedCarePlan(string id)
    {
        var getResponse = await this.HttpClient.GetStringAsync($"care-plans/{id}/details");
        return await HttpUtils.ParseJson<Bundle>(getResponse);
    }
}