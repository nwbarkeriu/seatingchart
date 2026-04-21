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
.ticker-wrap{overflow:hidden;width:100%;height:100vh;position:relative;}
.ticker{position:absolute;display:flex;flex-direction:row;left:0;top:0;height:100%;will-change:transform;}
.ticker-item{height:100%;min-width:420px;max-width:500px;padding:0 16px;display:flex;align-items:center;border-right:1px solid #333;flex-shrink:0;}
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
  var halfCount=Math.floor(items.length/2);
  var totalW=0;
  for(var i=0;i<halfCount;i++){totalW+=items[i].offsetWidth;}
  var speed=60;
  var offset=0;
  var last=performance.now();
  function anim(now){
    var dt=(now-last)/1000;
    last=now;
    offset+=speed*dt;
    if(offset>=totalW){offset-=totalW;}
    ticker.style.transform='translateX(-'+offset+'px)';
    requestAnimationFrame(anim);
  }
  ticker.style.transition='none';
  requestAnimationFrame(anim);
})();
</script>
</body>
</html>");

        return Content(sb.ToString(), "text/html");
    }

    /// <summary>
    /// Flat array of all ticker items (doubled for seamless CSS-only horizontal loop).
    /// Point DakBoard Fetch block to: https://thebarkers.info/api/ticker/flat
    /// </summary>
    [HttpGet("flat")]
    public async Task<IActionResult> GetFlatTickerData()
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

        // Duplicate for seamless CSS animation loop
        var doubled = new List<string>(allItems);
        doubled.AddRange(allItems);

        return Ok(doubled);
    }
}