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
                .Include(t => t.Players)
                .Include(t => t.Games)
                .ToListAsync();
        }
        
        [HttpPost("teams")]
        public async Task<ActionResult<Team>> CreateTeam(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTeams), new { id = team.Id }, team);
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
            if (id != player.Id)
                return BadRequest();
                
            _context.Entry(player).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(id))
                    return NotFound();
                throw;
            }
            
            return NoContent();
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