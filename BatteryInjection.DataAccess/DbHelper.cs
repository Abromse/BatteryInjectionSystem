using SqlSugar;
using BatteryInjection.Models;
namespace BatteryInjection.DataAccess
{
    public class DbHelper
    {
        private readonly SqlSugarClient _dbClient;
        public DbHelper()
        {
            _dbClient = new SqlSugarClient(new ConnectionConfig()
            {
                ConnectionString = "Data Source=BatteryInjection.db",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
            //排查问题，打印SQL
            _dbClient.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine($"SQL: {sql}");
            };
        }
        /// <summary>
        /// 初始化数据库，创建表
        /// </summary>
        public void InitDatabase() 
        {
            _dbClient.CodeFirst.InitTables<BatteryCell, InjectionRecord >();   
        }
        /// <summary>
        /// 插入一条电芯数据
        /// </summary>
        public int InsertCell(BatteryCell cell)
        {
            return _dbClient.Insertable(cell).ExecuteCommand();
        }
        /// <summary>
        /// 查询所有电芯
        /// </summary>
        public List<BatteryCell> GetAllCells()
        {
            return _dbClient.Queryable<BatteryCell>().ToList();
        }
        /// <summary>
        /// 插入一条注液记录
        /// </summary>
        public int InsertInjectionRecord(InjectionRecord record)
        {
            return _dbClient.Insertable(record).ExecuteCommand();
        }
        /// <summary>
        /// 查询所有注液记录
        /// </summary>
        public List<InjectionRecord> GetAllInjectionRecords()
        {
            return _dbClient.Queryable<InjectionRecord>().ToList();
        }
    }
}
