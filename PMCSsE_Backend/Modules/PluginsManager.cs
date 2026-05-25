

using PMCSsE_Communicator.PluginLoader;
using System.Reflection;

namespace PMCSsE_Backend.Modules
{
    internal static class PluginsManager
    {
        private static bool CanWork = true;
        private static readonly List<IPlugin> LoadedPlugins = [];
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
                    CanWork = false;
                    StaticTools.HandleLog($"在插件文件夹搜索插件文件失败，原因：{ex.Message}，堆栈：{ex.StackTrace}");
                    return;
                }
                foreach (string pluginFile in pluginFiles)
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(pluginFile);

                        // 查找所有实现了IPlugin接口的类型
                        IEnumerable<Type> pluginTypes = assembly.GetTypes()
                            .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                        foreach (var type in pluginTypes)
                        {
                            try
                            {
                                // 创建插件实例
                                if (Activator.CreateInstance(type) is IPlugin plugin)
                                {
                                    LoadedPlugins.Add(plugin);
                                    try
                                    {
                                        plugin.Initialize();
                                        StaticTools.HandleLog($"已初始化插件： {plugin.Name} v{plugin.Version}");
                                        plugin.Start();
                                        StaticTools.HandleLog($"已启动插件： {plugin.Name} v{plugin.Version}");
                                    }
                                    catch (Exception ex)
                                    {
                                        try { plugin.Stop(); } catch { }
                                        try { plugin.Dispose(); } catch { }
                                        StaticTools.HandleLog($"插件：{plugin.Name} v{plugin.Version}初始化/启动时未处理抛出的异常，已停止插件并释放资源");
                                        StaticTools.HandleLog($"原因：{ex.Message}，堆栈：{ex.StackTrace}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                StaticTools.HandleLog($"创建插件实例失败：{type.FullName}, 原因：{ex.Message}，堆栈：{ex.StackTrace}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        StaticTools.HandleLog($"加载插件失败，原因： {ex.Message}，堆栈：{ex.StackTrace}");
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
                    CanWork = false;
                    StaticTools.HandleLog($"创建插件文件夹失败，原因：{ex.Message}，堆栈：{ex.StackTrace}");
                }
            }
        }
    }
}
