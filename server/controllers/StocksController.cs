using Stonks.Models;
using Stonks.Services;
using Microsoft.AspNetCore.Mvc;

namespace Stonks.Controller;


[ApiController]
[Route("api/[Controller]")]
public class StocksController:ControllerBase{
    private readonly PolygonService _polygonService;

    public StocksController(PolygonService polygonService){
         _polygonService = polygonService;
    }
 
    // GET api/stocks/AAPL
     [HttpGet("{ticker}")]
     public async Task<ActionResult> GetStock(string ticker){
         if(string.IsNullOrWhiteSpace(ticker)){
             return BadRequest("Ticker symbol is required");
         }
         Stock? stock = await _polygonService.GetStockAsync(ticker.ToUpper());
         if(stock == null){
             return NotFound($"could not find the data for {ticker}");
         }
         return Ok(stock);
     }

    //Get history
    [HttpGet("{ticker}/history")]
    public async Task<ActionResult> GetStockHistory(string ticker){
        if(string.IsNullOrWhiteSpace(ticker)){
            return BadRequest("Ticker symbol is required");
        }

        List<PricePoint> history = await _polygonService.GetPriceHistoryAsync(ticker.ToUpper());

        if(!history.Any())
        {
            return NotFound($"No history found for {ticker}");
        }
        return Ok(history);

    }

    //Get portfolio
    [HttpGet("portfolio")]
    public async Task<ActionResult> GetPortfolio([FromQuery] string tickers){
        if(string.IsNullOrWhiteSpace(tickers)){
            return BadRequest("Ticker symbol is required");
        }
        List<string> tickerList = tickers.Split(',').Select(t=>t.Trim().ToUpper()).ToList();
        var tasks = tickerList.Select(t=>_polygonService.GetStockAsync(t));
        Stock?[] results = await Task.WhenAll(tasks);

        var portfolio = new Portfolio{
            Stocks = results.Where(s=> s!= null).Cast<Stock>().ToList()
        };
        return Ok(portfolio);
    }

    // GET api/stocks/AAPL/stats
    [HttpGet("{ticker}/stats")]
    public async Task<IActionResult> GetStockStats(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            return BadRequest("Ticker symbol is required");

        StockStats? stats = await _polygonService.GetStockStatsAsync(ticker.ToUpper());

        if (stats == null)
            return NotFound($"No stats found for ticker: {ticker}");

        return Ok(stats);
    }

[HttpGet("{ticker}/filter")]
public async Task<IActionResult> GetFilteredHistory(
    string ticker,
    [FromQuery] decimal? minPrice = null,
    [FromQuery] decimal? maxPrice = null,
    [FromQuery] string orderBy = "date")
{
    if (string.IsNullOrWhiteSpace(ticker))
        return BadRequest("Ticker symbol is required");

    List<PricePoint> history = await _polygonService.GetFilteredHistoryAsync(
        ticker.ToUpper(), minPrice, maxPrice, orderBy);

    if (!history.Any())
        return NotFound($"No data found for ticker: {ticker} with given filters");

    return Ok(history);
}

}
