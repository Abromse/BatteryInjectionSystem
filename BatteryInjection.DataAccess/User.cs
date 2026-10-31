using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryInjection.DataAccess
{
    [SugarTable("sys_user")]
   public class User()
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? HassPassword { get; set; }
        public string? Role { get; set; }
    }
}
