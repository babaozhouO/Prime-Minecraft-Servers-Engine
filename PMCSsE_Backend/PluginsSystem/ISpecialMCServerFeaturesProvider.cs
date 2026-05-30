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
        public IReadOnlyList<IAsyncSpecialMCServerFeature> Features { get; }
    }
    public interface IAsyncSpecialMCServerFeature
    {
        public string FeatureName { get; }
        public string FeatureDescription { get; }
        public Task AsyncFeature(MCServerManager mCServerManager);
    }
}
