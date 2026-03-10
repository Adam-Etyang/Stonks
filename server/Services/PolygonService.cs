using Stonks.Models;
using System.Text.Json;

namespace Stonks.Services;


public class PolygonService{

    public readonly HttpClient _httpClient;
    public readonly string _apiKey;
    
    public PolygonService(HttpClient httpClient, IConfiguration configuration){
        _httpClient = httpClient;
        _apiKey = configuration["PolygonApiKey"] ?? throw new Exception("Api key not found");

    }
    private async Task<string> GetCompanynameAsync(string ticker){
        try{
            string url = $"https://api.polygon.io/v3/reference/tickers/{ticker}?apiKey={_apiKey}";
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("results")
            .GetProperty("name")
            .GetString() ?? ticker; 
            
        }
        catch{
            return ticker;
        }
    }
    
    //creates a stock ticker symbol and returns the stock data on that stock
    public async Task<Stock?> GetStockAsync(string ticker){
        try{

            string url = $"https://api.polygon.io/v2/aggs/ticker/{ticker}/prev?apiKey={_apiKey}";
            Console.WriteLine($"Fetching: {url}");
            HttpResponseMessage response  = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(json);//convert string to JSON

            JsonElement result = doc.RootElement.GetProperty("results")[0];

            return new Stock{
            Ticker = ticker,
            CompanyName = await GetCompanynameAsync(ticker),
            OpenPrice = result.GetProperty("o").GetDecimal(),
            CurrentPrice = result.GetProperty("c").GetDecimal(),
            HighPrice = result.GetProperty("h").GetDecimal(),
            LowPrice = result.GetProperty("l").GetDecimal(),
            Volume = (long)result.GetProperty("v").GetDouble(),
            LastUpdated = DateTime.UtcNow
            };
        }
        catch(HttpRequestException ex)
        {
            Console.WriteLine($"Http request error: {ex.Message}");
            return null;
        }
        catch(Exception ex){
            Console.WriteLine($"Error fetching {ticker}: {ex.Message}");
            return null;
        }

    }

    //get stock statistics
    public async Task<StockStats?> GetStockStatsAsync(string ticker){
        
        try{
            List<PricePoint> history = await GetPriceHistoryAsync(ticker);
            if(!history.Any()){
                return null;
            }

            List<PricePoint> sorted = history.OrderBy(p=>p.Date).ToList();
            decimal firstClose = sorted.First().Close;
            decimal lastClose = sorted.Last().Close;
            decimal pricechange = lastClose-firstClose;
            return new StockStats{
                Ticker = ticker,
                HighestClose = sorted.Max(p=> p.Close),
                LowestClose = sorted.Min(p=>p.Close),
                AverageClose = sorted.Average(p=>p.Close),
                PriceChange = pricechange,
                PriceChangepct = Math.Round((pricechange/firstClose)*100,2),
                BestDay = sorted.MaxBy(p=>p.Close - p.Open),
                WorstDay = sorted.MinBy(p=>p.Close - p.Open)
            };
        }catch{
            Console.WriteLine("oops sth happed in the polygon service");
            return null;
        }
    }
    public async Task<List<PricePoint>> GetFilteredHistoryAsync(
    string ticker,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    string? orderBy = "date")
{
    List<PricePoint> history = await GetPriceHistoryAsync(ticker);

    // Filter by min price if provided
    IEnumerable<PricePoint> filtered = history;

    if (minPrice.HasValue)
        filtered = filtered.Where(p => p.Close >= minPrice.Value);

    if (maxPrice.HasValue)
        filtered = filtered.Where(p => p.Close <= maxPrice.Value);

    // Sort based on orderBy parameter
    filtered = orderBy switch
    {
        "price"     => filtered.OrderByDescending(p => p.Close),
        "volume"    => filtered.OrderByDescending(p => p.Volume),
        "gain"      => filtered.OrderByDescending(p => p.Close - p.Open),
        _           => filtered.OrderBy(p => p.Date) // default — sort by date
    };

    return filtered.ToList();
}

    //returns a list of daily pricepoints 
    public async Task<List<PricePoint>> GetPriceHistoryAsync(string ticker){

        try
        {
            string from = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
            string to = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string url = $"https://api.polygon.io/v2/aggs/ticker/{ticker}/prev?apiKey={_apiKey}";



            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            JsonDocument doc = JsonDocument.Parse(json);

            var results = doc.RootElement.GetProperty("results");
            var pricePoints = new List<PricePoint>();

            foreach(JsonElement item in results.EnumerateArray()){
                pricePoints.Add(new PricePoint
                    {
                        Date = DateTimeOffset.FromUnixTimeMilliseconds(item.GetProperty("t").GetInt64()).DateTime,
                        Open = item.GetProperty("o").GetDecimal(),
                        Close = item.GetProperty("c").GetDecimal(),
                        High = item.GetProperty("h").GetDecimal(),
                        Low = item.GetProperty("l").GetDecimal(),
                        Volume = (long)item.GetProperty("v").GetDouble()
                    });
            }
            return pricePoints;
        }
        catch(Exception ex){
            //TODO:log into a debug file
            Console.WriteLine($"Error fetching history for {ticker}: {ex.Message}");
            return new List<PricePoint>();
        }
    }

    
}



