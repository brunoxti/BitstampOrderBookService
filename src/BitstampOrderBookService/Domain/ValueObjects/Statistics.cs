namespace BitstampOrderBookService.Domain.ValueObjects
{
    public sealed class Statistics
    {
        public decimal? Highest { get; set; }
        public decimal? Lowest { get; set; }
        public decimal? Average { get; set; }
        public decimal? AccumulatedAverage { get; set; }
        public decimal? AccumulatedQuantityAverage { get; set; }
    }
}
