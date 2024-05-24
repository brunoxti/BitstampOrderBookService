using BitstampOrderBookService.Infrastructure.Data;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class MongoDbSettingsTests
    {
        [Fact]
        public void MongoDbSettings_Should_Have_Default_Constructor()
        {
            // Act
            var settings = new MongoDbSettings();

            // Assert
            Assert.NotNull(settings);
        }

        [Fact]
        public void MongoDbSettings_Should_Set_And_Get_ConnectionString()
        {
            // Arrange
            var settings = new MongoDbSettings();
            var connectionString = "mongodb://localhost:27017";

            // Act
            settings.ConnectionString = connectionString;

            // Assert
            Assert.Equal(connectionString, settings.ConnectionString);
        }

        [Fact]
        public void MongoDbSettings_Should_Set_And_Get_DatabaseName()
        {
            // Arrange
            var settings = new MongoDbSettings();
            var databaseName = "bitstamp";

            // Act
            settings.DatabaseName = databaseName;

            // Assert
            Assert.Equal(databaseName, settings.DatabaseName);
        }
    }
}
