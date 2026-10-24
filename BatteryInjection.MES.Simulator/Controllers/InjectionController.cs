using BatteryInjection.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BatteryInjection.MES.Simulator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InjectionController : Controller
    {
        private static List<InjectionRecord> _fakeDatabase = new();
        [HttpPost("report")]
        public IActionResult Report([FromBody] InjectionRecord record)
        {
            if (record == null)
                return BadRequest("没有收到数据");
            // 模拟存入数据库
            _fakeDatabase.Add(record);

            // 在 VS 的“输出”窗口打印出来，证明它真的收到了
            Console.WriteLine("========================================");
            Console.WriteLine($"【MES收到上报】条码：{record.Barcode}");
            Console.WriteLine($"【MES收到上报】注液量：{record.InjectionAmount}");
            Console.WriteLine($"【MES收到上报】是否合格：{record.IsPass}");
            Console.WriteLine("========================================");
            return Ok("MES数据接收成功");
        }
    }
}
