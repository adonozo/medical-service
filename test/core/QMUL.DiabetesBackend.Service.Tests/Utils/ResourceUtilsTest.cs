namespace QMUL.DiabetesBackend.Service.Tests.Utils;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Hl7.Fhir.Model;
using Service.Utils;
using Xunit;

public class ResourceUtilsTest
{
    [Fact]
    public void GenerateSearchBundle_ReturnsABundleInstance()
    {
        // Arrange
        var entries = new List<Resource> { TestUtils.GetStubPatient() };

        // Arrange and Act
        var bundle = ResourceUtils.GenerateSearchBundle(entries);

        // Assert
        bundle.Should().BeOfType<Bundle>();
    }

    [Fact]
    public void GenerateSearchBundle_ReturnsBundleTypeAndCurrentDate()
    {
        // Arrange
        var currentDate = DateTime.UtcNow.ToString("d");

        // Act
        var bundle = ResourceUtils.GenerateSearchBundle(new List<Resource>());

        // Assert
        bundle.Type.Should().Be(Bundle.BundleType.Searchset);
        var bundleTimestamp = bundle.Timestamp.Should().NotBeNull().And.Subject;
        bundleTimestamp!.Value.Date.Date.ToString("d").Should().Be(currentDate);
    }
}