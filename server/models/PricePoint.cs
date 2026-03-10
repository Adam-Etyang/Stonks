//holds data for a stoc at a particular time
namespace Stonks.Models;

public class PricePoint{
    public DateTime Date{get; set;}
    public decimal Open{get; set;}
    public decimal Close{get; set;}
    public decimal High{get; set;}
    public decimal Low{get; set;}
    public long Volume{get; set;}
}
