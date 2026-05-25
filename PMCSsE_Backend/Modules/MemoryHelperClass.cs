using System;
using System.Runtime.InteropServices;

namespace Prime_Minecraft_Servers_Engine.Modules
{
    internal static class MemoryHelperClass
    {
        // 内存信息结构体
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX Init()
            {
                dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
                return this;
            }
        }

        // WinAPI 导入
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        // 缓存总内存值（不变）
        private static readonly double _totalMB; // 改为MB单位
        private static readonly double _mbFactor = 1.0 / (1024 * 1024); // 新的MB转换因子

        static MemoryHelperClass()
        {
            // 初始化时获取总内存
            var memStatus = new MEMORYSTATUSEX().Init();
            if (GlobalMemoryStatusEx(ref memStatus))
            {
                // 直接转换为MB
                _totalMB = Math.Round(memStatus.ullTotalPhys * _mbFactor, 2);
            }
            else
            {
                // 备用方案：使用物理内存API
                _totalMB = GetTotalPhysicalMemoryMB(); // 改为MB单位
            }
        }

        public static (double Total, double Used, double Free) GetMemoryInfo()
        {
            var memStatus = new MEMORYSTATUSEX().Init();

            if (!GlobalMemoryStatusEx(ref memStatus))
            {
                // API调用失败时返回默认值
                return (_totalMB, 0, 0);
            }

            // 转换为MB
            double freeMB = Math.Round(memStatus.ullAvailPhys * _mbFactor, 2);
            double usedMB = Math.Round(_totalMB - freeMB, 2);

            return (_totalMB, usedMB, freeMB);
        }

        // 备用方案：获取物理内存大小（返回MB单位）
        private static double GetTotalPhysicalMemoryMB()
        {
            try
            {
                [DllImport("kernel32.dll")]
                [return: MarshalAs(UnmanagedType.Bool)]
                static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryKB);

                if (GetPhysicallyInstalledSystemMemory(out ulong memoryKB))
                {
                    // 将KB转换为MB (1MB = 1024KB)
                    return Math.Round(memoryKB / 1024.0, 2);
                }
            }
            catch
            {
                // 忽略错误
            }

            // 最终备用方案：使用系统信息（转换为MB）
            return Environment.Is64BitOperatingSystem ? 16 * 1024 : 4 * 1024;
        }
    }
}