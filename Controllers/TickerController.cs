using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TickerController : ControllerBase
{
    private readonly TickerService _tickerService;

    public TickerController(TickerService tickerService)
    {
        _tickerService = tickerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTickerData()
    {
        var sportsTicker = await _tickerService.GetTeamEventsAsync();
        var stockTicker = await _tickerService.GetStockPricesAsync();
        var localEvents = await _tickerService.GetLocalEventsAsync();

        var combinedTickerData = new
        {
            Sports = sportsTicker,
            Stocks = stockTicker,
            LocalEvents = localEvents
        };

        return Ok(combinedTickerData);
    }
}