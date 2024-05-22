using System.Text.Json.Serialization;

namespace BitstampOrderBookService.src.Application.DTOs
{

    public class OrderBookUpdate
    {
        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonPropertyName("data")]
        public OrderBookData Data { get; set; }
    }
}
