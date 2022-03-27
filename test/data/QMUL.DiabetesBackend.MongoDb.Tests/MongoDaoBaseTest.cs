namespace QMUL.DiabetesBackend.MongoDb.Tests
{
    using System;
    using System.Threading.Tasks;
    using DataInterfaces.Exceptions;
    using FluentAssertions;
    using MongoDB.Driver;
    using NSubstitute;
    using Xunit;

    public class MongoDaoBaseTest
    {
        [Fact]
        public async Task GetSingleOrThrow_WhenResultExists_ReturnsObject()
        {
            // Arrange
            var database = Substitute.For<IMongoDatabase>();
            var mongoDao = new MongoTestDao(database);
            const string expectedResult = "success";

            var cursorMock = Substitute.For<IAsyncCursor<string>>();
            cursorMock.MoveNextAsync().Returns(Task.FromResult(true));
            cursorMock.Current.Returns(new[] {expectedResult});

            var find = Substitute.For<IFindFluent<string, string>>();
            find.ToCursorAsync().Returns(Task.FromResult(cursorMock));
            find.Limit(1).Returns(find);

            // Act
            var result = await mongoDao.GetSingleOrThrowWrapper(find, new CreateException(string.Empty), () => { });

            // Assert
            result.Should().Be(expectedResult);
        }

        [Fact]
        public async Task GetSingleOrThrow_WhenResultIsNull_Throws()
        {
            // Arrange
            var database = Substitute.For<IMongoDatabase>();
            var mongoDao = new MongoTestDao(database);
            var expectedException = new NotFoundException(string.Empty);
            var fallback = Substitute.For<Action>();

            var cursorMock = Substitute.For<IAsyncCursor<string>>();
            cursorMock.MoveNextAsync().Returns(Task.FromResult(false));
            cursorMock.Current.Returns(Array.Empty<string>());

            var find = Substitute.For<IFindFluent<string, string>>();
            find.ToCursorAsync().Returns(Task.FromResult(cursorMock));
            find.Limit(1).Returns(find);

            // Act
            var action =
                new Func<Task<string>>(() => mongoDao.GetSingleOrThrowWrapper(find, expectedException, fallback));

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
            fallback.Received().Invoke();
        }

        [Fact]
        public void CheckAcknowledgedOrThrow_WhenTrue_Returns()
        {
            // Arrange
            var database = Substitute.For<IMongoDatabase>();
            var mongoDao = new MongoTestDao(database);

            // Act
            var action = new Action(() =>
                mongoDao.CheckAcknowledgedOrThrowWrapper(true, new CreateException(string.Empty), () => { }));

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void CheckAcknowledgedOrThrow_WhenFalse_Throws()
        {
            // Arrange
            var database = Substitute.For<IMongoDatabase>();
            var mongoDao = new MongoTestDao(database);
            var expectedException = new CreateException(string.Empty);
            var fallback = Substitute.For<Action>();

            // Act
            var action = new Action(() =>
                mongoDao.CheckAcknowledgedOrThrowWrapper(false, expectedException, fallback));

            // Assert
            action.Should().Throw<CreateException>();
            fallback.Received().Invoke();
        }

        private class MongoTestDao : MongoDaoBase
        {
            public MongoTestDao(IMongoDatabase database) : base(database)
            {
            }

            public async Task<TProjection> GetSingleOrThrowWrapper<TDocument, TProjection>(
                IFindFluent<TDocument, TProjection> find, DataExceptionBase exception, Action fallback)
            {
                return await this.GetSingleOrThrow(find, exception, fallback);
            }

            public void CheckAcknowledgedOrThrowWrapper(bool result, DataExceptionBase exception, Action fallback)
            {
                this.CheckAcknowledgedOrThrow(result, exception, fallback);
            }
        }
    }
}