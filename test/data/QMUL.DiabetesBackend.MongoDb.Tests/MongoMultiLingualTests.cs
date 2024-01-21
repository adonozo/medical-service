namespace QMUL.DiabetesBackend.MongoDb.Tests;

using System.Globalization;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

public class MongoMultiLingualTests
{
    [Theory]
    [InlineData("es", "-es")]
    [InlineData("en", "-en")]
    [InlineData("es-MX", "-es")]
    [InlineData("en-GB", "-en")]
    public void GetLocalizedCollection_WhenCultureIsSet_ReturnsCollectionName(string culture,
        string expectedSuffix)
    {
        // Arrange
        var database = Substitute.For<IMongoDatabase>();
        var collection = Substitute.For<IMongoCollection<BsonDocument>>();
        string builtCollectionName = null;
        database.GetCollection<BsonDocument>(Arg.Do<string>(name => builtCollectionName = name))
            .Returns(collection);

        CultureInfo.CurrentCulture = new CultureInfo(culture);
        var multiLingualCollection = new MultiLingualStub(database);

        // Act
        multiLingualCollection.CallLocalizedCollection();

        // Assert
        builtCollectionName.Should().NotBeNull().And.EndWith(expectedSuffix);
    }

    private class MultiLingualStub : MongoMultiLingualBase
    {
        private const string CollectionName = "stub";

        public void CallLocalizedCollection() => this.GetLocalizedCollection(CollectionName);

        public MultiLingualStub(IMongoDatabase database) : base(database)
        {
        }
    }
}