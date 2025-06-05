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
public class LunchController : ControllerBase
{
    private static readonly List<List<string>> SummerLunches = new()
    {
        new() { "Quesadilla, apples, yogurt chips" },
        new() { "Ham/salami, cantaloupe, chips, baby carrots" },
        new() { "Chicken sticks, strawberries, peppers, chips" },
        new() { "Pizza, oranges/banana, GoGurt" },
        new() { "Kids pick" },
        new() { "Lunchable, blueberries, grapes, cucumbers, chips" },
        new() { "Peanut butter sandwich, apples, cheese sticks, chips" },
        new() { "Fish sticks, pineapple, carrots, chips" },
        new() { "Ham/salami sandwich, oranges, peppers, chips" },
        new() { "Kids pick" },
        new() { "Chicken sticks, blueberries, cucumbers, chips" },
        new() { "Pizza, oranges, banana, yogurt" },
        new() { "Quesadilla, apples, yogurt chips" },
        new() { "Lunchable, blueberry, grapes, chips" },
        new() { "Kids pick" }
    };

    private static readonly List<List<string>> SchoolLunches = new()
    {
        new() { "Chicken Patty Sandwich, Popcorn Chicken Salad, Honey Glazed Carrots" },
        new() { "Nacho Supreme, Taco Salad, Refried Beans" },
        new() { "Bosco Sticks, Yogurt Parfait with Homemade Granola, Steamed Broccoli" },
        new() { "Pasta with Assorted Sauces & Garlic Breadstick, Taco Salad, Green Beans" },
        new() { "Chicken Nuggets with Blueberry Muffin, Popcorn Chicken Salad, Baked Apples" },
        new() { "Soft Pretzel with Cheese, Popcorn Chicken Salad, Green Beans" },
        new() { "Hot Dog, Taco Salad, Baked Beans" },
        new() { "Personal Pan Pizza, Yogurt Parfait with Homemade Granola, Steamed Broccoli" },
        new() { "French Toast Sticks with Sausage Links, Taco Salad, Baked Blueberry Cobbler" },
        new() { "Chicken Smackers with Dinner Roll, Popcorn Chicken Salad, Mashed Potatoes with Gravy" },
        new() { "Orange Chicken Rice Bowl, Popcorn Chicken Salad, Steamed Broccoli" },
        new() { "Mini Corn Dogs, Taco Salad, Smiley Potatoes" },
        new() { "Cheeseburger, Yogurt Parfait with Homemade Granola, Baked Beans" },
        new() { "Pizza Crunchers with Dip, Taco Salad, Green Beans" },
        new() { "Chicken Tenders with Biscuit, Popcorn Chicken Salad, Buttered Corn" }
    };

    private static readonly Dictionary<string, string> Events = new()
    {
        ["2025-06-03"] = "Test Event NB",
        ["2025-06-04"] = "Test Event NB",
        ["2025-08-04"] = "Teacher Work Day (No Students)",
        ["2025-08-05"] = "Teacher Work Day (No Students)",
        ["2025-08-06"] = "First Student Day",
        ["2025-09-01"] = "Labor Day Holiday",
        ["2025-10-06"] = "Fall Break",
        ["2025-10-07"] = "Fall Break",
        ["2025-10-08"] = "Fall Break",
        ["2025-10-09"] = "Fall Break",
        ["2025-10-10"] = "Fall Break",
        ["2025-11-06"] = "Elem. Parent/Teacher Conf. (Elem. Students Dismissed Half-Day)",
        ["2025-11-07"] = "Elem. Parent/Teacher Conf. (No School for Elem. Students)",
        ["2025-11-26"] = "Thanksgiving Break",
        ["2025-11-27"] = "Thanksgiving Break",
        ["2025-11-28"] = "Thanksgiving Break",
        ["2025-12-19"] = "Teacher Work Day (No Students)",
        ["2025-12-22"] = "Winter Break",
        ["2025-12-23"] = "Winter Break",
        ["2025-12-24"] = "Winter Break",
        ["2025-12-25"] = "Winter Break",
        ["2025-12-26"] = "Winter Break",
        ["2025-12-29"] = "Winter Break",
        ["2025-12-30"] = "Winter Break",
        ["2025-12-31"] = "Winter Break",
        ["2026-01-01"] = "Winter Break",
        ["2026-01-02"] = "Winter Break",
        ["2026-01-05"] = "Teacher Work Day (No Students)",
        ["2026-01-19"] = "Martin Luther King, Jr. Holiday",
        ["2026-02-16"] = "Presidents’ Day Holiday",
        ["2026-04-06"] = "Spring Break",
        ["2026-04-07"] = "Spring Break",
        ["2026-04-08"] = "Spring Break",
        ["2026-04-09"] = "Spring Break",
        ["2026-04-10"] = "Spring Break",
        ["2026-05-22"] = "Last Student Day",
        ["2026-05-25"] = "Memorial Day (Schools & Offices Closed)",
        ["2026-05-26"] = "Teacher Work Day (No Students)"
    };

    [HttpGet]
    public IActionResult GetLunch()
    {
        DateTime today = DateTime.Now;

        if (today.Hour >= 14)
            today = today.AddDays(1);

        var isSchool = today >= new DateTime(2025, 8, 6) && today <= new DateTime(2026, 5, 22);
        var start = isSchool ? new DateTime(2025, 8, 6) : new DateTime(2025, 5, 23);

        int weekdaysPassed = Enumerable.Range(0, (today - start).Days + 1)
            .Select(i => start.AddDays(i))
            .Count(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) - 1;

        int index = weekdaysPassed % 15;
        var menu = isSchool ? SchoolLunches[index] : SummerLunches[index];

        var todayStr = today.ToString("yyyy-MM-dd");
        var response = new
        {
            date = todayStr,
            menu,
            eventMessage = Events.ContainsKey(todayStr) ? Events[todayStr] : null
        };

        return Ok(response);
    }
}
