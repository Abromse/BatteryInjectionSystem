using SqlSugar;
using BatteryInjection.Models;
namespace BatteryInjection.DataAccess
{
    public class DbHelper
    {
        private readonly SqlSugarClient _dbClient;
        public DbHelper(string dbType = "Sqlite")
        {
            var config = new ConnectionConfig()
            {
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            };

            if (dbType == "Sqlite")
            {
                config.DbType = DbType.Sqlite;
                config.ConnectionString = "Data Source=BatteryInjection.db";
            }
            else if (dbType == "MySql")
            {
                // 注意：这里如果写真实MySQL，需要装 MySql.Data 包。
                // 但我们只是在面试时证明我们懂，所以写在这里备用即可。
                config.DbType = DbType.MySql;
                config.ConnectionString = "Server=localhost;Database=Battery;Uid=root;Pwd=123456;";
            }
            _dbClient = new SqlSugarClient(config);
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
