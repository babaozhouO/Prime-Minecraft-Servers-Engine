using ProtoBuf;

namespace PMCSsE_Communicator
{
    /// <summary>
    /// 多个 MC 服务端管理器数据的聚合容器，用于一次性传输所有管理器的状态信息。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class MCServerManagersData
    {
        /// <summary>
        /// MC 服务端管理器数据列表。
        /// </summary>
        [ProtoMember(1)]
        public List<MCServerManagerData> MCServerManagerDataList = [];
    }
    /// <summary>
    /// 单个 MC 服务端管理器的运行时状态数据，包含管理器的唯一标识和运行状态。
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class MCServerManagerData
    {
        /// <summary>
        /// 管理器的唯一标识符。
        /// </summary>
        [ProtoMember(1)]
        public required string ManagerID;
        /// <summary>
        /// 指示该管理器下的 MC 服务端是否正在运行。
        /// </summary>
        [ProtoMember(2)]
        public required bool IsMCServerRunning;
    }

    /// <summary>
    /// MC服务端配置类,包含多个MC服务端管理实例的配置和版本号
    /// </summary>
    [ProtoContract]
    public class MCServerManagerConfigs
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [ProtoMember(1)]
        public int ConfigVersion { get; set; } = 1;
        /// <summary>
        /// MC服务端配置列表,包含多个MC服务端管理实例的配置
        /// </summary>
        [ProtoMember(2)]
        public List<MCServerManagerConfig> MCServerManagerConfigsList { get; set; } = [];
    }
    /// <summary>
    ///   静态  MC服务端配置类,包含多个MC服务端管理实例的配置和版本号
    /// </summary>
    public static class StaticMCServerManagerConfigs
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public static int ConfigVersion { get; set; }
        /// <summary>
        ///   静态  MC服务端配置列表,包含多个MC服务端管理实例的配置
        /// </summary>
        public static List<MCServerManagerConfig> MCServerManagerConfigsList { get; set; } = [];
    }
    /// <summary>
    /// 单个MC服务端管理实例的配置结构
    /// </summary>
    [ProtoContract]
    public class MCServerManagerConfig
    {
        /// <summary>
        /// MC服务端管理实例ID
        /// </summary>
        [ProtoMember(1)]
        public string ManagerID { get; set; } = "";
        /// <summary>
        /// MC服务器名称
        /// </summary>
        [ProtoMember(2)]
        public string MCServerName { get; set; } = "";
        /// <summary>
        /// MC服务端类型
        /// </summary>
        [ProtoMember(3)]
        public string MCServerType { get; set; } = "Vanilla";
        /// <summary>
        /// MC服务端目录
        /// </summary>
        [ProtoMember(4)]
        public string MCServerDirectory { get; set; } = "";
        /// <summary>
        /// Java路径
        /// </summary>
        [ProtoMember(5)]
        public string JavaPath { get; set; } = "";
        /// <summary>
        /// 启动参数
        /// </summary>
        [ProtoMember(6)]
        public string StartUpArguments { get; set; } = "";
        /// <summary>
        /// 备份工具配置
        /// </summary>
        [ProtoMember(7)]
        public BackupManagerConfig BackupManagerConfig { get; set; } = new();
        /// <summary>
        /// 在线聊天系统配置
        /// </summary>
        [ProtoMember(8)]
        public OnlineChattingSystemConfig OnlineChattingSystemConfig { get; set; } = new();

    }
    /// <summary>
    /// 备份工具配置
    /// </summary>
    [ProtoContract]
    public class BackupManagerConfig
    {
        //-----------------------------------------------备份器配置-------------------------------//
        /// <summary>
        /// 是否启用自动备份
        /// </summary>
        [ProtoMember(1)]
        public bool AutoBackupEnabled { get; set; } = false;
        /// <summary>
        /// 备份工具运行模式：间隔天数后当天特定时间运行模式/固定时间间隔运行模式
        /// </summary>
        [ProtoMember(2)]
        public BackupTimingMode BackupTimingMode { get; set; } = BackupTimingMode.DayInterval_SpecificTime;
        /// <summary>
        /// 间隔天数（间隔天数后当天特定时间运行模式）
        /// </summary>
        [ProtoMember(3)]
        public string DayInterval { get; set; } = "1";
        /// <summary>
        /// 间隔天数后的当天特定时间（间隔天数后当天特定时间运行模式）
        /// </summary>
        [ProtoMember(4)]
        public string SpecificTime { get; set; } = "04:00:00";
        /// <summary>
        /// 时间间隔（固定时间间隔运行模式）
        /// </summary>
        [ProtoMember(5)]
        public string TimeInterval { get; set; } = "04:00:00";
        /// <summary>
        /// 备份前关服
        /// </summary>
        [ProtoMember(6)]
        public bool StopServerBeforeBackup { get; set; } = false;
        /// <summary>
        /// 备份模式
        /// </summary>
        [ProtoMember(7)]
        public BackupMode BackupMode { get; set; } = BackupMode.Full;
        /// <summary>
        /// 压缩等级 0~9
        /// </summary>
        [ProtoMember(8)]
        public string CompactionLevel { get; set; } = "0";
        /// <summary>
        /// 排除文件列表
        /// </summary>
        [ProtoMember(9)]
        public List<string> ExcludedFilesList { get; set; } = [];
        /// <summary>
        /// 排除文件拓展名列表
        /// </summary>
        [ProtoMember(10)]
        public List<string> ExcludedFileExtensionsList { get; set; } = [];
        /// <summary>
        /// 排除文件夹列表
        /// </summary>
        [ProtoMember(11)]
        public List<string> ExcludedFoldersList { get; set; } = [];
        /// <summary>
        /// 备份输出目录
        /// </summary>
        [ProtoMember(12)]
        public string BackupFileOutputDirectory { get; set; } = "";
        /// <summary>
        /// 远程备份存放目录
        /// </summary>
        [ProtoMember(13)]
        public string RemoteBackupFileStoreDirectory { get; set; } = "/";
        /// <summary>
        /// SFTP客户端
        /// </summary>
        [ProtoMember(14)]
        public SFTPClientConfig SFTPClientConfig { get; set; } = new();
    }
    /// <summary>
    /// 备份模式
    /// </summary>
    public enum BackupMode
    {
        /// <summary>
        /// 全量备份
        /// </summary>
        Full,
        /// <summary>
        /// 文件级增量备份
        /// </summary>
        FileLevel_Incremental,
        /// <summary>
        /// 块级增量备份
        /// </summary>
        BlockLevel_Incremental
    }
    /// <summary>
    /// 备份工具运行模式
    /// </summary>
    public enum BackupTimingMode
    {
        /// <summary>
        /// 间隔天数后当天特定时间运行模式
        /// </summary>
        DayInterval_SpecificTime,
        /// <summary>
        /// 固定时间间隔运行模式
        /// </summary>
        FixedTimeInterval
    }
    /// <summary>
    /// SFTP客户端配置
    /// </summary>
    [ProtoContract]
    public class SFTPClientConfig
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        [ProtoMember(1)]
        public bool Enabled { get; set; } = false;
        /// <summary>
        /// 主机地址
        /// </summary>
        [ProtoMember(2)]
        public string Host { get; set; } = "127.0.0.1";
        /// <summary>
        /// 端口
        /// </summary>
        [ProtoMember(3)]
        public int Port { get; set; } = 22;
        /// <summary>
        /// 用户名
        /// </summary>
        [ProtoMember(4)]
        public string UserName { get; set; } = "";
        /// <summary>
        /// 密码
        /// </summary>
        [ProtoMember(5)]
        public string Password { get; set; } = "";
        /// <summary>
        /// 缓冲区大小(MiB)
        /// </summary>
        [ProtoMember(6)]
        public int BufferSize { get; set; } = 1;
    }
    /// <summary>
    /// 在线聊天系统配置
    /// </summary>
    [ProtoContract]
    public class OnlineChattingSystemConfig
    {
        /// <summary>
        /// 监听连接请求的端口
        /// </summary>
        [ProtoMember(1)]
        public string ServerPort { get; set; } = "8080";
        /// <summary>
        /// 第三方社交平台名称
        /// </summary>
        [ProtoMember(2)]
        public string ThirdPartySocialPlatformName { get; set; } = "";
        /// <summary>
        /// 玩家账户列表
        /// </summary>
        [ProtoMember(3)]
        public List<PlayerAccount> PlayerAccountList { get; set; } = [];
    }
    /// <summary>
    /// 玩家账户类
    /// </summary>
    [ProtoContract]
    public class PlayerAccount
    {
        /// <summary>
        /// 是否已审核通过
        /// </summary>
        [ProtoMember(1)]
        public bool Approved { get; set; } = false;
        /// <summary>
        /// 玩家身份
        /// </summary>
        [ProtoMember(2)]
        public string PlayerRole { get; set; } = "玩家";
        /// <summary>
        /// 玩家名称
        /// </summary>
        [ProtoMember(3)]
        public string PlayerName { get; set; } = "";
        /// <summary>
        /// 密码哈希值
        /// </summary>
        [ProtoMember(4)]
        public string PasswordHash { get; set; } = "";
        /// <summary>
        /// 第三方社交平台账号
        /// </summary>
        [ProtoMember(5)]
        public string ThirdPartySocialPlatformAccount { get; set; } = "";
    }
}

