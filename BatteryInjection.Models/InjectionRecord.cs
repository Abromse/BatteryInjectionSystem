using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace BatteryInjection.Models
{
    [SugarTable("injection_record")]
    public class InjectionRecord
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        [SugarColumn(Length =100, IsNullable = false)]
        public string Barcode { get; set; } = "";
        public decimal BeforeWeight { get; set; }
        public decimal AfterWeight { get; set; }
        public decimal InjectionAmount { get; set; }
        public bool IsPass { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
