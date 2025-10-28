using System.ComponentModel.DataAnnotations;

namespace SeatingChartApp.Models
{
    public class MealPlan
    {
        public int Id { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public string? Breakfast { get; set; }
        public string? Lunch { get; set; }
        public string? Dinner { get; set; }
        public string? Snacks { get; set; }
        
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        public List<MealPlanRecipe> Recipes { get; set; } = new();
    }
    
    public class Recipe
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        public string? Ingredients { get; set; }
        public string? Instructions { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookTimeMinutes { get; set; }
        public int Servings { get; set; }
        
        public string? Category { get; set; } // Breakfast, Lunch, Dinner, Snack, Dessert
        public string? Cuisine { get; set; }
        public int? Rating { get; set; } // 1-5 stars
        
        public bool IsFavorite { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastMadeDate { get; set; }
        
        public List<MealPlanRecipe> MealPlans { get; set; } = new();
    }
    
    public class MealPlanRecipe
    {
        public int Id { get; set; }
        
        public int MealPlanId { get; set; }
        public MealPlan MealPlan { get; set; } = null!;
        
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = null!;
        
        public string MealType { get; set; } = string.Empty; // Breakfast, Lunch, Dinner, Snack
    }
    
    public class ShoppingList
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
        public bool IsCompleted { get; set; }
        
        public List<ShoppingListItem> Items { get; set; } = new();
    }
    
    public class ShoppingListItem
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Category { get; set; } // Produce, Dairy, Meat, etc.
        public int Quantity { get; set; } = 1;
        public string? Unit { get; set; } // lbs, oz, cups, etc.
        public bool IsChecked { get; set; }
        
        public int ShoppingListId { get; set; }
        public ShoppingList ShoppingList { get; set; } = null!;
    }
}