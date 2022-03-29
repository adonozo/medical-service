namespace QMUL.DiabetesBackend.ServiceImpl.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Hl7.Fhir.Model;
    using Model.Constants;
    using Model.Extensions;
    using Xunit;

    public class ExtensionsTest
    {
        [Fact]
        public void HasInsulinFlag_WhenExtensionContainsInsulin_ReturnsTrue()
        {
            // Arrange
            var medicationRequest = new MedicationRequest
            {
                Extension = new List<Extension>
                {
                    new() {Url = Extensions.InsulinFlag, Value = new FhirBoolean(true)}
                }
            };

            // Act
            var isInsulin = medicationRequest.HasInsulinFlag();

            // Assert
            isInsulin.Should().Be(true);
        }

        [Fact]
        public void HasInsulinFlag_WhenDoesNotHaveExtension_ReturnsFalse()
        {
            // Arrange
            var medicationRequest = new MedicationRequest
            {
                Id = Guid.NewGuid().ToString()
            };

            // Act
            var isInsulin = medicationRequest.HasInsulinFlag();

            // Assert
            isInsulin.Should().Be(false);
        }
        
        [Fact]
        public void SetInsulinFlag_WhenSuccessful_SetsExtension()
        {
            // Arrange
            var medicationRequest = new MedicationRequest
            {
                Id = Guid.NewGuid().ToString()
            };

            // Act
            medicationRequest.SetInsulinFlag();

            // Assert
            medicationRequest.Extension.Should().HaveCount(1);
            medicationRequest.Extension[0].Url.Should().Be(Extensions.InsulinFlag);
            medicationRequest.Extension[0].Value.Should().BeEquivalentTo(new FhirBoolean(true));
        }
    }
}