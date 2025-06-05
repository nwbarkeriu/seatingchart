using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace SeatingChartApp.Models
{
    public class Table
    {
        public int TableNumber { get; set; }
        public List<Guest> Guests { get; set; } = new List<Guest>();
    }
}
