namespace SeatingChartApp.Models
{
    public class Table
    {
        public int TableNumber { get; set; }
        public List<Guest> Guests { get; set; } = new List<Guest>();
    }
}
