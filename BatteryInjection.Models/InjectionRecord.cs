using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryInjection.Models
{
    public class InjectionRecord
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = "";
        public decimal BeforeWeight { get; set; }
        public decimal AfterWeight { get; set; }
        public decimal InjectionAmount { get; set; }
        public bool IsPass { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
