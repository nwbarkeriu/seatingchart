using Microsoft.EntityFrameworkCore;
using SeatingChartApp.Models;

namespace SeatingChartApp.Data
{
    public class MealPlannerDbContext : DbContext
    {
        public MealPlannerDbContext(DbContextOptions<MealPlannerDbContext> options) : base(options) { }
        
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<MealPlanRecipe> MealPlanRecipes { get; set; }
        public DbSet<ShoppingList> ShoppingLists { get; set; }
        public DbSet<ShoppingListItem> ShoppingListItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure MealPlanRecipe relationships
            modelBuilder.Entity<MealPlanRecipe>()
                .HasOne(mpr => mpr.MealPlan)
                .WithMany(mp => mp.Recipes)
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
                
            // Create indexes for better performance
            modelBuilder.Entity<MealPlan>()
                .HasIndex(mp => mp.Date)
                .IsUnique();
                
            modelBuilder.Entity<Recipe>()
                .HasIndex(r => r.Category);
                
            // Seed some default data
            modelBuilder.Entity<Recipe>().HasData(
                new Recipe 
                { 
                    Id = 1, 
                    Name = "Spaghetti & Meatballs", 
                    Category = "Dinner", 
                    PrepTimeMinutes = 15, 
                    CookTimeMinutes = 30, 
                    Servings = 6,
                    Ingredients = "1 lb ground beef, 1 lb spaghetti, marinara sauce, garlic, onion, breadcrumbs, egg",
                    Instructions = "1. Make meatballs with beef, breadcrumbs, egg. 2. Cook spaghetti. 3. Simmer meatballs in sauce.",
                    IsFavorite = true,
                    CreatedDate = new DateTime(2025, 9, 21)
                },
                new Recipe 
                { 
                    Id = 2, 
                    Name = "Pancakes", 
                    Category = "Breakfast", 
                    PrepTimeMinutes = 10, 
                    CookTimeMinutes = 15, 
                    Servings = 6,
                    Ingredients = "2 cups flour, 2 eggs, 1.5 cups milk, 2 tbsp sugar, 1 tsp baking powder",
                    Instructions = "1. Mix dry ingredients. 2. Whisk wet ingredients. 3. Combine and cook on griddle.",
                    IsFavorite = true,
                    CreatedDate = new DateTime(2025, 9, 21)
                }
            );
        }
    }
}