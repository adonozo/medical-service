namespace QMUL.DiabetesBackend.Integration.Tests;

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.DependencyInjection;
using Utils;
using Xunit;
using Task = System.Threading.Tasks.Task;

public class MedicationTests : IClassFixture<TestFixture>, IAsyncLifetime
{
    private readonly HttpClient httpClient;
    private readonly MongoDBTest mongoDbTest;

    public MedicationTests(TestFixture fixture)
    {
        this.httpClient = fixture.CreateClient();
        var services = fixture.Services;
        this.mongoDbTest = services.GetRequiredService<MongoDBTest>();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await this.mongoDbTest.ResetDatabase(TestFixture.TestDatabase);
    }

    [Fact]
    public async Task GetMedications_ReturnsOK()
    {
        var result = await this.httpClient.GetAsync("medications");
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateMedication_ReturnsObject()
    {
        // Arrange
        var medication = this.GetTestMedication();

        // Act
        var createResponse = await this.httpClient.PostResource("medications", medication);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var createResult = await HttpUtils.ParseResult<Medication>(createResponse.Content);
        createResult.Id.Should().NotBeNull();

        var getResponse = await this.httpClient.GetStringAsync($"medications/{createResult.Id}");
        var savedMedication = await HttpUtils.ParseJson<Medication>(getResponse);
        savedMedication.Code.Should().BeEquivalentTo(medication.Code);
        savedMedication.DoseForm.Should().BeEquivalentTo(medication.DoseForm);
        savedMedication.Ingredient.Should().BeEquivalentTo(medication.Ingredient);
    }

    private Medication GetTestMedication() => new()
    {
        Code = new CodeableConcept
        {
            Coding = new List<Coding>
            {
                new()
                {
                    Code = "400621001",
                    System = "http://snomed.info/sct",
                    Display = "Lorazepam 2mg/ml injection solution 1ml vial (product)"
                }
            }
        },
        DoseForm = new CodeableConcept
        {
            Coding = new List<Coding>
            {
                new()
                {
                    Code = "385219001",
                    System = "http://snomed.info/sct",
                    Display = "Injection solution (qualifier value)"
                }
            }
        },
        Ingredient = new List<Medication.IngredientComponent>
        {
            new()
            {
                Item = new CodeableReference
                {
                    Concept = new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new()
                            {
                                Code = "387106007",
                                System = "http://snomed.info/sct",
                                Display = "Lorazepam (substance)"
                            }
                        }
                    }
                },
                Strength = new Ratio
                {
                    Numerator = new Quantity
                    {
                        Code = "mg",
                        Value = 2,
                        System = "http://unitsofmeasure.org"
                    },
                    Denominator = new Quantity
                    {
                        Code = "mL",
                        Value = 1,
                        System = "http://unitsofmeasure.org"
                    }
                }
            }
        }
    };
}
