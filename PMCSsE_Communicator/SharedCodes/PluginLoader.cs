using PMCSsE_Communicator.PluginLoader;
using System.Reflection;

namespace PMCSsE_Communicator.SharedCodes
{
    public class PluginLoader
    {
        private PluginLoadContext? _context;
        public IPlugin? Plugin;
        private WeakReference? _contextWeakRef; // 用于验证卸载
        public string? PluginName => Plugin?.Name;
        public Action<string> PluginStarted = delegate { };
        public Action<string,string> PluginReportLog = delegate { };
        public Action<string> PluginStopped = delegate { };
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
