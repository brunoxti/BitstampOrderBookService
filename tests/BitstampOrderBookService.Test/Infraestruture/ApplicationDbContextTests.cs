using BitstampOrderBookService.Domain.Entities;
using BitstampOrderBookService.Infrastructure.Data;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace BitstampOrderBookService.Tests.UnitTests
{
    public class ApplicationDbContextTests
    {
        [Fact]
        public void ApplicationDbContext_Should_Set_And_Get_OrderBooks()
        {
            // Arrange
            var mockDatabase = new Mock<IMongoDatabase>();
            var mockOrderBookCollection = new Mock<IMongoCollection<OrderBook>>();
            mockDatabase.Setup(db => db.GetCollection<OrderBook>("orderbooks", null)).Returns(mockOrderBookCollection.Object);

            var context = new ApplicationDbContext(mockDatabase.Object);

            // Act
            var orderBooksCollection = context.OrderBooks;

            // Assert
            Assert.NotNull(orderBooksCollection);
            Assert.Equal(mockOrderBookCollection.Object, orderBooksCollection);
        }
    }
}
