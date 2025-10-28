using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using SeatingChartApp.Data;
using SeatingChartApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<TickerService>();
        builder.Services.AddScoped<SchoolEventsService>();
        builder.Services.AddControllers();
        
        // Add database contexts
        builder.Services.AddDbContext<LineupDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("LineupConnection") ?? 
                            "Data Source=lineup.db"));
        // Uncomment the following line if you have a WeatherForecastService
        // builder.Services.AddSingleton<WeatherForecastService>();
        var app = builder.Build();
        
        // Ensure lineup database is created
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LineupDbContext>();
            context.Database.EnsureCreated();
        }
        
        app.MapControllers();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapBlazorHub();
        app.MapRazorPages();
        app.MapFallbackToPage("/_Host");
        app.Run();
    }
}
