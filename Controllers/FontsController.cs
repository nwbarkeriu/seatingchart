using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class FontsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var fontsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fonts");
        if (!Directory.Exists(fontsDir))
            return Ok(new List<object>());

        var fontFiles = Directory.GetFiles(fontsDir)
            .Select(f => new
            {
                Name = Path.GetFileNameWithoutExtension(f),
                File = Path.GetFileName(f)
            }).ToList();

        return Ok(fontFiles);
    }
}