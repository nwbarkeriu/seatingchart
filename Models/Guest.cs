using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace SeatingChartApp.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsManuallyAssigned { get; set; } = false;
        public Dictionary<int, int?> TableAssignments { get; set; } = new Dictionary<int, int?>();
    }
}
