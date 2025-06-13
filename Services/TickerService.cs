using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

public class TickerService
{
    private readonly HttpClient _http;

    public TickerService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<string>> GetTeamEventsAsync()
    {
        var teams = new List<TeamSource>
        {
            new("New York Yankees", "baseball", "mlb", "10"),
            new("St. Louis Cardinals", "baseball", "mlb", "24"),
            new("Indiana Pacers", "basketball", "nba", "11")//,
            // new("IU Baseball", "baseball", "college-baseball", "294"),
            // new("IU Football", "football", "college-football", "84"),
            // new("IU Basketball", "basketball", "mens-college-basketball", "84"),
            // new("Indianapolis Colts", "football", "nfl", "11")
        };

        var results = new List<string>();

        foreach (var team in teams)
        {
            try
            {
                var teamData = await _http.GetFromJsonAsync<JsonElement>(team.TeamUrl);
                if (!teamData.TryGetProperty("team", out var teamNode))
                {
                    results.Add($"{team.DisplayName}: No team data found.");
                    continue;
                }
                if (!teamNode.TryGetProperty("nextEvent", out var eventArray) || eventArray.GetArrayLength() == 0)
                {
                    results.Add($"{team.DisplayName}: No upcoming events found.");
                    continue;
                }
                var eventObj = eventArray[0];
                var gameId = eventObj.GetProperty("id").GetString() ?? "unknown";
                var competitions = eventObj.GetProperty("competitions");
                var comp = competitions[0];
                var competitors = comp.GetProperty("competitors");
                // Determine which team is HOME and which is AWAY
                var competitor1 = competitors[0];
                var competitor2 = competitors[1];
                var homeTeam = competitor1.GetProperty("homeAway").GetString() == "home" ? competitor1 : competitor2;
                var awayTeam = competitor1.GetProperty("homeAway").GetString() == "away" ? competitor1 : competitor2;
                // Update variables to reflect HOME and AWAY
                var teamAway = awayTeam;
                var teamHome = homeTeam;

                if (competitors.GetArrayLength() < 2)
                {
                    results.Add($"{team.DisplayName}: Not enough competitors.");
                    continue;
                }
                // string headline = eventObj.GetProperty("competitions")[0].GetProperty("notes")[0].GetProperty("headline").GetString() ?? "No headline available";
                //the line below is causing MLB games to faile bc it is missing headline node
                string nameAway = teamAway.GetProperty("team").GetProperty("displayName").GetString() ?? "Away Team";
                string nameHome = teamHome.GetProperty("team").GetProperty("displayName").GetString() ?? "Home Team";
                string abbrevAway = teamAway.GetProperty("team").GetProperty("shortDisplayName").GetString() ?? "shortDisplayNameAway";
                string abbrevHome = teamHome.GetProperty("team").GetProperty("shortDisplayName").GetString() ?? "shortDisplayNameHome";
                string logoAway = teamAway.GetProperty("team").GetProperty("logos")[0].GetProperty("href").GetString() ?? "logoAway";
                string logoHome = teamHome.GetProperty("team").GetProperty("logos")[0].GetProperty("href").GetString() ?? "logoHome";
                string scoreAway = teamAway.TryGetProperty("score", out var sAway) ? sAway.GetProperty("displayValue").GetString() ?? "?" : "?";
                string scoreHome = teamHome.TryGetProperty("score", out var sHome) ? sHome.GetProperty("displayValue").GetString() ?? "?" : "?";
                //string recordAway = teamAway.GetProperty("record").GetProperty("items")[0].GetProperty("summary").GetString() ?? "??-??";
                //string recordHome = teamHome.GetProperty("record").GetProperty("items")[1].GetProperty("summary").GetString() ?? "??-??";
                
                string date = eventObj.GetProperty("date").GetString() ?? "";
                string dateStr = "";
                if (DateTime.TryParse(date, out var gameTime))
                {
                    dateStr = gameTime.ToLocalTime().ToString("h:mm tt");
                }

                string eventType = "";
                if (team.TeamUrl.Contains("mlb")) {
                    //results.Add($@"THIS IS A TEST");
                        eventType = "MLB";
                } else if (team.TeamUrl.Contains("nfl")) {
                    //results.Add($@"THIS IS A TEST NFL");
                    eventType = "NFL";
                } else if (team.TeamUrl.Contains("nba")) {
                    // results.Add($@"THIS IS A TEST NBA");
                    eventType = "NBA";
                } else if (team.TeamUrl.Contains("college-baseball")) {
                    //results.Add($@"THIS IS A TEST COLLEGE BASEBALL");
                    eventType = "College_Baseball";
                } else if (team.TeamUrl.Contains("college-football")) {
                    //results.Add($@"THIS IS A TEST COLLEGE FOOTBALL");
                    eventType = "College_Football";
                } else if (team.TeamUrl.Contains("mens-college-basketball")) {
                    //results.Add($@"THIS IS A TEST COLLEGE BASKETBALL");
                    eventType = "College_Basketball";
                } else {
                    //results.Add($@"{team.DisplayName}: Unsupported sport or league.");
                    eventType = "Unknown";
                    continue;
                }
                //Get the status of the game
                string statusState = comp.GetProperty("status").GetProperty("type").GetProperty("state").GetString() ?? "";
                // Branching logic based on status.state
                if (statusState == "pre")
                {
                    if (eventType == "MLB" || eventType == "College_Baseball")
                    {
                    // Check if probables exist for both teams
                    var probablesAwayExists = teamAway.TryGetProperty("probables", out var probablesAway) && probablesAway.GetArrayLength() > 0;
                    var probablesHomeExists = teamHome.TryGetProperty("probables", out var probablesHome) && probablesHome.GetArrayLength() > 0;
                    //string winsAway = teamAway.GetProperty("probables")[0].GetProperty("athlete").GetProperty("statistics")[0].GetProperty("stats")[25].GetProperty("displayValue").GetString() ?? "unknown";
                    // Get pre-game information for away team
                    string probableAway = probablesAwayExists ? GetSafeProperty(probablesAway[0].GetProperty("athlete"), "lastName", "unknown"): "unknown";
                    string winsAway = probablesAwayExists ? GetSafeProperty(probablesAway[0].GetProperty("statistics")[0].GetProperty("stats")[25], "displayValue", "unknown"): "unknown";
                    string lossesAway = probablesAwayExists ? GetSafeProperty(probablesAway[0].GetProperty("statistics")[0].GetProperty("stats")[6], "displayValue", "unknown"): "unknown";
                    string eraAway = probablesAwayExists ? GetSafeProperty(probablesAway[0].GetProperty("statistics")[0].GetProperty("stats")[53], "displayValue", "unknown"): "unknown";

                    // Get pre-game information for home team
                    string probableHome = probablesHomeExists ? GetSafeProperty(probablesHome[0].GetProperty("athlete"), "lastName", "unknown"): "unknown";
                    string winsHome = probablesHomeExists ? GetSafeProperty(probablesHome[0].GetProperty("statistics")[0].GetProperty("stats")[25], "displayValue", "unknown"): "unknown";
                    string lossesHome = probablesHomeExists ? GetSafeProperty(probablesHome[0].GetProperty("statistics")[0].GetProperty("stats")[6], "displayValue", "unknown"): "unknown";
                    string eraHome = probablesHomeExists ? GetSafeProperty(probablesHome[0].GetProperty("statistics")[0].GetProperty("stats")[53], "displayValue", "unknown"): "unknown";

                    // Show pre-game information
                    results.Add($@"
                    <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                        <!-- Row 1 -->
                        <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevHome} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <span style='opacity:0.6;'> {dateStr} </span>
                        <!-- Row 2 -->
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> {probableAway} ({winsAway} - {lossesAway}  {eraAway} ERA) </span>
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> {probableHome} ({winsHome} - {lossesHome}  {eraHome} ERA) </span>
                    </div>
                    ");
                    } else if (eventType == "NBA" || eventType == "College_Basketball")
                    {
                    // Get pre-game information for basketball
                    string headline = eventObj.GetProperty("competitions")[0].GetProperty("notes")[0].GetProperty("headline").GetString() ?? "No headline available";
                    // Fetch series summary from the scoreboard API
                    string seriesSummary = await GetScoreboardPropertyAsync("basketball", "nba", "series.summary", gameId);
                    string oddsDetails = await GetScoreboardPropertyAsync("basketball", "nba", "competitions.odds", gameId);

                    // Show pre-game information for basketball
                    results.Add($@"
                    <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                        <!-- Row 1 -->
                        <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevHome} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <span style='opacity:0.6;'> {dateStr} </span>
                        <!-- Row 2 -->
                        <span style='grid-column: span 3; text-align: left; font-size: 14px; opacity:0.8;'> {headline} </span>
                        <span style='grid-column: span 3; text-align: left; font-size: 14px; opacity:0.8;'> {seriesSummary} </span>
                        <span style='grid-column: span 3; text-align: left; font-size: 14px; opacity:0.8;'> {oddsDetails} </span>
                    </div>
                    ");
                    } else if (eventType == "NFL" || eventType == "College_Football")
                    {
                        // Show pre-game information for football
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} </span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevHome} </span>
                            <span style='opacity:0.6;'> {dateStr} </span>
                        </div>
                        ");
                    }
                }
                else if (statusState == "in")
                {
                    // Show live score for in-progress games
                    if (eventType == "MLB" || eventType == "College_Baseball")
                    {
                    // Show pre-game information
                    results.Add($@"
                    <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                        <!-- Row 1 -->
                        <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevHome} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <span style='opacity:0.6;'> INSERT STATUS INFO HERE (e.g. B4 2 out) </span>
                        <!-- Row 2 -->
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> INSERT DATA HERE1 </span>
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> INSERT DATA HERE2 </span>
                    </div>
                    ");
                    } else if (eventType == "NBA" || eventType == "College_Basketball")
                    {
                    // Get in-game information for basketball


                    // Show in-game information for basketball
                    results.Add($@"
                    <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                        <!-- Row 1 -->
                        <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold;'> {abbrevHome} </span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <span style='opacity:0.6;'>  INSERT STATUS INFO HERE (e.g. 4:40 3Q) </span>
                        <!-- Row 2 -->
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> INSERT DATA HERE3 </span>
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'>  INSERT HIGHLIHGHT INFO </span>
                    </div>
                    ");
                    } else if (eventType == "NFL" || eventType == "College_Football")
                    {
                        // Show in-game information for football
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} </span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevHome} </span>
                            <span style='opacity:0.6;'> INSERT STATUS INFO HERE (e.g. 4:40 3Q)</span>
                            <!-- Row 2 -->
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> INSERT DATA HERE4 </span>
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'>  INSERT DATA HERE5 </span>
                        </div>
                        ");
                    }
                }
                else if (statusState == "post")
                {

                // Determine the winner
                string winner = int.TryParse(scoreAway, out var awayScore) && int.TryParse(scoreHome, out var homeScore)
                    ? (awayScore > homeScore ? abbrevAway : abbrevHome)
                    : "unknown";

                    if (eventType == "MLB" || eventType == "College_Baseball") {
                    // Capture the winning, losing, and saving pitcher's last names using GetSafeProperty
                    string winningPitcher = GetSafeProperty(comp.GetProperty("status").GetProperty("featuredAthletes")[0].GetProperty("athlete"), "lastName", "unknown");
                    string losingPitcher = GetSafeProperty(comp.GetProperty("status").GetProperty("featuredAthletes")[1].GetProperty("athlete"), "lastName", "unknown");
                    string savingPitcher = GetSafeProperty(comp.GetProperty("status").GetProperty("featuredAthletes")[2].GetProperty("athlete"), "lastName", "");
                    // Conditionally include "SV:" only if savingPitcher is not empty
                    string savingPitcherDisplay = string.IsNullOrEmpty(savingPitcher) ? "" : $" SV: {savingPitcher}";
                    // Show final score for completed games
                    results.Add($@"
                    <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                        <!-- Row 1 -->
                        <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{abbrevAway}</span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{scoreAway}</span>
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{abbrevHome}</span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{scoreHome}</span>
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div><span style='opacity:0.6;'> FINAL </span>
                        <!-- Row 2 -->
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> W: {winningPitcher} | {savingPitcherDisplay}</span>
                        <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> | L: {losingPitcher} </span>
                    </div>
                    ");
                    } else if (eventType == "NBA" || eventType == "College_Basketball") {
                    // Show final score for basketball
                    results.Add($@"
                    <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                        <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{abbrevAway}</span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{scoreAway}</span>
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                        <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                        <span style='font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{abbrevHome}</span>
                        <span style='width: 20px;'></span> <!-- Empty cell -->
                        <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{scoreHome}</span>
                        <div style='border-left: 1px solid #ccc; height: 40px;'></div><span style='opacity:0.6;'> FINAL </span>
                    </div>
                        ");
                    } else if (eventType == "NFL" || eventType == "College_Football")
                    {
                        // Show final score for football
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{abbrevAway}</span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{scoreAway}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{abbrevHome}</span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{scoreHome}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div><span style='opacity:0.6;'> FINAL </span>
                        </div>
                        ");
                    }
                }
                else
                {
                    // Default case for unknown status
                    results.Add($"{team.DisplayName}: Status unknown.");
                }
            // Format: {Logo} Team1 (Record) Score1 @ {Logo} Team2 (Record) Score2 STATUS
                // results.Add($"<img src='{logoAway}' alt='{nameHome}' height='20' style='vertical-align:middle;' />{abbrev1}  @ <img src='{logoHome}' alt='{name2}' height='20' style='vertical-align:middle;' />{abbrev2}   ({dateStr})");

            }
            catch (Exception ex)
            {
                results.Add($"{team.DisplayName}: Error - {ex.Message}");
            }
        }

