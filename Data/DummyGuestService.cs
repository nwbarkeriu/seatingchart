using SeatingChartApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SeatingChartApp.Data
{
    public static class DummyGuestService
    {
        public static List<Guest> Guests { get; set; } = new List<Guest>();
    }
}
