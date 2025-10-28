using System.ComponentModel.DataAnnotations;

namespace SeatingChartApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Coach { get; set; }
        public string? League { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public List<Player> Players { get; set; } = new();
        public List<Game> Games { get; set; } = new();
    }
    
    public class Player
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public int JerseyNumber { get; set; }
        public string? PreferredPosition { get; set; }
        public bool IsActive { get; set; } = true;
        
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        
        public List<GameLineupPosition> LineupPositions { get; set; } = new();
    }
    
    public class Game
    {
        public int Id { get; set; }
        
        [Required]
        public string Opponent { get; set; } = string.Empty;
        
        public DateTime GameDate { get; set; }
        public string? Location { get; set; }
        public bool IsHome { get; set; } = true;
        
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;
        
        public List<GameLineupPosition> LineupPositions { get; set; } = new();
    }
    
    public class GameLineupPosition
    {
        public int Id { get; set; }
        
        public int GameId { get; set; }
        public Game Game { get; set; } = null!;
        
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        
        public int Inning { get; set; }
        public string Position { get; set; } = string.Empty; // P, C, 1B, 2B, 3B, SS, LF, CF, RF, DH, etc.
        public int BattingOrder { get; set; } // 1-9 for starting lineup
    }
}