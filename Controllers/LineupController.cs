using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeatingChartApp.Data;
using SeatingChartApp.Models;

namespace SeatingChartApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LineupController : ControllerBase
    {
        private readonly LineupDbContext _context;
        
        public LineupController(LineupDbContext context)
        {
            _context = context;
        }
        
        // Teams
        [HttpGet("teams")]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            return await _context.Teams
                .Include(t => t.Players.Where(p => p.IsActive))
                .Include(t => t.Games)
                .ToListAsync();
        }
        
        [HttpPost("teams")]
        public async Task<ActionResult<Team>> CreateTeam(Team team)
        {
            team.CreatedDate = DateTime.UtcNow;
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTeams), new { id = team.Id }, team);
        }
        
        [HttpPut("teams/{id}")]
        public async Task<ActionResult<Team>> UpdateTeam(int id, Team team)
        {
            var existing = await _context.Teams.FindAsync(id);
            if (existing == null)
                return NotFound();
            
            existing.Name = team.Name;
            existing.Coach = team.Coach;
            existing.League = team.League;
            existing.HomeField = team.HomeField;
            
            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        
        // Players
        [HttpGet("teams/{teamId}/players")]
        public async Task<ActionResult<IEnumerable<Player>>> GetPlayers(int teamId)
        {
            return await _context.Players
                .Where(p => p.TeamId == teamId && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        
        [HttpPost("teams/{teamId}/players")]
        public async Task<ActionResult<Player>> CreatePlayer(int teamId, Player player)
        {
            player.TeamId = teamId;
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
            return Ok(player);
        }
        
        [HttpPut("players/{id}")]
        public async Task<IActionResult> UpdatePlayer(int id, Player player)
        {
            var existing = await _context.Players.FindAsync(id);
            if (existing == null)
                return NotFound();
            
            existing.Name = player.Name;
            existing.JerseyNumber = player.JerseyNumber;
            existing.PreferredPosition = player.PreferredPosition;
            existing.IsActive = player.IsActive;
            
            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        
        [HttpDelete("players/{id}")]
        public async Task<IActionResult> DeletePlayer(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player == null)
                return NotFound();
                
            player.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        
        // Sync all players for a team in one call (update existing, add new, deactivate removed)
        [HttpPost("teams/{teamId}/players/sync")]
        public async Task<ActionResult<IEnumerable<Player>>> SyncPlayers(int teamId, List<Player> players)
        {
            var existingPlayers = await _context.Players
                .Where(p => p.TeamId == teamId && p.IsActive)
                .ToListAsync();
            
            var incomingIds = players.Where(p => p.Id > 0).Select(p => p.Id).ToHashSet();
            
            // Deactivate players not in the incoming list
            foreach (var existing in existingPlayers)
            {
                if (!incomingIds.Contains(existing.Id))
                {
                    existing.IsActive = false;
                }
            }
            
            var result = new List<Player>();
            
            foreach (var player in players)
            {
                if (player.Id > 0)
                {
                    // Update existing player
                    var existing = existingPlayers.FirstOrDefault(p => p.Id == player.Id);
                    if (existing != null)
                    {
                        existing.Name = player.Name;
                        existing.JerseyNumber = player.JerseyNumber;
                        existing.PreferredPosition = player.PreferredPosition;
                        existing.IsActive = true;
                        result.Add(existing);
                    }
                }
                else
                {
                    // New player
                    player.TeamId = teamId;
                    player.IsActive = true;
                    _context.Players.Add(player);
                    result.Add(player);
                }
            }
            
            await _context.SaveChangesAsync();
            return Ok(result);
        }
        
        // Games
        [HttpGet("teams/{teamId}/games")]
        public async Task<ActionResult<IEnumerable<Game>>> GetGames(int teamId)
        {
            return await _context.Games
                .Where(g => g.TeamId == teamId)
                .OrderByDescending(g => g.GameDate)
                .ToListAsync();
        }
        
        [HttpPost("teams/{teamId}/games")]
        public async Task<ActionResult<Game>> CreateGame(int teamId, Game game)
        {
            game.TeamId = teamId;
            _context.Games.Add(game);
            await _context.SaveChangesAsync();
            return Ok(game);
        }
        
        [HttpPut("games/{id}")]
        public async Task<ActionResult<Game>> UpdateGame(int id, Game game)
        {
            var existing = await _context.Games.FindAsync(id);
            if (existing == null)
                return NotFound();
            
            existing.Opponent = game.Opponent;
            existing.GameDate = game.GameDate;
            existing.Location = game.Location;
            existing.IsHome = game.IsHome;
            
            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        
        // Game Lineups
        [HttpGet("games/{gameId}/lineup")]
        public async Task<ActionResult<IEnumerable<GameLineupPosition>>> GetGameLineup(int gameId)
        {
            return await _context.GameLineupPositions
                .Include(glp => glp.Player)
                .Where(glp => glp.GameId == gameId)
                .OrderBy(glp => glp.Inning)
                .ThenBy(glp => glp.BattingOrder)
                .ToListAsync();
        }
        
        [HttpPost("games/{gameId}/lineup")]
        public async Task<ActionResult> SaveGameLineup(int gameId, List<GameLineupPosition> lineup)
        {
            // Remove existing lineup for this game
            var existingLineup = await _context.GameLineupPositions
                .Where(glp => glp.GameId == gameId)
                .ToListAsync();
            _context.GameLineupPositions.RemoveRange(existingLineup);
            
            // Add new lineup
            foreach (var position in lineup)
            {
                position.Id = 0; // Reset ID so EF treats as new
                position.GameId = gameId;
                _context.GameLineupPositions.Add(position);
            }
            
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpPut("games/{gameId}/lineup/{inning}")]
        public async Task<ActionResult> UpdateInningLineup(int gameId, int inning, List<GameLineupPosition> positions)
        {
            // Remove existing positions for this game/inning
            var existingPositions = await _context.GameLineupPositions
                .Where(glp => glp.GameId == gameId && glp.Inning == inning)
                .ToListAsync();
            _context.GameLineupPositions.RemoveRange(existingPositions);
            
            // Add new positions
            foreach (var position in positions)
            {
                position.Id = 0;
                position.GameId = gameId;
                position.Inning = inning;
                _context.GameLineupPositions.Add(position);
            }
            
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.Id == id);
        }
    }
}