using System.Reflection;

namespace PMCSsE_Communicator.SharedCodes
{
    /// <summary>
    /// 插件加载器，负责从 DLL 文件加载、启动、停止和卸载插件。
    /// 使用可回收的 AssemblyLoadContext 实现插件热加载与卸载。
    /// </summary>
    public class PluginLoader
    {
        private PluginLoadContext? _context;
        /// <summary>
        /// 已加载的插件实例。若未加载插件则为 null。
        /// </summary>
        public IPlugin? Plugin;
        private WeakReference? _contextWeakRef; // 用于验证卸载
        /// <summary>
        /// 已加载插件的名称。若未加载插件则为 null。
        /// </summary>
        public string? PluginName => Plugin?.Name;
        /// <summary>
        /// 插件启动时触发，参数为插件名称。
        /// </summary>
        public Action<string> PluginStarted = delegate { };
        /// <summary>
        /// 插件上报日志时触发，参数为插件名称和日志内容。
        /// </summary>
        public Action<string,string> PluginReportLog = delegate { };
        /// <summary>
        /// 插件停止时触发，参数为插件名称。
        /// </summary>
        public Action<string> PluginStopped = delegate { };
        /// <summary>
        /// 从指定路径加载插件程序集，查找并实例化 IPlugin 实现，然后执行初始化。
        /// 若加载失败会抛出异常，并自动清理已创建的对象。
        /// </summary>
        /// <param name="pluginAssemblyPath">插件 DLL 文件的完整路径。</param>
        public void LoadPlugin(string pluginAssemblyPath)
        {
            // 防止重复加载，先卸载现有的
            if (_context != null)
                UnloadPlugin();

            PluginLoadContext? context = null;
            IPlugin? instance = null;

            try
            {
                context = new PluginLoadContext(pluginAssemblyPath);
                Assembly assembly = context.LoadFromAssemblyPath(pluginAssemblyPath);

                var pluginType = assembly.GetExportedTypes()
                    .FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);
                if (pluginType == null)
                    throw new InvalidOperationException("No IPlugin implementation found.");

                instance = (IPlugin)Activator.CreateInstance(pluginType)!;
                instance.Initialize(); // 插件初始化（可能抛异常）
                instance.Started += PluginStarted;
                instance.ReportLog += PluginReportLog;
                instance.Stopped += PluginStopped;
                // 一切顺利才赋值给字段
                _context = context;
                Plugin = instance;
                _contextWeakRef = new WeakReference(context);
            }
            catch
            {
                // 初始化失败，必须清理已经创建的对象
                instance?.Stop(); // 若已初始化，可能需反初始化
                context?.Unload();
                // 强制回收临时上下文
                context = null;
                instance = null;
                for (int i = 0; i < 5; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); }
                throw; // 重新抛出原始异常，让调用方知晓
            }
        }

        /// <summary>
        /// 启动已加载的插件。若未加载插件则抛出 InvalidOperationException。
        /// </summary>
        public void StartPlugin()
        {
            if (Plugin == null)
                throw new InvalidOperationException("No plugin loaded.");
            try
            {
                Plugin.Start();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 卸载当前插件。依次调用插件的 Stop/Dispose 方法，卸载 AssemblyLoadContext，
        /// 并执行多次 GC 回收以验证程序集已完全卸载。若卸载可能不完整则抛出异常。
        /// </summary>
        public void UnloadPlugin()
        {
            // 1. 先通知插件进行清理（捕获所有异常，不能影响后续卸载）
            if (Plugin != null)
            {
                try
                {
                    Plugin.Stop();
                    Plugin.Dispose();
                    Plugin.Started -= PluginStarted;
                    Plugin.ReportLog -= PluginReportLog;
                    Plugin.Stopped -= PluginStopped;
                }
                catch
                {
                    throw;
                }
                Plugin = null;
            }

            // 2. 卸载 AssemblyLoadContext
            var context = _context;
            _context = null;
            if (context != null)
            {
                try
                {
                    context.Unload();
                }
                catch
                {
                    throw;
                }
            }

            // 3. 强制 GC 并验证
            for (int i = 0; i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            bool unloaded = _contextWeakRef?.IsAlive == false;
            _contextWeakRef = null;
            if (!unloaded)
            {
                throw new Exception("Plugin context may not be fully unloaded; check for memory leaks.");
            }
        }
        /// <summary>
        /// 停止已加载的插件（仅调用 Stop，不卸载程序集）。若未加载插件则抛出 InvalidOperationException。
        /// </summary>
        public void StopPlugin()
        {
            if (Plugin == null)
                throw new InvalidOperationException("No plugin loaded.");
            try
            {
                Plugin.Stop();
            }
            catch
            {
                throw;
            }
        }
    }
}
