using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
public class DinnerController : ControllerBase
{
    // Barker Family Dinner Menu - Real meals with smart rotation logic
    private static readonly List<FamilyMeal> BarkerFamilyMeals = new()
    {
        // EASY MEALS - Perfect for busy sports nights
        new FamilyMeal { Name = "Order in Pizza", PrepType = "Easy", CookTime = 0, Protein = "None" },
        new FamilyMeal { Name = "Order In", PrepType = "Easy", CookTime = 0, Protein = "None" },
        new FamilyMeal { Name = "Deli Sandwiches & Chips", PrepType = "Easy", CookTime = 15, Protein = "Other" },
        new FamilyMeal { Name = "Leftovers", PrepType = "Easy", CookTime = 5, Protein = "Varies" },
        
        // CROCKPOT MEALS - Great for busy days, set and forget
        new FamilyMeal { Name = "Crockpot BBQ Chicken & Chips", PrepType = "Crockpot", CookTime = 480, Protein = "Chicken" },
        new FamilyMeal { Name = "Crockpot Meal", PrepType = "Crockpot", CookTime = 480, Protein = "Varies" },
        new FamilyMeal { Name = "Roast, Carrots & Potatoes", PrepType = "Crockpot", CookTime = 480, Protein = "Beef" },
        new FamilyMeal { Name = "Pulled Pork & Mac N Cheese", PrepType = "Crockpot", CookTime = 480, Protein = "Pork" },
        
        // CHICKEN MEALS - Family favorite protein
        new FamilyMeal { Name = "Chicken Skewers, Grilled Veggies & Garlic Bread", PrepType = "Grill", CookTime = 45, Protein = "Chicken" },
        new FamilyMeal { Name = "Chicken Tacos & Rice", PrepType = "Stovetop", CookTime = 30, Protein = "Chicken" },
        new FamilyMeal { Name = "Baked Chicken & Mashed Potatoes", PrepType = "Oven", CookTime = 90, Protein = "Chicken" },
        new FamilyMeal { Name = "Baked Chicken, Fries & Veggies", PrepType = "Oven", CookTime = 75, Protein = "Chicken" },
        new FamilyMeal { Name = "Baked Chicken, Pasta & Garlic Bread", PrepType = "Oven", CookTime = 60, Protein = "Chicken" },
        new FamilyMeal { Name = "Chicken Enchiladas & Rice", PrepType = "Oven", CookTime = 60, Protein = "Chicken" },
        
        // BEEF MEALS - Hearty family dinners
        new FamilyMeal { Name = "Grilled Hamburgers & Fries", PrepType = "Grill", CookTime = 30, Protein = "Beef" },
        new FamilyMeal { Name = "Beef Tacos & Rice", PrepType = "Stovetop", CookTime = 30, Protein = "Beef" },
        new FamilyMeal { Name = "Meatballs & Potato Casserole", PrepType = "Oven", CookTime = 75, Protein = "Beef" },
        new FamilyMeal { Name = "Country Fried Steak, Mashed Potatoes & Corn", PrepType = "Stovetop", CookTime = 45, Protein = "Beef" },
        
        // PORK MEALS - Comfort food favorites
        new FamilyMeal { Name = "Pork Chops, Pasta & Garlic Bread", PrepType = "Stovetop", CookTime = 45, Protein = "Pork" },
        new FamilyMeal { Name = "Smoked Sausage, Fried Potatoes & Mac N Cheese", PrepType = "Stovetop", CookTime = 30, Protein = "Pork" },
        
        // OTHER PROTEINS & PASTA
        new FamilyMeal { Name = "Spaghetti & Garlic Bread", PrepType = "Stovetop", CookTime = 30, Protein = "None" }
    };

    [HttpGet]
    public IActionResult GetDinner()
    {
        // Get current time in ET (adjust as needed for your timezone)
        DateTime now = DateTime.Now;
        
        // Switch to next day's meal at 10 PM ET
        DateTime targetDate = now.Hour >= 22 ? now.Date.AddDays(1) : now.Date;
        
        // Generate dinner for the target date using intelligent logic
        var dinner = GenerateSmartDinnerForDate(targetDate);
        
        var response = new
        {
            date = targetDate.ToString("yyyy-MM-dd"),
            dinner = dinner.Name,
            cookTime = dinner.CookTime,
            prepType = dinner.PrepType
        };

        return Ok(response);
    }

    private static FamilyMeal GenerateSmartDinnerForDate(DateTime date)
    {
        var meals = BarkerFamilyMeals.ToList();
        var random = new Random(date.DayOfYear + date.Year); // Consistent randomization per date
        
        // SPORTS NIGHT LOGIC - Tuesday and Thursday prefer easier meals
        bool isSportsNight = date.DayOfWeek == DayOfWeek.Tuesday || date.DayOfWeek == DayOfWeek.Thursday;
        
        var availableMeals = meals.ToList();
        
        if (isSportsNight)
        {
            // 70% chance of easy meal on sports nights
            var easyMeals = availableMeals.Where(m => 
                m.PrepType == "Easy" || 
                m.PrepType == "Crockpot" ||
                m.CookTime <= 30).ToList();
                
            if (easyMeals.Any() && random.Next(100) < 70)
            {
                availableMeals = easyMeals;
            }
        }
        
        // PROTEIN ROTATION LOGIC - Check previous days to avoid same protein 3+ nights
        string lastProtein = GetLastProteinUsed(date.AddDays(-1));
        string secondLastProtein = GetLastProteinUsed(date.AddDays(-2));
        
        if (lastProtein == secondLastProtein && !string.IsNullOrEmpty(lastProtein))
        {
            // Avoid third consecutive night of same protein
            availableMeals = availableMeals.Where(m => m.Protein != lastProtein).ToList();
        }
        
        // WEEKEND SPECIAL LOGIC - More likely to order in or grill
        if (date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday)
        {
            var weekendMeals = meals.Where(m => 
                m.Name.Contains("Pizza") || 
                m.Name.Contains("Order") ||
                m.PrepType == "Grill").ToList();
                
            if (weekendMeals.Any() && random.Next(100) < 50) // 50% chance weekend special
            {
                availableMeals = weekendMeals.Intersect(availableMeals).ToList();
            }
        }
        
        // If we filtered too much, reset to all meals
        if (!availableMeals.Any())
        {
            availableMeals = meals.ToList();
        }
        
        // SELECT RANDOM MEAL from available options
        return availableMeals[random.Next(availableMeals.Count)];
    }
    
    private static string GetLastProteinUsed(DateTime date)
    {
        // This would typically check stored meal history
        // For now, we'll use a simple deterministic approach based on date
        var lastMeal = GenerateBasicMealForDate(date);
        return lastMeal.Protein;
    }
    
    private static FamilyMeal GenerateBasicMealForDate(DateTime date)
    {
        var random = new Random(date.DayOfYear + date.Year);
        return BarkerFamilyMeals[random.Next(BarkerFamilyMeals.Count)];
    }

    public class FamilyMeal
    {
        public string Name { get; set; } = string.Empty;
        public string PrepType { get; set; } = string.Empty; // Easy, Crockpot, Grill, Oven, Stovetop
        public int CookTime { get; set; } // Minutes
        public string Protein { get; set; } = string.Empty; // Chicken, Beef, Pork, None, Other, Varies
    }
}