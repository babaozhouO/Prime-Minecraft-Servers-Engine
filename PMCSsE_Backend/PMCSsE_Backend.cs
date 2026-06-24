/*Copyright 2025 八宝粥(1749861851@qq.com)

   Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/
using PMCSsE_Backend.Modules;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace PMCSsE_Backend
{
    internal class PMCSsE_Backend
    {
        private async static Task<int> Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            if (args.Contains("background"))
            {
                if (Paths.APPExeFile != null)
                {
                    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    {
                        string? exe = Assembly.GetEntryAssembly()?.Location;
                        if (exe != null)
                        {
                            Console.WriteLine($"当前可执行文件路径：{exe}");

                            ProcessStartInfo info_unix = new(Paths.APPExeFile)
                            {
                                CreateNoWindow = true,
                                UseShellExecute = false,
                                Arguments = exe
                            };
                            Process.Start(info_unix);
                            return 0;
                        }
                    }
                    Console.WriteLine($"当前可执行文件路径：{Paths.APPExeFile}");
                    ProcessStartInfo info = new(Paths.APPExeFile)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(info);
                    return 0;
                }

            }
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                MCServerManagers_ManagerClass.ShutDown();
                StaticTools.HandleLog("正在停止日志记录");
                StaticTools.LogsWriter.Dispose();
                StaticTools.HandleLog("已停止日志记录");
                StaticTools.HandleLog("程序已停止并释放资源");
                var ex = (Exception)e.ExceptionObject;
                try { StaticTools.HandleLog($"发生未处理的异常，请复制此消息并反馈: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}内部异常：{ex.InnerException?.Message}"); } catch { }
                Thread.Sleep(10000);//10s
                try { Console.ReadLine(); } catch { }
                try { Environment.Exit(1); } catch { }
            };
            if (args.Contains("debug"))
            {
                RunningStateRecorder.Debug = true;
            }
            if (args.Contains("first"))//配置流程
            {
                if (Console.IsInputRedirected)
                {
                    StaticTools.HandleLog("初次启动需要进行交互以配置程序，但当前输入流已被重定向");
                    StaticTools.HandleLog("若使用Linux，请尝试直接使用SSH连接");
                    return 1;
                }
                StaticTools.HandleLog($"PMCSsE正在启动（配置模式）{(RunningStateRecorder.Debug ? "（调试模式）" : "")}");
                StaticTools.HandleLog("项目网址：https://github.com/babaozhouO/Prime-Minecraft-Servers-Engine");
                StaticTools.HandleLog($"此模式下，你可以：");
                StaticTools.HandleLog($"配置/修改端口号");
                StaticTools.HandleLog($"配置/修改访问密钥");
                if (!StaticConfigManagerClass.LoadConfig())
                {
                    StaticTools.HandleLog("按Enter键退出");
                    try
                    {
                        Console.ReadLine();
                    }
                    catch { }
                    return 1;
                }
                while (true)
                {
                    StaticTools.HandleLog("请设置前端的连接端口，纯数字（0~65535）");
                    StaticTools.HandleLog("应避免使用网络服务中的常用端口号，如Web(80)，SSH(22)");
                    string? input;
                    try
                    {
                        input = Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        StaticTools.HandleLog("读取命令行输入失败");
                        StaticTools.HandleLog($"异常:{ex.Message}");
                        StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                        StaticTools.HandleLog("按Enter键退出");
                        try
                        {
                            Console.ReadLine();
                        }
                        catch { }
                        return 1;
                    }
                    if (string.IsNullOrEmpty(input))
                    {
                        StaticTools.HandleLog("输入为空");
                        continue;
                    }
                    int port;
                    try
                    {
                        port = Convert.ToInt32(input);
                    }
                    catch (FormatException)
                    {
                        StaticTools.HandleLog("非纯数字");
                        continue;
                    }
                    catch (OverflowException)
                    {
                        StaticTools.HandleLog("数字过大");
                        continue;
                    }
                    if (port < 0 || port > 65535)
                    {
                        StaticTools.HandleLog("超出端口号范围（0~65535）");
                        continue;
                    }
                    try
                    {
                        using var listener = new TcpListener(IPAddress.Any, port);
                        listener.Start();
                        listener.Stop();
                    }
                    catch (SocketException ex)
                    {
                        // 10048 (Win32) 或 98 EADDRINUSE (Linux/macOS) 表示地址已在使用
                        if (ex.ErrorCode == 10048 || ex.ErrorCode == 98)
                        {
                            StaticTools.HandleLog("指定的端口号已被占用");
                            continue;
                        }
                        StaticTools.HandleLog("检测端口号占用情况时发生异常");
                        StaticTools.HandleLog($"异常:{ex.Message}");
                        StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        StaticTools.HandleLog("检测端口号占用情况时发生异常");
                        StaticTools.HandleLog($"异常:{ex.Message}");
                        StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                        continue;
                    }
                    //检查通过
                    StaticTools.HandleLog($"端口号可用性验证通过");
                    StaticAPPConfigClass.ListenPort = port;
                    break;
                }
                while (StaticAPPConfigClass.Registered)
                {
                    StaticTools.HandleLog($"已设置访问密钥，修改需输入旧访问密钥，不输入任何文字并按Enter可取消修改");
                    string? input;
                    try
                    {
                        input = Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        StaticTools.HandleLog("读取命令行输入失败");
                        StaticTools.HandleLog($"异常:{ex.Message}");
                        StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                        StaticTools.HandleLog("按Enter键退出");
                        try
                        {
                            Console.ReadLine();
                        }
                        catch { }
                        return 1;
                    }
                    if (!string.IsNullOrEmpty(input))
                    {
                        byte[] passwordBytes;
                        try
                        {
                            passwordBytes = Encoding.UTF8.GetBytes(input);
                        }
                        catch (Exception ex)
                        {
                            StaticTools.HandleLog("使用UTF8编码输入的旧密钥失败");
                            StaticTools.HandleLog($"异常:{ex.Message}");
                            StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                            continue;
                        }
                        //迭代次数合理，算法跨平台，sha256全平台支持，输出长度合理，不会发生异常
                        byte[] saltedPasswordHash = Rfc2898DeriveBytes.Pbkdf2(
                            passwordBytes,
                            StaticAPPConfigClass.Salt,
                            iterations: 100000,//迭代次数
                            hashAlgorithm: HashAlgorithmName.SHA256,
                            outputLength: 32
                        );
                        if (saltedPasswordHash.SequenceEqual(StaticAPPConfigClass.SaltedPasswordHash))
                        {
                            StaticTools.HandleLog("密钥正确");
                            StaticAPPConfigClass.Registered = false;
                            break;
                        }
                        else
                        {
                            StaticTools.HandleLog("密钥错误");
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                while (!StaticAPPConfigClass.Registered)
                {
                    StaticTools.HandleLog("为保证安全，必须设置访问密钥");
                    StaticTools.HandleLog("是否让程序自动生成一个（Y/N）");
                    string? choice;
                    try
                    {
                        choice = Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        StaticTools.HandleLog("读取命令行输入失败");
                        StaticTools.HandleLog($"异常:{ex.Message}");
                        StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                        StaticTools.HandleLog("按Enter键退出");
                        try
                        {
                            Console.ReadLine();
                        }
                        catch { }
                        return 1;
                    }

                    if (!string.IsNullOrEmpty(choice))
                    {
                        if (choice == "Y" || choice == "y")
                        {
                            byte[] passwordBytes = new byte[18];
                            byte[] salt = new byte[16];
                            using var rng = RandomNumberGenerator.Create();
                            {
                                rng.GetBytes(passwordBytes);
                                rng.GetBytes(salt);
                            }//统一进行UTF8编码
                            string password = Convert.ToBase64String(passwordBytes);//用作展示
                            passwordBytes = Encoding.UTF8.GetBytes(password);//base64绝对编码成功
                            byte[] saltedPasswordHash = Rfc2898DeriveBytes.Pbkdf2(
                                passwordBytes,
                                salt,
                                iterations: 100000,//迭代次数
                                hashAlgorithm: HashAlgorithmName.SHA256,
                                outputLength: 32
                            );
                            StaticAPPConfigClass.SaltedPasswordHash = saltedPasswordHash;
                            StaticAPPConfigClass.Salt = salt;
                            StaticAPPConfigClass.Registered = true;
                            if (!StaticConfigManagerClass.SaveAPPConfig())
                            {
                                StaticTools.HandleLog("按Enter键退出");
                                try
                                {
                                    Console.ReadLine();
                                }
                                catch { }
                                return 1;
                            }
                            StaticTools.HandleLog("以后请使用以下密钥登录");
                            StaticTools.HandleLog(password, true);//已做保护，密钥不会记录在日志里
                            StaticTools.HandleLog("请妥善保管密钥，丢失后无法找回");
                            StaticTools.HandleLog("将清空控制台文本，请记下密钥后再按下Enter");
                            try { Console.ReadLine(); } catch { }
                            try { Console.Clear(); } catch { }
                        }
                        else if (choice == "N" || choice == "n")
                        {
                            StaticTools.HandleLog("请输入你想设置的访问密钥");
                            string? password;
                            try
                            {
                                password = Console.ReadLine();
                            }
                            catch (Exception ex)
                            {
                                StaticTools.HandleLog("读取命令行输入失败");
                                StaticTools.HandleLog($"异常:{ex.Message}");
                                StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                                StaticTools.HandleLog("按Enter键退出");
                                try
                                {
                                    Console.ReadLine();
                                }
                                catch { }
                                return 1;
                            }

                            if (!string.IsNullOrEmpty(password))
                            {
                                byte[] passwordBytes;
                                try
                                {
                                    passwordBytes = Encoding.UTF8.GetBytes(password);
                                }
                                catch (Exception ex)
                                {
                                    StaticTools.HandleLog("使用UTF8编码输入的密钥失败");
                                    StaticTools.HandleLog($"异常:{ex.Message}");
                                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                                    continue;//重来
                                }
                                byte[] salt = new byte[16];
                                using var rng = RandomNumberGenerator.Create();
                                {
                                    rng.GetBytes(salt);
                                }
                                byte[] saltedPasswordHash = Rfc2898DeriveBytes.Pbkdf2(
                                    passwordBytes,
                                    salt,
                                    iterations: 100000,//迭代次数
                                    hashAlgorithm: HashAlgorithmName.SHA256,
                                    outputLength: 32
                                );
                                StaticAPPConfigClass.SaltedPasswordHash = saltedPasswordHash;
                                StaticAPPConfigClass.Salt = salt;
                                StaticAPPConfigClass.Registered = true;
                                if (!StaticConfigManagerClass.SaveAPPConfig())
                                {
                                    StaticTools.HandleLog("按Enter键退出");
                                    try
                                    {
                                        Console.ReadLine();
                                    }
                                    catch { }
                                    return 1;
                                }
                                StaticTools.HandleLog("以后请使用以下密钥登录");
                                StaticTools.HandleLog(password, true);//已做保护，密钥不会记录在日志里
                                StaticTools.HandleLog("请妥善保管密钥，丢失后无法找回");
                                StaticTools.HandleLog("将清空控制台文本，请记下密钥后再按下Enter");
                                try { Console.ReadLine(); } catch { }
                                try { Console.Clear(); } catch { }
                            }
                            else
                            {
                                StaticTools.HandleLog("输入为空");
                            }
                        }
                    }
                    else
                    {
                        StaticTools.HandleLog("请选择其中之一");
                    }
                }
                //配置完成
                StaticTools.HandleLog("已完成配置流程，此后不再要求使用可交互终端也不接受任何命令");
                StaticTools.HandleLog($"所有操作均在前端的图形化界面上完成");
                StaticTools.HandleLog($"请删除“first”启动参数后再次启动");
                Thread.Sleep(3000);
                try { Console.ReadLine(); } catch { }
                return 0;
            }
            StaticTools.HandleLog($"PMCSsE正在启动{(RunningStateRecorder.Debug ? "（调试模式）" : "")}");
            StaticTools.HandleLog("项目网址：https://github.com/babaozhouO/Prime-Minecraft-Servers-Engine");
            StaticTools.HandleLog($"系统命令行使用的编码：{RunningStateRecorder.SystemCommandLineEncoding.EncodingName}");
            if (!StaticConfigManagerClass.LoadConfig())
            {
                StaticTools.HandleLog("按Enter键退出");//保留原因：后台运行时，console.readline会直接报错跳过，前台运行时，显示报错信息
                try
                {
                    Console.ReadLine();
                }
                catch { }
                return 1;
            }
            if (!StaticAPPConfigClass.Registered)
            {
                StaticTools.HandleLog("未执行配置流程，请在运行命令后添加“first”参数并在可交互终端环境下运行");
                Thread.Sleep(10000);
                return 1;
            }
            CancellationTokenSource ExitTokenSource = new();
            MCServerManagers_ManagerClass.ExitCalled += () =>
            {
                ExitTokenSource.Cancel();
            };

            // ---- 拦截退出信号，触发安全关闭流程 ----
            using var sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, ctx =>
            {
                ctx.Cancel = true; // 阻止默认行为（立即杀进程），改为自己清理
                StaticTools.HandleLog("收到 SIGINT 信号（Ctrl+C），正在安全退出...");
                ExitTokenSource.Cancel();
            });
            using var sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
            {
                ctx.Cancel = true;
                StaticTools.HandleLog("收到 SIGTERM 信号，正在安全退出...");
                ExitTokenSource.Cancel();
            });
            // Linux 下关闭终端 / SSH 断开 会发送 SIGHUP
            using var sighup = PosixSignalRegistration.Create(PosixSignal.SIGHUP, ctx =>
            {
                ctx.Cancel = true;
                StaticTools.HandleLog("收到 SIGHUP 信号（终端关闭/SSH断开），正在安全退出...");
                ExitTokenSource.Cancel();
            });
            // Windows 下 Console.CancelKeyPress 作为双保险
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                ExitTokenSource.Cancel();
            };
            // ---- 退出信号拦截结束 ----

            StaticTools.HandleLog($"将在*:{StaticAPPConfigClass.ListenPort}上监听前端连接请求");
            MCServerManagers_ManagerClass.Initialize();
            try
            {
                await Task.Delay(Timeout.Infinite, ExitTokenSource.Token);
            }
            catch (Exception ex)
            {
                StaticTools.HandleLog("在Main函数中等待组件发出退出信号时发生异常");
                StaticTools.HandleLog($"异常：{ex.Message}");
                StaticTools.HandleLog($"堆栈跟踪：{ex.StackTrace}");
            }
            MCServerManagers_ManagerClass.ShutDown();
            StaticTools.HandleLog("正在停止日志记录");
            StaticTools.LogsWriter.Dispose();
            StaticTools.HandleLog("已停止日志记录");
            StaticTools.HandleLog("程序已停止并释放资源");
            StaticTools.HandleLog("请按Enter键退出");
            try { Console.ReadLine(); } catch { }
            return 0;
        }
    }
}
