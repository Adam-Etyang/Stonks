//model for stock

namespace Stonks.Models;

public class Stock{
    public string Ticker{get; set;}
    public string CompanyName{get; set;}
    public decimal CurrentPrice{get; set;}
    public decimal OpenPrice{get; set;}
    public decimal HighPrice{get;set;}
    public decimal LowPrice{get;set;}
    public long Volume{get; set;}
    public DateTime LastUpdated{get; set;}
}
