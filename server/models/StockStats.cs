namespace Stonks.Models;

public class StockStats{

    public string Ticker{get; set;} = string.Empty;
    public decimal HighestClose{get; set;}
    public decimal LowestClose{get; set;}
    public decimal AverageClose{get; set;}
    public decimal PriceChange{get; set;}
    public decimal PriceChangepct{get; set;}

    public PricePoint ? BestDay {get; set;}
    public PricePoint ? WorstDay  {get; set;}


}
