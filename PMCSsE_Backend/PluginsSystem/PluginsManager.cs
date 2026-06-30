using PMCSsE_Backend.Modules;
using PMCSsE_Communicator.SharedCodes;
using System.Reflection;

namespace PMCSsE_Backend.PluginsSystem
{
    internal static class PluginsManager
    {
        private static readonly List<PluginLoader> Plugins = [];
        internal static readonly List<ISpecialMCServerFeaturesProvider> SpecialMCServerFeaturesProviders = [];
        private static readonly Lock Lock = new();
        internal static void LoadAllPlugins()
        {
            if (Directory.Exists(Paths.PluginsDir))
            {
                string[]? pluginFiles;
                try
                {
                    pluginFiles = Directory.GetFiles(Paths.PluginsDir, "*.dll");
                    StaticTools.HandleLog($"找到[{pluginFiles.Length}]个插件：{Environment.NewLine}{string.Join(Environment.NewLine, pluginFiles)}");
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"在插件文件夹搜索插件文件失败，原因：{ex.Message}，堆栈：{ex.StackTrace}");
                    return;
                }
                foreach (string pluginFile in pluginFiles)
                {
                    PluginLoader loader = new();
                    try
                    {
                        loader.LoadPlugin(pluginFile);
                        if (loader.Plugin != null)
                        {
                            loader.PluginStarted += HandlePluginStarted;
                            loader.PluginStopped += HandlePluginStopped;
                            loader.PluginReportLog += HandlePluginReportLog;
                            loader.StartPlugin();
                            Plugins.Add(loader);

                            if (loader.Plugin is ISpecialMCServerFeaturesProvider provider)
                            {
                                SpecialMCServerFeaturesProviders.Add(provider);
                                StaticTools.HandleLog($"插件[{loader.PluginName}]注册了特定种类服务端特定功能");
                                StaticTools.HandleLog($"目标服务端：{provider.TargetMCServerType}");
                                StaticTools.HandleLog($"功能列表：");
                                foreach (var feature in provider.Features)
                                {
                                    StaticTools.HandleLog($"{feature.FeatureName}:{feature.FeatureDescription}");
                                }
                            }
                            StaticTools.HandleLog($"加载并启动插件[{loader.PluginName}]成功");
                        }
                    }
                    catch (Exception ex)
                    {
                        StaticTools.HandleLog($"加载并启动插件失败，原因：{ex.Message}，堆栈：{ex.StackTrace}");
                    }
                }

            }
            else
            {
                try
                {
                    Directory.CreateDirectory(Paths.PluginsDir);
                }
                catch (Exception ex)
                {
                    StaticTools.HandleLog($"创建插件文件夹失败，原因：{ex.Message}，堆栈：{ex.StackTrace}");
                }
            }
        }
        // ===== 新增：卸载单个插件（按名称） =====
        internal static bool UnloadPlugin(string pluginName)
        {
            PluginLoader? target = null;
            using (Lock.EnterScope())
            {
                target = Plugins.FirstOrDefault(p =>
                   string.Equals(p.PluginName, pluginName, StringComparison.OrdinalIgnoreCase));
                if (target != null)
                {
                    Plugins.Remove(target);
                    if (target.Plugin is ISpecialMCServerFeaturesProvider provider)
                        SpecialMCServerFeaturesProviders.Remove(provider);
                }
            }

            if (target == null)
            {
                StaticTools.HandleLog($"未找到要卸载的插件：{pluginName}");
                return false;
            }

            try
            {
                target.StopPlugin();
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
                target.PluginStarted -= HandlePluginStarted;
                target.PluginReportLog -= HandlePluginReportLog;
                target.PluginStopped -= HandlePluginStopped;
#pragma warning restore CS8601 // 引用类型赋值可能为 null。
                target.UnloadPlugin();
                StaticTools.HandleLog($"插件已卸载：{pluginName}");
                return true;
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog($"卸载插件[{pluginName}]时出错：{ex.Message}，堆栈：{ex.StackTrace}");
                return false;
            }


        }
        internal static void HandlePluginStarted(string pluginName)
        {
            StaticTools.HandleLog($"插件[{pluginName}]已启动");
        }
        internal static void HandlePluginStopped(string pluginName)
        {
            StaticTools.HandleLog($"插件[{pluginName}]已停止");
        }
        internal static void HandlePluginReportLog(string pluginName, string log)
        {
            StaticTools.HandleLog($"[{pluginName}]:{log}");
        }
    }

}
