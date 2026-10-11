namespace BatteryInjection.Models
{
    public class BatteryCell
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = "";
        public string Model { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
