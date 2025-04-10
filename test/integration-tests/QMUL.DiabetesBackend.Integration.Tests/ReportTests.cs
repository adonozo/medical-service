namespace QMUL.DiabetesBackend.Integration.Tests;

using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hl7.Fhir.Model;
using Model;
using Model.Enums;
using Stubs;
using Utils;
using Xunit;
using Code = Model.FHIR.Code;
using Coding = Model.FHIR.Coding;
using Instant = NodaTime.Instant;
using ResourceReference = Model.FHIR.ResourceReference;
using Task = System.Threading.Tasks.Task;

[Collection(TestFixture.IntegrationTestCollection)]
public class ReportTests : IntegrationTestBase
{
    public ReportTests(TestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CreateReport_RecordsData()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var resultId = await this.CreateResult(patientId);

        var report = new DiagnosisReport
        {
            Category = DiagnosisCategory.Hemogram,
            Code = new Code(new Coding("", "", "")),
            Issued = Instant.FromUtc(2025, 01, 01, 12, 0, 0),
            Status = Status.Final,
            EffectiveTimestamp = Instant.FromUtc(2025, 01, 01, 12, 0, 0),
            Subject = new ResourceReference(patientId),
            Results = new []{ new ResourceReference(resultId) },
            Conclusion = "Test conclusion"
        };

        // Act
        var reportResponse = await this.HttpClient.PostJson("reports", report);
        reportResponse.StatusCode.Should().Be(HttpStatusCode.OK, $"report response: {await reportResponse.Content.ReadAsStringAsync()}");
        var parsedResult = await reportResponse.Content.Parse<DiagnosisReport>();

        // Assert
        parsedResult.Id.Should().NotBeNull();
        reportResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var newReport = await GetReport(parsedResult.Id);
        newReport.Should().BeEquivalentTo(parsedResult);
    }

    [Fact]
    public async Task UpdateReport_UpdatesData()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var resultId = await this.CreateResult(patientId);
        var reportId = await CreateReport(patientId, resultId);

        var report = await GetReport(reportId);
        report.Results = Array.Empty<ResourceReference>();

        // Act
        var reportResponse = await this.HttpClient.PutJson($"reports/{reportId}", report);

        // Assert
        reportResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        report = await GetReport(reportId);
        report.Results.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteReport_DeletesData()
    {
        // Arrange
        var patientId = await this.CreatePatient();
        var resultId = await this.CreateResult(patientId);
        var reportId = await CreateReport(patientId, resultId);

        // Act
        var deleteResponse = await this.HttpClient.DeleteAsync($"reports/{reportId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var reportResponse = await this.HttpClient.GetAsync($"reports/{reportId}");
        reportResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<string> CreateResult(string patientId)
    {
        var result = ObservationStubs.BloodGlucoseReading(patientId);
        var resultResponse = await this.HttpClient.PostResource("observations", result);
        var parsedResponse = await HttpUtils.ParseResourceResult<Observation>(resultResponse.Content);
        return parsedResponse.Id;
    }

    private async Task<string> CreateReport(string patientId, string resultId)
    {
        var report = new DiagnosisReport
        {
            Category = DiagnosisCategory.Hemogram,
            Code = new Code(new Coding("", "", "")),
            Issued = Instant.FromUtc(2025, 01, 01, 12, 0, 0),
            Status = Status.Final,
            EffectiveTimestamp = Instant.FromUtc(2025, 01, 01, 12, 0, 0),
            Subject = new ResourceReference(patientId),
            Results = new []{ new ResourceReference(resultId) },
            Conclusion = "Test conclusion"
        };

        var reportResponse = await this.HttpClient.PostJson("reports", report);
        var parsedResponse = await reportResponse.Content.Parse<DiagnosisReport>();
        return parsedResponse.Id;
    }

    private async Task<DiagnosisReport> GetReport(string reportId)
    {
        var reportResponse = await this.HttpClient.GetAsync($"reports/{reportId}");
        return await reportResponse.Content.Parse<DiagnosisReport>();
    }
}