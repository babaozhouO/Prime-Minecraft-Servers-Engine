using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

namespace PMCSsE_Communicator.SharedCodes
{
    /// <summary>
    /// 插件专用的程序集加载上下文，继承自 AssemblyLoadContext 并启用可回收模式。
    /// 使用 AssemblyDependencyResolver 解析插件目录下的依赖程序集，
    /// 对于共享接口程序集则回退到默认加载上下文以避免重复加载。
    /// </summary>
    /// <remarks>
    /// 使用指定的插件路径初始化可回收的加载上下文。
    /// </remarks>
    /// <param name="pluginPath">插件程序集文件的完整路径。</param>
    public class PluginLoadContext(string pluginPath) : AssemblyLoadContext(isCollectible: true)
    {
        private AssemblyDependencyResolver _resolver = new AssemblyDependencyResolver(pluginPath);

        /// <summary>
        /// 加载指定名称的程序集。优先使用依赖解析器从插件目录加载；
        /// 若解析失败则返回 null，交由运行时默认加载上下文处理（用于共享接口程序集）。
        /// </summary>
        /// <param name="assemblyName">要加载的程序集名称。</param>
        /// <returns>加载的程序集，若应由默认上下文处理则返回 null。</returns>
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            // 先尝试用依赖解析器查找
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);

            // 对于共享的接口程序集，回退到默认上下文，避免重复加载
            return null; // 返回 null 让运行时用默认加载方式
        }

        /// <summary>
        /// 加载指定名称的非托管 DLL。优先从插件目录解析路径；
        /// 若解析失败则返回 IntPtr.Zero。
        /// </summary>
        /// <param name="unmanagedDllName">非托管 DLL 的名称。</param>
        /// <returns>加载的非托管库句柄，失败时返回 IntPtr.Zero。</returns>
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);
            return IntPtr.Zero;
        }
    }
}

