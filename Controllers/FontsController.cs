using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class FontsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetFonts()
    {
        var fontsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts", "fonts.json");
        if (!System.IO.File.Exists(fontsPath))
            return NotFound();

        var json = System.IO.File.ReadAllText(fontsPath);
        return Content(json, "application/json");
    }
}