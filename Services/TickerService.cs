using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

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
            new("Indiana Pacers", "basketball", "nba", "11"),
            new("Chi Cubs", "baseball", "mlb", "16"),
            new("IU Baseball", "baseball", "college-baseball", "294"),
            new("IU Football", "football", "college-football", "84"),
            new("IU Basketball", "basketball", "mens-college-basketball", "84"),
            new("Indianapolis Colts", "football", "nfl", "11")
        };
        var results = new List<string>();
        foreach (var team in teams)
        {
        try
        {
                var teamData = await _http.GetFromJsonAsync<JsonElement>(team.TeamUrl);
            if (!teamData.TryGetProperty("team", out var teamNode))
            {
                //results.Add($"{team.DisplayName}: No team data found.");
                continue;
            }
            if (!teamNode.TryGetProperty("nextEvent", out var eventArray) || eventArray.GetArrayLength() == 0)
            {
                //results.Add($"{team.DisplayName}: No upcoming events found.");
                continue;
            }
                var eventObj = eventArray[0];
                var gameId = eventObj.GetProperty("id").GetString() ?? "unknown";
                var eventSummaryUrl = team.GetEventUrl(gameId);
                var eventSummary = await _http.GetFromJsonAsync<JsonElement>(eventSummaryUrl);

            if (!eventSummary.TryGetProperty("competitions", out var competitions) || competitions.GetArrayLength() == 0)
            {
                //results.Add($"{team.DisplayName}: No event data found.");
                continue;
            }
            var comp = competitions[0];
            var competitors = comp.GetProperty("competitors");

            if (competitors.GetArrayLength() < 2)
            {
                results.Add($"{team.DisplayName}: Not enough competitors.");
                continue;
            }
                // Determine which team is HOME and which is AWAY
                var competitor1 = competitors[0];
                var competitor2 = competitors[1];
                var homeTeam = competitor1.GetProperty("homeAway").GetString() == "home" ? competitor1 : competitor2;
                var homeTeamID = competitor1.GetProperty("homeAway").GetString() == "home" ? competitor1.GetProperty("team").GetProperty("id").GetString() : competitor2.GetProperty("team").GetProperty("id").GetString();
                var awayTeamID = competitor1.GetProperty("homeAway").GetString() == "away" ? competitor1.GetProperty("team").GetProperty("id").GetString() : competitor2.GetProperty("team").GetProperty("id").GetString();
                var awayTeam = competitor1.GetProperty("homeAway").GetString() == "away" ? competitor1 : competitor2;
                // Update variables to reflect HOME and AWAY
                var teamAway = awayTeam;
                var teamHome = homeTeam;
                string nameAway = awayTeam.GetProperty("team").GetProperty("displayName").GetString() ?? "Away Team";
                string nameHome = homeTeam.GetProperty("team").GetProperty("displayName").GetString() ?? "Home Team";
                string abbrevAway = awayTeam.GetProperty("team").GetProperty("shortDisplayName").GetString() ?? "shortDisplayNameAway";
                string abbrevHome = homeTeam.GetProperty("team").GetProperty("shortDisplayName").GetString() ?? "shortDisplayNameHome";
                string scoreAway = awayTeam.GetProperty("score").GetString() ?? "0";
                string scoreHome = homeTeam.GetProperty("score").GetString() ?? "0";
                string recordAway = awayTeam.GetProperty("records")[0].GetProperty("summary").GetString() ?? "0-0";
                string recordHome = homeTeam.GetProperty("records")[0].GetProperty("summary").GetString() ?? "0-0";
                Console.WriteLine($"Away Team: {nameAway}, Home Team: {nameHome}, Away Record: {recordAway}, Home Record: {recordHome}");
                // Defensive: handle both "logo" (string) and "logos" (array)
                string logoAway = "logoAway";
                var awayTeamObj = awayTeam.GetProperty("team");
            if (awayTeamObj.TryGetProperty("logos", out var logosAway) && logosAway.ValueKind == JsonValueKind.Array && logosAway.GetArrayLength() > 0)
                logoAway = logosAway[0].GetProperty("href").GetString() ?? "logoAway";
            else if (awayTeamObj.TryGetProperty("logo", out var logoPropAway) && logoPropAway.ValueKind == JsonValueKind.String)
                logoAway = logoPropAway.GetString() ?? "logoAway";
                string logoHome = "logoHome";
                var homeTeamObj = homeTeam.GetProperty("team");
            if (homeTeamObj.TryGetProperty("logos", out var logosHome) && logosHome.ValueKind == JsonValueKind.Array && logosHome.GetArrayLength() > 0)
                logoHome = logosHome[0].GetProperty("href").GetString() ?? "logoHome";
            else if (homeTeamObj.TryGetProperty("logo", out var logoPropHome) && logoPropHome.ValueKind == JsonValueKind.String)
                logoHome = logoPropHome.GetString() ?? "logoHome";
                string date = comp.GetProperty("date").GetString() ?? "";
                string dateStr = "";
            if (DateTime.TryParse(date, out var gameTime))
            {
                // Filter: Only include games within the last 5 days
                var now = DateTime.UtcNow;
                var fiveDaysAgo = now.AddDays(-2);
                if (gameTime.ToUniversalTime() < fiveDaysAgo)
                {
                    // Skip this event if it's older than 5 days
                    continue;
                }
                dateStr = gameTime.ToLocalTime().ToString("h:mm tt");
            }
                string eventType = "";
                var leagueCode = "";
            if (team.TeamUrl.Contains("mlb")) 
                {
                //results.Add($@"THIS IS A TEST");
                    eventType = "MLB";
                    //gameId = "401696070";
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
                string statusState = comp.GetProperty("status").GetProperty("type").GetProperty("state").GetString() ?? "";
                //string statusState = "post";
            if (statusState == "pre"){
                    if (eventType == "MLB" || eventType == "College_Baseball")
                    {
                        if (eventType == "MLB") {
                            leagueCode = "mlb";
                        } else {
                            leagueCode = "college-baseball";
                        }
                        string probableData = await GetScoreboardPropertyAsync("baseball", leagueCode, "competitions.probables", gameId);
                        string headlineData = await GetScoreboardPropertyAsync("baseball", leagueCode, "competitions.headlines", gameId);
                        
                        string GetTeamIdFromSpan(string span)
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(span, "data-teamid='(\\d+)'");
                            return match.Success ? match.Groups[1].Value : "";
                        }
                        string awayProbablePitcher = "", homeProbablePitcher = "";
                        string probablePitcherTeamId = GetTeamIdFromSpan(probableData);
                        var spans = probableData.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var span in spans) 
                        {
                            var teamId = GetTeamIdFromSpan(span);
                            if (teamId == awayTeamID) awayProbablePitcher = span;
                            else if (teamId == homeTeamID) homeProbablePitcher = span;
                        }
                            // Show pre-game information
                            results.Add($@"
                            <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                                <!-- Row 1 -->
                                <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                                <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span> <br /> {awayProbablePitcher}</span>
                                <span style='width: 20px;'></span> <!-- Empty cell -->
                                <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                                <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                                <span style='font-size: 24px; font-weight:bold;'> {abbrevHome} <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span>  <br /> {homeProbablePitcher}</span>
                                <span style='width: 20px;'></span> <!-- Empty cell -->
                                <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                                <span style='opacity:0.6;'> {dateStr} </span>
                                <!-- Row 2 -->
                                <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'>{headlineData}</span>
                                <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'></span>
                            </div>
                            ");
                    } else if (eventType == "NBA" || eventType == "College_Basketball")
                    {
                        if (eventType == "NBA") {
                            leagueCode = "nba";
                        } else {
                            leagueCode = "mens-college-basketball";
                        }
                        // Get pre-game information for basketball
                        string headline = eventObj.GetProperty("competitions")[0].GetProperty("notes")[0].GetProperty("headline").GetString() ?? "No headline available";
                        // Fetch series summary from the scoreboard API
                        string seriesSummary = await GetScoreboardPropertyAsync("basketball", leagueCode, "series.summary", gameId);
                        string oddsDetails = await GetScoreboardPropertyAsync("basketball", leagueCode, "competitions.odds", gameId);
                        // Show pre-game information for basketball
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <!-- Row 1 -->
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevAway}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevHome}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <span style='opacity:0.6;'> {dateStr} </span>
                            <!-- Row 2 -->
                            <span style='grid-column: span 3; text-align: left; font-size: 14px; opacity:0.8;'> {headline} </span>
                            <span style='grid-column: span 3; text-align: left; font-size: 14px; opacity:0.8;'> {seriesSummary} </span>
                            <span style='grid-column: span 3; text-align: center; font-size: 14px; opacity:0.8;'> {oddsDetails} </span>
                        </div>
                        ");
                    } else if (eventType == "NFL" || eventType == "College_Football")
                    {
                        if (eventType == "NFL") {
                            leagueCode = "nfl";
                        } else {
                            leagueCode = "college-football";
                        }
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
                    if (eventType == "MLB" || eventType == "College_Baseball")
                        {
                        if (eventType == "MLB") {
                            leagueCode = "mlb";
                        } else {
                            leagueCode = "college-baseball";
                        }
                        string inGameHomeScore = await GetScoreboardPropertyAsync("baseball", leagueCode,  "event.homeScore", gameId);
                        string inGameAwayScore = await GetScoreboardPropertyAsync("baseball", leagueCode,  "event.awayScore", gameId);
                        string inGameShortDetailStatus = await GetScoreboardPropertyAsync("baseball", leagueCode,  "event.ShortDetailstatus", gameId);
                        string inGameLastPlay = await GetScoreboardPropertyAsync("baseball", leagueCode,  "competitions.lastPlay", gameId);
                        string inGameCount = await GetScoreboardPropertyAsync("baseball", leagueCode,  "competitions.count", gameId);
                        string inGameBases = await GetScoreboardPropertyAsync("baseball", leagueCode,  "competitions.bases", gameId);
                        string inGameOuts = await GetScoreboardPropertyAsync("baseball", leagueCode,  "competitions.outs", gameId);
                        string inGamePitcher = await GetScoreboardPropertyAsync("baseball", leagueCode,  "competitions.pitcher", gameId);
                        string inGameBatter = await GetScoreboardPropertyAsync("baseball", leagueCode,  "competitions.batter", gameId);
                        string GetTeamIdFromSpan(string span)
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(span, "data-teamid='(\\d+)'");
                            return match.Success ? match.Groups[1].Value : "";
                        }
                        string pitcherTeamId = GetTeamIdFromSpan(inGamePitcher);
                        string batterTeamId = GetTeamIdFromSpan(inGameBatter);
                        string awayPitcher = "", homePitcher = "", awayBatter = "", homeBatter = "";
                            if (pitcherTeamId == awayTeamID) awayPitcher = inGamePitcher;
                            else if (pitcherTeamId == homeTeamID) homePitcher = inGamePitcher;
                            if (batterTeamId == awayTeamID) awayBatter = inGameBatter;
                            else if (batterTeamId == homeTeamID) homeBatter = inGameBatter;
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <!-- Row 1 -->
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevAway} <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span> <br /> {awayPitcher}{awayBatter} </span>
                            <span style='width: 20px;'> </span> <!-- Empty cell --> 
                            <span style='align: right; font-size: 24px; font-weight:bold;'> {inGameAwayScore} </span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'> </div> <!-- Empty border cell -->
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevHome}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span> <br /> {homePitcher}{homeBatter}</span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold;'>{inGameHomeScore}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <span style='opacity:0.6; display: flex; flex-direction: column; align-items: center; grid-column: span 2;'>
                                <span style='margin-bottom: 1px; font-size: 14px; opacity:0.8;'>{inGameShortDetailStatus}</span>
                                <span style='margin-bottom: 25px; height: 10px;'>{inGameBases}</span>
                                <span style='margin-bottom: 1px; height: 10px;'>{inGameOuts}</span>
                            </span>
                            <!-- Row 2 -->
                            <span style='grid-row: 2; grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> {inGameLastPlay} </span>
                    
                            </div>
                        ");

                    } else if (eventType == "NBA" || eventType == "College_Basketball")
                    {
                        if (eventType == "NBA") {
                            leagueCode = "nba";
                        } else {
                            leagueCode = "mens-college-basketball";
                        }
                        // Get in-game information for basketball
                        string inGameHomeScoreBB = await GetScoreboardPropertyAsync("basketball", leagueCode, "event.homeScore", gameId);
                        string inGameAwayScoreBB = await GetScoreboardPropertyAsync("basketball", leagueCode, "event.awayScore", gameId);
                        string inGameLastPlayBB = await GetScoreboardPropertyAsync("basketball", leagueCode, "competitions.lastPlay", gameId);
                        string inGameShortDetailStatusBB = await GetScoreboardPropertyAsync("basketball", leagueCode, "event.ShortDetailstatus", gameId);

                        // Show in-game information for basketball
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <!-- Row 1 -->
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevAway}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold;'> {inGameAwayScoreBB} </span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevHome}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold;'>{inGameHomeScoreBB}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <span style='opacity:0.6;'>  {inGameShortDetailStatusBB} </span>
                            <!-- Row 2 -->
                            <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> {inGameLastPlayBB} </span>
                            <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'>  IN-GAME NBA DATA 2 </span>
                        </div>
                        ");
                    } else if (eventType == "NFL" || eventType == "College_Football")
                    {
                        if (eventType == "NFL") {
                            leagueCode = "nfl";
                        } else {
                            leagueCode = "college-football";
                        }
                        string inGameHomeScoreNFL = await GetScoreboardPropertyAsync("football", leagueCode, "event.homeScore", gameId);
                        string inGameAwayScoreNFL = await GetScoreboardPropertyAsync("football", leagueCode, "event.awayScore", gameId);
                        string inGameLastPlayNFL = await GetScoreboardPropertyAsync("football", leagueCode, "competitions.lastPlay", gameId);
                        string inGameShortDetailStatusNFL = await GetScoreboardPropertyAsync("football", leagueCode, "event.ShortDetailstatus", gameId);

                        // Show in-game information for football
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <!-- Row 1 -->
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevAway}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span> </span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold;'> {inGameAwayScoreNFL} </span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold;'> {abbrevHome}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold;'>{inGameHomeScoreNFL}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <span style='opacity:0.6;'>  {inGameShortDetailStatusNFL} </span>
                            <!-- Row 2 -->
                            <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> {inGameLastPlayNFL} </span>
                            <span style='grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'>  IN-GAME NFL DATA 2 </span>
                        </div>
                        ");
                    }
                }
                else if (statusState == "post")
                {
                    // Determine the winner
                    string winner = int.TryParse(scoreAway, out var awayScore) && int.TryParse(scoreHome, out var homeScore)
                        ? (awayScore > homeScore ? abbrevAway : abbrevHome) : "unknown";
                    if (eventType == "MLB" || eventType == "College_Baseball") //Post-game for baseball is COMPLETE
                    {
                        if (eventType == "MLB") {
                            leagueCode = "mlb";
                        } else {
                            leagueCode = "college-baseball";
                        }
                        // Fetch the post-game pitcher spans (joined by |)
                        string postGameDetails = await GetScoreboardPropertyAsync("baseball", leagueCode, "competitions.postGamePitcher", gameId);
                        string postGameFinal = await GetScoreboardPropertyAsync("baseball", leagueCode, "competitions.postGameStatus", gameId);
                        // Helper to extract teamId from span
                        string GetTeamIdFromSpan(string span)
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(span, "data-teamid='(\\d+)'");
                            return match.Success ? match.Groups[1].Value : "";
                        }
                        // Split the spans and assign to home/away
                        string awayPostGamePitcher = "", homePostGamePitcher = "";
                        var spans = postGameDetails.Split('|', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var span in spans)
                        {
                            var teamId = GetTeamIdFromSpan(span);
                            if (teamId == awayTeamID) awayPostGamePitcher = span;
                            else if (teamId == homeTeamID) homePostGamePitcher = span;
                        }
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <!-- Row 1 -->
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{abbrevAway}  <span style='color: white; font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span><br>{awayPostGamePitcher}</span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{scoreAway}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{abbrevHome}  <span style='color: white; font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span><br>{homePostGamePitcher}</span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{scoreHome}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <span style='opacity:0.6;'> {postGameFinal} </span>
                            <!-- Row 2 -->
                            <span style='opacity:0.6;'></span><!-- Empty Span for Row 2 -->
                            <!-- You can add more info here if needed -->
                        </div>
                        ");
                    } else if (eventType == "NBA" || eventType == "College_Basketball") //Post-game for basketball is COMPLETE
                    {
                        if (eventType == "NBA") {
                            leagueCode = "nba";
                        } else {
                            leagueCode = "mens-college-basketball";
                        }
                        string postGameHeadline = await GetScoreboardPropertyAsync("basketball", leagueCode, "competitions.leaders", gameId);
                        string postGameFinalBB = await GetScoreboardPropertyAsync("basketball", leagueCode, "competitions.postGameStatus", gameId);
                        string GetTeamIdFromSpan(string span)
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(span, "data-teamid='(\\d+)'");
                            return match.Success ? match.Groups[1].Value : "";
                        }
                        // postGameHeadline is the string returned from GetScoreboardPropertyAsync
                        string awayLeaderSpan = "", homeLeaderSpan = "";
                            var spans = postGameHeadline.Split('|', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var span in spans)
                            {
                                var teamId = GetTeamIdFromSpan(span);
                                if (teamId == awayTeamID) awayLeaderSpan = span;
                                else if (teamId == homeTeamID) homeLeaderSpan = span;
                            }
                        // Show final score for basketball -- Done
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <!-- Row 1 -->
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='height: 80px; font-size: 24px; font-weight:bold; margin-bottom: -10px; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'> {abbrevAway}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span><br />{awayLeaderSpan}</span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{scoreAway}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='height: 80px; font-size: 24px; font-weight:bold; margin-bottom: -10px; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{abbrevHome}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span><br /><span style='color: white;'>{homeLeaderSpan}</span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{scoreHome}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <span style='opacity:0.6;'> {postGameFinalBB} </span>
                            <!-- Row 2 -->
                            <!-- <span style='grid-row: 2; grid-column: span 4; text-align: left; font-size: 14px; opacity:0.8;'> {postGameHeadline} </span> -->
                        </div>");
                    } else if (eventType == "NFL" || eventType == "College_Football")
                    {
                        if (eventType == "NFL") {
                            leagueCode = "nfl";
                        } else {
                            leagueCode = "college-football";
                        }
                        // Show final score for football
                        results.Add($@"
                        <div style='display: grid; grid-template-columns: auto auto auto auto auto auto auto auto auto; align-items: center; gap: 10px; max-width: 100%; overflow: hidden; text-overflow: ellipsis;'>
                            <img src='{logoAway}' alt='{nameAway}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{abbrevAway}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordAway} </span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevAway ? "color:yellow;" : "opacity:0.8;")}'>{scoreAway}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <img src='{logoHome}' alt='{nameHome}' height='40' style='vertical-align:middle;' />
                            <span style='font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{abbrevHome}  <span style='font-size: 12px; opacity:0.6; vertical-align:middle;'> {recordHome} </span></span>
                            <span style='width: 20px;'></span> <!-- Empty cell -->
                            <span style='align: right; font-size: 24px; font-weight:bold; {(winner == abbrevHome ? "color:yellow;" : "opacity:0.8;")}'>{scoreHome}</span>
                            <div style='border-left: 1px solid #ccc; height: 40px;'></div>
                            <span style='opacity:0.6;'> FINAL </span>
                        </div>
                        ");
                    }
                }
                else
                {
                    // Default case for unknown status
                    results.Add($"{team.DisplayName}: Status unknown.");
                }
        }
        catch (Exception ex)
        {
                results.Add($"{team.DisplayName}: Error - {ex.Message}");
        }
    }
    return results;
    }
    public async Task<List<string>> GetSchoolEventsAsync()
    {
        var icsUrl = "https://www.CCS.k12.in.us/fs/calendar-manager/events.ics?calendar_ids[]=171&calendar_ids[]=3";
        var results = new List<string>();

        try
        {
            // Fetch the ICS file
            var icsData = await _http.GetStringAsync(icsUrl);

            // Parse the ICS file
            var calendar = Calendar.Load(icsData);

            // Extract and filter events
            var upcomingEvents = calendar.Events
                .Where(e => e.Start.AsSystemLocal > DateTime.Now) // Only future events
                .OrderBy(e => e.Start.AsSystemLocal)             // Sort by start date
                .Take(3);                                        // Take the next 3 events

            foreach (var calendarEvent in upcomingEvents)
            {
                var eventTitle = calendarEvent.Summary ?? "No Title";
                var eventStart = calendarEvent.Start.AsSystemLocal.ToString("MMM dd");
                var eventEnd = calendarEvent.End.AsSystemLocal.ToString("MMM dd");
                // Only include eventEnd if it is different from eventStart
                var eventDateDisplay = eventStart;
                if (eventEnd != eventStart)
                {
                    eventDateDisplay += $" - {eventEnd}";
                }
                results.Add($"<div style='display: flex; align-items: center; height: 100%; max-height: 80px; width: auto;'> {eventTitle} <br>{eventDateDisplay}</div>");
            }
        }
        catch (Exception ex)
        {
            results.Add($"Error fetching school events: {ex.Message}");
        }

        return results;
    }
    public async Task<List<string>> GetLocalEventsAsync()
    {
        var results = new List<string>();
        var rssUrl = "https://www.visithamiltoncounty.com/event/rss/";

        try
        {
            // Fetch the RSS feed
            var rssData = await _http.GetStringAsync(rssUrl);

            // Load the RSS feed
            using var stringReader = new StringReader(rssData);
            using var xmlReader = XmlReader.Create(stringReader);
            var feed = SyndicationFeed.Load(xmlReader);

            if (feed != null)
            {
                // Filter events by category "Carmel"
                var filteredEvents = feed.Items
                    .Where(item =>
                        item.Categories.Any(category =>
                        category.Name.Contains("Carmel", StringComparison.OrdinalIgnoreCase)))
                    .Select(item =>
                    {
                    var title = item.Title?.Text ?? "No Title";
                    var link = item.Links.FirstOrDefault()?.Uri.ToString() ?? "No Link";
                    var description = item.Summary?.Text ?? "";
                    var eventDetails = ExtractEventDetails(description);
                    var imageUrl = ExtractImageUrl(description);

                    // Format the result with the image to the right
                    return $@"
                        <div style='display: flex; align-items: center; height: 100%; max-height: 80px; width: auto;'>
                            <div style='margin-right: 20px;'>
                                <img src='{imageUrl}' alt='Event Image' style='border-radius: 45%; height: 100%; max-height: 80px; width: auto;' />
                            </div>
                            <div style='flex: 1;'>
                                {title} <br>{eventDetails} <br><a href='{link}' target='_blank'>More Info</a>
                            </div>
                        </div>";
                    })
                    .Take(5); // Take the next 5 events

                results.AddRange(filteredEvents);

                if (!results.Any())
                {
                    results.Add("No events found for the category 'Carmel'.");
                }
            }
            else
            {
                results.Add("No events found in the RSS feed.");
            }
        }
        catch (Exception ex)
        {
            results.Add($"Error fetching local events: {ex.Message}");
        }

        return results;
    }
    public async Task<List<string>> GetStockPricesAsync()
    {
        var symbols = new[] { "NVDA", "AAPL", "CAVA", "ORCL", "ANET", "WTKWY", "GOOGL", "AMZN", "META", "TSLA" };
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
                var previousClose = quoteJson.GetProperty("pc").GetDouble(); // "pc" is the previous close price
                var highPrice = quoteJson.GetProperty("h").GetDouble(); // "h" is the daily high price
                var lowPrice = quoteJson.GetProperty("l").GetDouble(); // "l" is the daily low price
                var openPrice = quoteJson.GetProperty("o").GetDouble(); // "o" is the market open price
                // Determine the color based on percentage change
                var color = percentChange >= 0 ? "green" : "red";

                // Add result to the list
                results.Add($@"
                    <div style='display: flex; align-items: center;'>
                    <div style='margin-right: 20px;'>
                        <img src='{logo}' alt='{symbol}' style='vertical-align:middle; margin-right: 10px; height: 90px; max-height: 90px; width: auto;' />
                    </div>
                    <div style='flex: 1;'>
                        <span>{symbol}: ${price}</span><br>
                        <span style='color: {color}; margin-left: 10px;'>({percentChange:+0.00;-0.00}%)</span>
                    </div>
                </div>");
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
                    } else if (propertyPath == "event.ShortDetailstatus")
                    {
                        var status = scoreboardData.GetProperty("status").GetProperty("type").GetProperty("shortDetail").GetString() ?? "No status available";
                        return status;
                    }
                    else if (propertyPath == "event.homeScore")
                    {
                        var homeScore = competitionsSB[0].GetProperty("competitors")[0].GetProperty("score").GetString() ?? "No home score available";
                        return homeScore;
                    }
                    else if (propertyPath == "event.awayScore")
                    {
                        var awayScore = competitionsSB[0].GetProperty("competitors")[1].GetProperty("score").GetString() ?? "No away score available";
                        return awayScore;
                    }
                    else if (propertyPath == "competitions.lastPlay")
                    {
                        var lastPlay = competitionsSB[0].GetProperty("situation").GetProperty("lastPlay").GetProperty("text").GetString() ?? "No last play information available";
                        return lastPlay;
                    }
                    else if (propertyPath == "competitions.count")
                    {
                        var balls = competitionsSB[0].GetProperty("situation").GetProperty("balls").GetInt32();
                        var strikes = competitionsSB[0].GetProperty("situation").GetProperty("strikes").GetInt32();
                        //var outs = competitionsSB[0].GetProperty("situation").GetProperty("outs").GetInt32();
                        //var outWord = outs == 1 ? "out" : "outs";
                        var count = $"{balls} - {strikes}";
                        return count;
                    }
                    else if (propertyPath == "competitions.outs") 
                    {
                        var outs = competitionsSB[0].GetProperty("situation").GetProperty("outs").GetInt32();
                        // outs is an int (0, 1, 2, or 3)
                        if(outs < 0 || outs > 3)
                        {
                            string outSvg = @"
                            <svg width='40' height='20' viewBox='0 0 60 20'>
                            <circle cx='10' cy='10' r='6'' fill='#FFF' stroke='#333' stroke-width='2'/>
                            <circle cx='30' cy='10' r='6'' fill='#FFF' stroke='#333' stroke-width='2'/>
                            <circle cx='50' cy='10' r='6'' fill='#FFF' stroke='#333' stroke-width='2'/>
                            </svg>";
                            return outSvg;
                        } else {
                        string outSvg = $@"
                            <svg width='40' height='20' viewBox='0 0 60 20'>
                            <circle cx='10' cy='10' r='6'' fill='{(outs > 0 ? "#FFD700" : "#FFF")}' stroke='#333' stroke-width='2'/>
                            <circle cx='30' cy='10' r='6'' fill='{(outs > 1 ? "#FFD700" : "#FFF")}' stroke='#333' stroke-width='2'/>
                            <circle cx='50' cy='10' r='6'' fill='{(outs > 2 ? "#FFD700" : "#FFF")}' stroke='#333' stroke-width='2'/>
                            </svg>";
                            return outSvg;
                        }
                    }
                    else if (propertyPath == "competitions.bases")
                    {
                        bool onFirst = false, onSecond = false, onThird = false;
                        // Extract base runner booleans from situation
                        onFirst = competitionsSB[0].GetProperty("situation").GetProperty("onFirst").GetBoolean();
                        onSecond = competitionsSB[0].GetProperty("situation").GetProperty("onSecond").GetBoolean();
                        onThird = competitionsSB[0].GetProperty("situation").GetProperty("onThird").GetBoolean();
                        if (onFirst || onSecond || onThird)
                        {
                        // Create SVG here (step 2)
                            string baseSvg = $@"
                            <svg width='40' height='40' viewBox='0 0 60 60'>
                            <rect x='25' y='5' width='10' height='10' fill='{(onSecond ? "#FFD700" : "#DDD")}' stroke='#333'/>
                            <rect x='5' y='25' width='10' height='10' fill='{(onThird ? "#FFD700" : "#DDD")}' stroke='#333'/>
                            <rect x='45' y='25' width='10' height='10' fill='{(onFirst ? "#FFD700" : "#DDD")}' stroke='#333'/>
                            <rect x='25' y='45' width='10' height='10' fill='#FFF' stroke='#333'/>
                            </svg>";
                            return baseSvg;
                        }
                        else
                        {
                            string baseSvg = @"
                            <svg width='40' height='40' viewBox='0 0 60 60'>
                            <rect x='25' y='5' width='10' height='10' fill='#DDD' stroke='#333'/>
                            <rect x='5' y='25' width='10' height='10' fill='#DDD' stroke='#333'/>
                            <rect x='45' y='25' width='10' height='10' fill='#DDD' stroke='#333'/>
                            <rect x='25' y='45' width='10' height='10' fill='#FFF' stroke='#333'/>
                            </svg>";
                            return baseSvg;
                        }
                    }
                    else if (propertyPath == "competitions.pitcher") 
                    {
                        var pitcher = competitionsSB[0].GetProperty("situation").GetProperty("pitcher").GetProperty("athlete").GetProperty("shortName").GetString();
                        var pitcherImg = competitionsSB[0].GetProperty("situation").GetProperty("pitcher").GetProperty("athlete").GetProperty("headshot").GetString();
                        var pitcherTeamId = competitionsSB[0].GetProperty("situation").GetProperty("pitcher").GetProperty("athlete").GetProperty("team").GetProperty("id").GetString();
                        var pitcherSummary = competitionsSB[0].GetProperty("situation").GetProperty("pitcher").GetProperty("summary").GetString() ?? "No pitcher summary available";
                        var pitcherSpan = $@"<span data-teamid='{pitcherTeamId}' style='font-size: 14px; opacity:0.8; display: flex; align-items: center; gap: 6px;'>
                        <img src='{pitcherImg}' alt='{pitcher}' style='height:18px; width:18px; border-radius:50%; vertical-align:middle; margin-right:4px;' />{pitcher} <span style='font-size: 12px; opacity:0.6;'>{pitcherSummary}</span>
                        </span>";
                        return pitcherSpan;
                    }
                    else if (propertyPath == "competitions.batter") 
                    {
                        var batter = competitionsSB[0].GetProperty("situation").GetProperty("batter").GetProperty("athlete").GetProperty("shortName").GetString();
                        var batterImg = competitionsSB[0].GetProperty("situation").GetProperty("batter").GetProperty("athlete").GetProperty("headshot").GetString();
                        var batterTeamId = competitionsSB[0].GetProperty("situation").GetProperty("batter").GetProperty("athlete").GetProperty("team").GetProperty("id").GetString();
                        var batterSummary = competitionsSB[0].GetProperty("situation").GetProperty("batter").GetProperty("summary").GetString() ?? "No batter summary available";
                        var batterSpan = $@"<span data-teamid='{batterTeamId}' style='font-size: 14px; opacity:0.8; display: flex; align-items: center; gap: 6px;'>
                        <img src='{batterImg}' alt='{batter}' style='height:18px; width:18px; border-radius:50%; vertical-align:middle; margin-right:4px;' />{batter} <span style='font-size: 12px; opacity:0.6;'>{batterSummary}</span>
                        </span>";
                        return batterSpan;
                    }
                    else if (propertyPath == "competitions.leaders")
                    {
                        var leadersSpans = new List<string>();
                        var competitors = competitionsSB[0].GetProperty("competitors");
                        foreach (var competitor in competitors.EnumerateArray())
                        {
                            if (competitor.TryGetProperty("leaders", out var leadersArray))
                            {
                                foreach (var leaderCategory in leadersArray.EnumerateArray())
                                {
                                    var name = leaderCategory.GetProperty("name").GetString();
                                    if (name == "rating")
                                    {
                                        if (leaderCategory.TryGetProperty("leaders", out var ratingLeaders))
                                        {
                                            foreach (var leader in ratingLeaders.EnumerateArray())
                                            {
                                                if (leader.TryGetProperty("displayValue", out var displayValueProp) &&
                                                    leader.TryGetProperty("athlete", out var athlete) &&
                                                    athlete.TryGetProperty("shortName", out var athleteshortNameProp) &&
                                                    athlete.TryGetProperty("headshot", out var athleteHeadshotProp) &&
                                                    athlete.TryGetProperty("team", out var athleteTeam) &&
                                                    athleteTeam.TryGetProperty("id", out var athleteTeamIdProp))
                                                {
                                                    var displayValue = displayValueProp.GetString();
                                                    var athleteshortName = athleteshortNameProp.GetString();
                                                    var athleteHeadshot = athleteHeadshotProp.GetString();
                                                    var athleteTeamId = athleteTeamIdProp.GetString();
                                                    var leaderSpan = $@"<span data-teamid='{athleteTeamId}' style='font-size: 12px; opacity:0.8; display: flex; align-items: center; gap: 6px; vertical-align:top; margin-top: -5px;'>
                                                        <img src='{athleteHeadshot}' alt='{athleteshortName}' style='height:25px; width:25px; border-radius:50%; margin-right:4px;' />{athleteshortName} <span style='font-size: 12px; opacity:0.6;'>{displayValue}</span>
                                                        </span>";
                                                        // return leaderSpan;
                                                    leadersSpans.Add(leaderSpan);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // Return both leader spans, joined or as a list as needed
                        return string.Join("|", leadersSpans);
                    }
                    else if (propertyPath == "competitions.probables")
                    {
                        var probablesSpans = new List<string>();
                        var competitors = competitionsSB[0].GetProperty("competitors");
                        foreach (var competitor in competitors.EnumerateArray())
                        {
                            if (competitor.TryGetProperty("probables", out var probablesArray))
                            {
                                foreach (var probable in probablesArray.EnumerateArray())
                                {
                                    if (probable.TryGetProperty("athlete", out var athlete) &&
                                        athlete.TryGetProperty("shortName", out var shortNameProp) &&
                                        athlete.TryGetProperty("headshot", out var headshotProp) &&
                                        athlete.TryGetProperty("team", out var team) &&
                                        team.TryGetProperty("id", out var teamIdProp) && 
                                        probable.TryGetProperty("record", out var recordProp))
                                    {
                                        var shortName = shortNameProp.GetString();
                                        var headshot = headshotProp.GetString();
                                        var teamId = teamIdProp.GetString();
                                        var pitcherRecord = recordProp.GetString() ?? "No record available";
                                        var probablesSpan = $@"<span data-teamid='{teamId}' style='font-size: 12px; opacity:0.8; display: flex; align-items: center; gap: 6px; vertical-align:top; margin-top: -5px;'>
                                            <img src='{headshot}' alt='{shortName}' style='height:25px; width:25px; border-radius:50%; margin-right:4px;' />{shortName} <span style='font-size: 12px; opacity:0.6;'>{pitcherRecord} ERA</span>
                                            </span>";
                                        probablesSpans.Add(probablesSpan);
                                    }
                                }
                            }
                        }
                        return string.Join("|", probablesSpans);
                    }
                    else if (propertyPath == "competitions.headlines")
                    {
                        // Get the headlines for the game
                        var headlines = competitionsSB[0].GetProperty("headlines");
                        if (headlines.GetArrayLength() > 0)
                        {
                            var headlineText = headlines[0].GetProperty("shortLinkText").GetString() ?? "No headline available";
                            return headlineText;
                        }
                        else
                        {
                            return "No headlines available for this game.";
                        }
                    }
                    else if (propertyPath == "competitions.description")
                    {
                        // Get the description of the game
                        var description = competitionsSB[0].GetProperty("description").GetString() ?? "No description available";
                        return description;
                    }
                    else if (propertyPath == "competitions.postGamePitcher")
                    {
                        var pitcherSpans = new List<string>();
                        // Defensive: check for status and featuredAthletes
                        if (competitionsSB[0].TryGetProperty("status", out var status) &&
                            status.TryGetProperty("featuredAthletes", out var featuredAthletes) &&
                            featuredAthletes.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var athleteObj in featuredAthletes.EnumerateArray())
                            {
                                if (athleteObj.TryGetProperty("athlete", out var athlete) &&
                                    athlete.TryGetProperty("shortName", out var shortNameProp) &&
                                    athlete.TryGetProperty("headshot", out var headshotProp) &&
                                    athlete.TryGetProperty("team", out var team) &&
                                    team.TryGetProperty("id", out var teamIdProp))
                                {
                                    var shortName = shortNameProp.GetString();
                                    var headshot = headshotProp.GetString();
                                    var teamId = teamIdProp.GetString();

                                    // Defensive: check for statistics array
                                    string record = "";
                                    if (athleteObj.TryGetProperty("statistics", out var statsArray) && statsArray.ValueKind == JsonValueKind.Array)
                                    {
                                        // Try to get stats for W-L, ERA, Saves, etc.
                                        var stats = statsArray.EnumerateArray().ToList();
                                        if (stats.Count >= 3)
                                        {
                                            var wins = stats[2].GetProperty("displayValue").GetString() ?? "";
                                            var losses = stats[1].GetProperty("displayValue").GetString() ?? "";
                                            var era = stats.Count > 3 ? stats[3].GetProperty("displayValue").GetString() ?? "" : "";
                                            var saves = stats[0].GetProperty("displayValue").GetString() ?? "";
                                            // Compose record string based on available stats
                                            if (!string.IsNullOrEmpty(saves))
                                                record = $"{wins}-{losses}, {saves} Saves";
                                            else
                                                record = $"{wins}-{losses}, {era} ERA";
                                        }
                                    }
                                    var pitcherSpan = $@"<span data-teamid='{teamId}' style='font-size: 14px; opacity:0.8; display: flex; align-items: center; gap: 6px;'>
                                        <img src='{headshot}' alt='{shortName}' style='height:18px; width:18px; border-radius:50%; vertical-align:middle; margin-right:4px;' />{shortName} <span style='color: white; font-size: 12px; opacity:0.6;'>{record}</span>
                                        </span>";
                                    pitcherSpans.Add(pitcherSpan);
                                }
                            }
                        }
                        return string.Join("|", pitcherSpans);
                    } 
                    else if (propertyPath == "competitions.postGameStatus")
                    {
                        // Get the headlines for the game
                        var postGameStatus = competitionsSB[0].GetProperty("status").GetProperty("type").GetProperty("shortDetail").GetString() ?? "No post game status available";
                        return postGameStatus;
                    }
                    else
                    {
                        // Handle other properties as needed
                        return $"Property '{propertyPath}' not recognized.";
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

    // Helper method to extract Local event details from the description
    private string ExtractEventDetails(string description)
    {
        try
        {
            // Load the description as HTML
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(description);

            // Extract the raw text from the description
            var rawText = HtmlEntity.DeEntitize(htmlDoc.DocumentNode.InnerText).Trim();

            // Match the date range using regex
            var dateRegex = new Regex(@"\b(\d{2}/\d{2}/\d{4})\b\s*to\s*\b(\d{2}/\d{2}/\d{4})\b");
            var match = dateRegex.Match(rawText);

            if (match.Success)
            {
                // Parse the start and end dates
                var startDate = DateTime.TryParse(match.Groups[1].Value, out var start) ? start.ToString("MMM dd, yyyy") : "Invalid Start Date";
                var endDate = DateTime.TryParse(match.Groups[2].Value, out var end) ? end.ToString("MMM dd, yyyy") : "Invalid End Date";

                // Format the date range
                return startDate == endDate ? startDate : $"{startDate} - {endDate}";
            }

            // If no date range is found, return a default message
            return "No Date Range Found";
        }
        catch (Exception ex)
        {
            // Log the exception and return a default message
            Console.WriteLine($"Error extracting event details: {ex.Message}");
            return "Error Extracting Event Details";
        }
    }

    // Helper method to extract the image URL from the description for Local events
    private string ExtractImageUrl(string description)
    {
        try
        {
            // Load the description as HTML
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(description);

            // Find the first <img> tag and get its src attribute
            var imgNode = htmlDoc.DocumentNode.SelectSingleNode("//img");
            return imgNode?.GetAttributeValue("src", "No Image Available") ?? "No Image Available";
        }
        catch (Exception ex)
        {
            // Log the exception and return a default message
            Console.WriteLine($"Error extracting image URL: {ex.Message}");
            return "No Image Available";
        }
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
    public string GetEventUrl(string gameId) =>$"https://site.api.espn.com/apis/site/v2/sports/{Sport}/{League}/scoreboard/summary?event={gameId}";

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