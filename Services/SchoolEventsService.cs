using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ical.Net;

namespace SeatingChartApp.Services
{
    public class SchoolEventsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SchoolEventsService> _logger;
        private List<SchoolEvent> _cachedEvents = new();
        private DateTime _lastCacheUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(4); // Cache for 4 hours
        
        public SchoolEventsService(HttpClient httpClient, ILogger<SchoolEventsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<SchoolEvent>> GetSchoolEventsAsync()
        {
            // Check if cache is still valid
            if (DateTime.Now - _lastCacheUpdate < _cacheExpiry && _cachedEvents.Any())
            {
                return _cachedEvents;
            }

            try
            {
                await RefreshEventsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh school events from API");
                // Return cached events if available, even if stale
                if (_cachedEvents.Any())
                {
                    _logger.LogWarning("Using stale cached events due to API failure");
                    return _cachedEvents;
                }
            }

            return _cachedEvents;
        }

        private async Task RefreshEventsAsync()
        {
            var icsUrl = "https://www.CCS.k12.in.us/fs/calendar-manager/events.ics?calendar_ids[]=171&calendar_ids[]=3";
            
            try
            {
                _logger.LogInformation("Fetching school events from CCS calendar");
                var icsData = await _httpClient.GetStringAsync(icsUrl);
                var calendar = Calendar.Load(icsData);

                _cachedEvents = calendar.Events
                    .Where(e => e.Start?.AsSystemLocal != null)
                    .Select(e => new SchoolEvent
                    {
                        Title = e.Summary ?? "School Event",
                        StartDate = e.Start.AsSystemLocal,
                        EndDate = e.End?.AsSystemLocal ?? e.Start.AsSystemLocal,
                        Description = e.Description ?? "",
                        Location = e.Location ?? ""
                    })
                    .OrderBy(e => e.StartDate)
                    .ToList();

                _lastCacheUpdate = DateTime.Now;
                _logger.LogInformation($"Successfully loaded {_cachedEvents.Count} school events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch or parse school events from {Url}", icsUrl);
                throw;
            }
        }

        public async Task<List<SchoolEvent>> GetUpcomingEventsAsync(int maxEvents = 10)
        {
            var allEvents = await GetSchoolEventsAsync();
            return allEvents
                .Where(e => e.StartDate > DateTime.Now)
                .Take(maxEvents)
                .ToList();
        }

        public async Task<List<SchoolEvent>> GetEventsForDateAsync(DateTime date)
        {
            var allEvents = await GetSchoolEventsAsync();
            return allEvents
                .Where(e => e.StartDate.Date <= date.Date && e.EndDate.Date >= date.Date)
                .ToList();
        }

        public async Task<bool> IsSchoolDayAsync(DateTime date)
        {
            var events = await GetEventsForDateAsync(date);
            
            // Check if it's a weekend
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                return false;

            // Check for no-school events
            foreach (var evt in events)
            {
                var title = evt.Title.ToLower();
                if (title.Contains("no students") || 
                    title.Contains("break") || 
                    title.Contains("holiday") ||
                    title.Contains("teacher work day") ||
                    title.Contains("no school") ||
                    title.Contains("last student day"))
                {
                    return false;
                }
            }

            // Check if it's within the school year (approximate dates)
            var schoolYearStart = new DateTime(date.Year, 8, 1);
            var schoolYearEnd = new DateTime(date.Year + 1, 5, 31);
            
            // Adjust for year boundary
            if (date.Month >= 8)
            {
                schoolYearEnd = new DateTime(date.Year + 1, 5, 31);
            }
            else
            {
                schoolYearStart = new DateTime(date.Year - 1, 8, 1);
                schoolYearEnd = new DateTime(date.Year, 5, 31);
            }

            return date >= schoolYearStart && date <= schoolYearEnd;
        }

        public async Task<SchoolEvent?> GetEventForDateAsync(DateTime date)
        {
            var events = await GetEventsForDateAsync(date);
            return events.FirstOrDefault();
        }

        public async Task<List<string>> GetFormattedUpcomingEventsAsync(int maxEvents = 3)
        {
            var events = await GetUpcomingEventsAsync(maxEvents);
            var results = new List<string>();

            foreach (var evt in events)
            {
                var eventStart = evt.StartDate.ToString("MMM dd");
                var eventEnd = evt.EndDate.ToString("MMM dd");
                var eventDateDisplay = eventStart;
                
                if (eventEnd != eventStart)
                {
                    eventDateDisplay += $" - {eventEnd}";
                }

                results.Add($"<div style='display: flex; align-items: center; height: 100%; max-height: 80px; width: auto;'> {evt.Title} <br>{eventDateDisplay}</div>");
            }

            return results;
        }

        // Force refresh cache (useful for manual refresh)
        public async Task ForceRefreshAsync()
        {
            _lastCacheUpdate = DateTime.MinValue;
            await GetSchoolEventsAsync();
        }
    }

    public class SchoolEvent
    {
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        
        public bool IsToday => StartDate.Date <= DateTime.Today && EndDate.Date >= DateTime.Today;
        public bool IsUpcoming => StartDate > DateTime.Now;
        public int DaysAway => (StartDate.Date - DateTime.Today).Days;
        public bool IsMultiDay => StartDate.Date != EndDate.Date;
    }
}