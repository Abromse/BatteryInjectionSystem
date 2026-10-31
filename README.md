# 新能源电池注液工序上位机系统

本项目是基于 C# 和 .NET 8 开发的一款工业级 WPF 上位机软件，主要用于新能源电池自动化产线。系统实现了底层的设备通信、数据采集、本地数据持久化存储，以及与企业 MES 系统的数据对接。

## 主要功能

- 安全登录与权限控制：系统启动时强制拦截登录。密码采用 BCrypt 算法进行哈希加密存储，不保存明文。支持管理员与普通用户角色切换，普通用户无法执行删除敏感数据等操作。支持注销当前账号并平滑返回登录界面。
- 多协议设备通信：使用 NModbus 库实现与产线 PLC 的 Modbus TCP 通信，进行寄存器读写；封装自定义 TCP Socket 客户端，实时接收称重仪推送的数据；通过 UDP 监听接收车间设备的心跳广播。
- 数据持久化与上报：本地使用 SqlSugar 配合 SQLite 数据库，自动建表并存储生产记录和用户信息。通过 HttpClient 将注液数据以 JSON 格式上报给后端的 WebAPI（模拟 MES 系统）。
- 界面交互与工业防呆：基于 MaterialDesign 美化界面。通过 IValueConverter 实现状态灯的红绿预警。配置防抖机制防止按钮高频连点。跨线程更新 UI 时使用 Dispatcher 保证界面不卡死。
- 系统稳定性保障：加入了全局异常捕获机制防闪退，内置本地文件日志并按大小自动滚动切分以防撑爆硬盘。IP 和端口等核心参数已抽离至 config.json 配置文件，方便现场部署。

## 技术栈

- 开发语言：C# / .NET 8
- 界面框架：WPF / MVVM (CommunityToolkit.Mvvm)
- 数据库与 ORM：SQLite / SqlSugarCore
- 通信协议：Modbus TCP / 自定义 TCP / UDP / HTTP (RESTful)
- 安全与日志：BCrypt.Net / 自定义文件日志

## 项目结构

- BatteryInjection.UI：WPF 客户端主界面、登录窗口以及 ViewModel。
- BatteryInjection.Device：设备驱动层，封装 PLC、称重仪和 UDP 通信。
- BatteryInjection.DataAccess：数据访问层，包含 SqlSugar 的封装与数据库操作。
- BatteryInjection.MES.Client：调用 MES 接口的客户端封装。
- BatteryInjection.MES.Simulator：基于 ASP.NET Core 编写的模拟 MES 服务端。
- BatteryInjection.Models：数据实体模型。
- BatteryInjection.Common：公共工具类，包含配置读取和日志记录。

## 运行说明

1. 环境：安装 Visual Studio 2022 和 .NET 8 SDK。
2. 配置：修改 BatteryInjection.UI 目录下的 config.json，填入正确的 PLC IP、端口和 MES 接口 URL。
3. 启动服务端：运行 BatteryInjection.MES.Simulator 模拟 MES 接口。
4. 启动客户端：运行 BatteryInjection.UI。默认登录账号为 admin，密码为 12345。
