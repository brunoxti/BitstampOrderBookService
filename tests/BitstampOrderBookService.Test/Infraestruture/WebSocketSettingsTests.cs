using BitstampOrderBookService.Infrastructure.Data;
using Xunit;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class WebSocketSettingsTests
    {
        [Fact]
        public void WebSocketSettings_Should_Have_Default_Constructor()
        {
            // Act
            var settings = new WebSocketSettings();

            // Assert
            Assert.NotNull(settings);
        }

        [Fact]
        public void WebSocketSettings_Should_Set_And_Get_Uri()
        {
            // Arrange
            var settings = new WebSocketSettings();
            var uri = "wss://test.com";

            // Act
            settings.Uri = uri;

            // Assert
            Assert.Equal(uri, settings.Uri);
        }
    }
}
