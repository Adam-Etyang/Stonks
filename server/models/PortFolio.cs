//holds a collection of stocks

namespace Stonks.Models;

public class Portfolio{
     public List<Stock> Stocks {get; set;} = new List<Stock>();
     public decimal TotalValue => Stocks.Sum(s => s.CurrentPrice);
     public int StockCount => Stocks.Count;

 }
