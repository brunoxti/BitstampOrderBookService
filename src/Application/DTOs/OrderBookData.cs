using System.Text.Json.Serialization;

namespace BitstampOrderBookService.src.Application.DTOs
{
    public class OrderBookData
    {
        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("microtimestamp")]
        public string MicroTimestamp { get; set; }

        [JsonPropertyName("bids")]
        public List<string[]> Bids { get; set; }

        [JsonPropertyName("asks")]
        public List<string[]> Asks { get; set; }
    }
}
