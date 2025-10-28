using Microsoft.EntityFrameworkCore;
using SeatingChartApp.Models;

namespace SeatingChartApp.Data
{
    public class LineupDbContext : DbContext
    {
        public LineupDbContext(DbContextOptions<LineupDbContext> options) : base(options) { }
        
        // Lineup-related tables
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameLineupPosition> GameLineupPositions { get; set; }
        
        // Meal planner tables
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<MealPlanRecipe> MealPlanRecipes { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Team-Player relationship
            modelBuilder.Entity<Player>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure Team-Game relationship
            modelBuilder.Entity<Game>()
                .HasOne(g => g.Team)
                .WithMany(t => t.Games)
                .HasForeignKey(g => g.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure GameLineupPosition relationships
            modelBuilder.Entity<GameLineupPosition>()
                .HasOne(glp => glp.Game)
                .WithMany(g => g.LineupPositions)
                .HasForeignKey(glp => glp.GameId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<GameLineupPosition>()
                .HasOne(glp => glp.Player)
                .WithMany(p => p.LineupPositions)
                .HasForeignKey(glp => glp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Create composite index for efficient queries
            modelBuilder.Entity<GameLineupPosition>()
                .HasIndex(glp => new { glp.GameId, glp.Inning, glp.Position })
                .IsUnique();
                
            // Configure MealPlan relationships
            modelBuilder.Entity<MealPlanRecipe>()
                .HasOne(mpr => mpr.MealPlan)
                .WithMany()
                .HasForeignKey(mpr => mpr.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<MealPlanRecipe>()
                .HasOne(mpr => mpr.Recipe)
                .WithMany(r => r.MealPlans)
                .HasForeignKey(mpr => mpr.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure ShoppingList relationships
            modelBuilder.Entity<ShoppingListItem>()
                .HasOne(sli => sli.ShoppingList)
                .WithMany(sl => sl.Items)
                .HasForeignKey(sli => sli.ShoppingListId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Seed some default data
            modelBuilder.Entity<Team>().HasData(
                new Team { Id = 1, Name = "Barker Family Baseball", Coach = "Nathan Barker", League = "Family League", CreatedDate = new DateTime(2025, 9, 21) }
            );
            
            // Seed some default recipes
            modelBuilder.Entity<Recipe>().HasData(
                new Recipe { Id = 1, Name = "Spaghetti & Meatballs", Category = "Dinner", PrepTimeMinutes = 15, CookTimeMinutes = 30, Servings = 4, IsFavorite = true },
                new Recipe { Id = 2, Name = "Pancakes", Category = "Breakfast", PrepTimeMinutes = 10, CookTimeMinutes = 15, Servings = 4, IsFavorite = true },
                new Recipe { Id = 3, Name = "Grilled Cheese", Category = "Lunch", PrepTimeMinutes = 5, CookTimeMinutes = 10, Servings = 2, IsFavorite = true }
            );
        }
    }
}