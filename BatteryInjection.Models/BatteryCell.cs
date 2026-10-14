using SqlSugar;
namespace BatteryInjection.Models
{
    [SugarTable("battery_cell")]
    public class BatteryCell
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        [SugarColumn(Length = 100, IsNullable = false)]
        public string Barcode { get; set; } = "";
        [SugarColumn(Length = 50)]
        public string Model { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
