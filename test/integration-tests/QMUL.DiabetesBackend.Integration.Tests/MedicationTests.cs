namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

public class MedicationTests : IClassFixture<TestFixture>
{
    private readonly HttpClient httpClient;

    public MedicationTests(TestFixture fixture)
    {
        this.httpClient = fixture.CreateClient();
    }

    [Fact]
    public async Task GetMedications_ReturnsOK()
    {
        var result = await this.httpClient.GetAsync("medications");
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}