        return results;
    }

    public async Task<List<string>> GetLocalEventsAsync()
    {
        return await Task.FromResult(new List<string>
        {
            "Carmel Farmers Market - Sat 8AM",
            "Movies at Midtown - Wed 7PM",
            "Jazz on the Monon - Fri 6:30PM"
        });
    }

    public async Task<List<string>> GetStockPricesAsync()
    {
        var symbols = new[] { "NVDA", "AAPL", "CAVA", "ORCL" };
        var apiKey = "d166ef9r01qvtdbgkbn0d166ef9r01qvtdbgkbng"; // Replace with your Finnhub API key
        var results = new List<string>();
        foreach (var symbol in symbols)
        {
            try
            {
                // Fetch stock price and percentage change
                var quoteUrl = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={apiKey}";
                var quoteJson = await _http.GetFromJsonAsync<JsonElement>(quoteUrl);
                var price = quoteJson.GetProperty("c").GetDouble(); // "c" is the current price
                var percentChange = quoteJson.GetProperty("dp").GetDouble(); // "dp" is the percentage change

                // Fetch company logo
                var profileUrl = $"https://finnhub.io/api/v1/stock/profile2?symbol={symbol}&token={apiKey}";
                var profileJson = await _http.GetFromJsonAsync<JsonElement>(profileUrl);
                var logo = profileJson.TryGetProperty("logo", out var logoProperty) ? logoProperty.GetString() ?? "No logo available" : "No logo available";

                // Add result to the list
                results.Add($"<img src='{logo}' alt='{symbol}' height='40' style='vertical-align:middle;' /> {symbol}: ${price} ({percentChange:+0.00;-0.00}%)");
            }
            catch (Exception ex)
            {
                results.Add($"Error fetching stock data for {symbol}: {ex.Message}");
            }
        }
        return results;
    }

    public async Task<string> GetScoreboardPropertyAsync(string sport, string league, string propertyPath, string gameId)
    {
        var url = $"https://site.api.espn.com/apis/site/v2/sports/{sport}/{league}/scoreboard/summary?event={gameId}";
        Console.WriteLine($"Fetching scoreboard data from: {url}");
        try
        {
            var scoreboardData = await _http.GetFromJsonAsync<JsonElement>(url);

        if (scoreboardData.TryGetProperty("competitions", out var competitionsSB) && competitionsSB.GetArrayLength() > 0)
                {
                    if (propertyPath == "series.summary")
                    {
                        var seriesSummary = competitionsSB[0].GetProperty("series").GetProperty("summary").GetString() ?? "No series summary available";
                        return seriesSummary;
                    }
                    else if (propertyPath == "competitions.odds")
                    {
                        var eventOdds = competitionsSB[0].GetProperty("odds")[0].GetProperty("details").GetString() ?? "No event odds available";
                        return eventOdds;
                    }
                }
            return $"No data available for {propertyPath}";
        }
        catch (Exception ex)
        {
            return $"Error fetching {propertyPath}: {ex.Message}";
        }
    }
    // Helper method to safely get a property value
    private static string GetSafeProperty(JsonElement element, string propertyName, string defaultValue = "")
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetString() ?? defaultValue : defaultValue;
    }
}

public class TeamSource
{
    public string DisplayName { get; set; }
    public string Sport { get; set; }
    public string League { get; set; }
    public string TeamId { get; set; }

    public TeamSource(string name, string sport, string league, string teamId)
    {
        DisplayName = name;
        Sport = sport;
        League = league;
        TeamId = teamId;
    }
    public string TeamUrl => $"https://site.api.espn.com/apis/site/v2/sports/{Sport}/{League}/teams/{TeamId}";

}

public class YahooFinanceResponse
{
    public QuoteResponse? QuoteResponse { get; set; }
}

public class QuoteResponse
{
    public List<QuoteResult>? Result { get; set; }
}

public class QuoteResult
{
    public string? Symbol { get; set; }
    public double? RegularMarketPrice { get; set; }
}