using PMCSsE_Backend.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMCSsE_Backend.PluginsSystem
{
    /// <summary>
    /// 为特定服务端种类提供特定功能
    /// </summary>
    public interface ISpecialMCServerFeaturesProvider
    {
        /// <summary>
        /// 服务端种类
        /// </summary>
        public string TargetMCServerType { get; }
        /// <summary>
        /// 功能列表，编译时固定，包含该服务端种类的所有特殊功能定义。
        /// </summary>
        public IAsyncSpecialMCServerFeature[] Features { get; }
    }
    /// <summary>
    /// 异步特殊功能定义接口。为特定服务端种类提供可在服务端运行时调用的异步功能。
    /// </summary>
    public interface IAsyncSpecialMCServerFeature
    {
        /// <summary>
        /// 功能名称。
        /// </summary>
        public string FeatureName { get; }
        /// <summary>
        /// 功能描述文本。
        /// </summary>
        public string FeatureDescription { get; }
        /// <summary>
        /// 异步执行该功能。接收目标 MCServerManager 作为上下文参数。
        /// </summary>
        /// <param name="mCServerManager">目标 MC 服务端管理器实例。</param>
        public Task AsyncFeature(MCServerManager mCServerManager);
    }
}
