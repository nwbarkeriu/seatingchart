using Microsoft.AspNetCore.Mvc;
using System.Text;

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
        var schoolEvents = await _tickerService.GetSchoolEventsAsync();

        var combinedTickerData = new
        {
            Sports = sportsTicker,
            Stocks = stockTicker,
            LocalEvents = localEvents,
            SchoolEvents = schoolEvents
        };

        return Ok(combinedTickerData);
    }

    [HttpGet("dakboard")]
    public async Task<ContentResult> GetDakBoardTicker()
    {
        var sports = await _tickerService.GetTeamEventsAsync();
        var stocks = await _tickerService.GetStockPricesAsync();
        var localEvents = await _tickerService.GetLocalEventsAsync();
        var schoolEvents = await _tickerService.GetSchoolEventsAsync();

        var allItems = new List<string>();
        allItems.AddRange(sports);
        allItems.AddRange(stocks);
        allItems.AddRange(localEvents);
        allItems.AddRange(schoolEvents);

        // Duplicate for seamless looping
        var doubled = new List<string>(allItems);
        doubled.AddRange(allItems);

        var sb = new StringBuilder();
        sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<meta name='viewport' content='width=device-width,initial-scale=1'/>
<meta http-equiv='refresh' content='300'/>
<style>
*{margin:0;padding:0;box-sizing:border-box;}
html,body{background:#000;color:#fff;font-family:'Segoe UI',Arial,sans-serif;overflow:hidden;height:100%;}
.ticker-wrap{overflow:hidden;height:100vh;position:relative;}
.ticker{position:absolute;width:100%;left:0;top:0;will-change:transform;}
.ticker-item{height:100vh;display:flex;align-items:center;justify-content:center;padding:10px;}
</style>
</head>
<body>
<div class='ticker-wrap'>
<div class='ticker'>
");

        foreach (var item in doubled)
        {
            sb.Append("<div class='ticker-item'>");
            sb.Append(item);
            sb.Append("</div>\n");
        }

        sb.Append(@"</div>
</div>
<script>
(function(){
  var ticker=document.querySelector('.ticker');
  var items=ticker.querySelectorAll('.ticker-item');
  if(!items.length)return;
  var itemH=window.innerHeight;
  var halfCount=Math.floor(items.length/2);
  var idx=0;
  ticker.style.transition='none';
  ticker.style.transform='translateY(0px)';
  setInterval(function(){
    idx++;
    ticker.style.transition='transform 600ms ease-in-out';
    ticker.style.transform='translateY(-'+(idx*itemH)+'px)';
    if(idx>=halfCount){
      setTimeout(function(){
        idx=0;
        ticker.style.transition='none';
        ticker.style.transform='translateY(0px)';
      },600);
    }
  },4600);
})();
</script>
</body>
</html>");

        return Content(sb.ToString(), "text/html");
    }
}