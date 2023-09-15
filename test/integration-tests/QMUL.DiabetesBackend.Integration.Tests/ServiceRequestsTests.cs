namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model.Extensions;
using Stubs;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

public class ServiceRequestsTests : IntegrationTestBase
{
    public ServiceRequestsTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateServiceRequests_RecordsData()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var serviceRequest = ServiceRequestStubs.GlucoseMeasureRequest(patientId);

        // Act
        var createResponse = await this.HttpClient.PostResource("service-requests/", serviceRequest);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdResource = await HttpUtils.ParseResult<ServiceRequest>(createResponse.Content);
        createdResource.Id.Should().NotBeNull();
        var resource = await this.GetServiceRequest(createdResource.Id);

        resource.Status.Should().Be(RequestStatus.Draft);
        resource.Intent.Should().Be(RequestIntent.Plan);
        resource.Subject.Should().BeEquivalentTo(serviceRequest.Subject);
        var timing = resource.Occurrence.Should().BeOfType<Timing>().Subject;
        timing.Repeat.Should().BeEquivalentTo(((Timing)serviceRequest.Occurrence).Repeat);
        timing.NeedsStartDate().Should().BeTrue("Duration is 4 weeks without start date");

        var contained = resource.Contained.Should().ContainSingle().Which.Should().BeOfType<ServiceRequest>().Subject;
        contained.Id.Should().BeNull();
        contained.Should().BeEquivalentTo(serviceRequest.Contained[0]);
    }

    [Fact]
    public async Task UpdateServiceRequest_RecordsData()
    {
        // Arrange
        var serviceRequest = await this.CreateServiceRequest();
        var updateRequest = serviceRequest;
        updateRequest.Occurrence = TimingStubs.DatedDuration;
        ((ServiceRequest)updateRequest.Contained[0]).Occurrence = TimingStubs.DatedDurationThuFri;

        // Act
        var updateResponse = await this.HttpClient.PutResource($"service-requests/{serviceRequest.Id}", updateRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var updatedResource = await this.GetServiceRequest(serviceRequest.Id);
        updatedResource.Occurrence.Should().BeEquivalentTo(updateRequest.Occurrence);
        updatedResource.Contained.Should().BeEquivalentTo(updateRequest.Contained);
    }

    [Fact]
    public async Task DeleteServiceRequest_RemovesResource()
    {
        // Arrange
        var serviceRequest = await this.CreateServiceRequest();

        // Act
        var deleteResponse = await this.HttpClient.DeleteAsync($"service-requests/{serviceRequest.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await this.HttpClient.GetAsync($"service-requests/{serviceRequest.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<ServiceRequest> CreateServiceRequest()
    {
        var patientId = await this.CreatePatient();
        var serviceRequest = ServiceRequestStubs.GlucoseMeasureRequest(patientId);
        var createResponse = await this.HttpClient.PostResource("service-requests/", serviceRequest);
        return await HttpUtils.ParseResult<ServiceRequest>(createResponse.Content);
    }

    private async Task<ServiceRequest> GetServiceRequest(string id)
    {
        var resourceJson = await this.HttpClient.GetStringAsync($"service-requests/{id}");
        return await HttpUtils.ParseJson<ServiceRequest>(resourceJson);
    }
}