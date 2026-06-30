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
using PMCSsE_Communicator.SharedCodes;
using Renci.SshNet.Security;
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
                var ex = (Exception)e.ExceptionObject;
                StaticTools.HandleLog($"发生未处理的异常，请复制此消息并反馈: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}内部异常：{ex.InnerException?.Message}");
                MCServerManagers_ManagerClass.ShutDown();
                StaticTools.HandleLog("正在停止日志记录");
                StaticTools.LogsWriter.Dispose();
                StaticTools.HandleLog("已停止日志记录");
                StaticTools.HandleLog("程序已停止并释放资源");
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
                    StaticTools.HandleLog("若使用Linux，请尝试使用SSH直接连接，部分远程面板会重定向输入");
                    return 1;
                }
                StaticTools.HandleLog($"PMCSsE正在启动（配置模式）{(RunningStateRecorder.Debug ? "（调试模式）" : "")}");
                StaticTools.HandleLog("项目网址：https://github.com/babaozhouO/Prime-Minecraft-Servers-Engine");
                switch (StaticConfigManager.ConfigFilesState)
                {
                    case StaticConfigManager.ConfigFilesStateEnum.Good:
                        StaticTools.HandleLog($"当前配置文件状态：正常");
                        StaticTools.HandleLog($"此模式下，你可以：");
                        StaticTools.HandleLog($"修改监听地址及端口号");
                        StaticTools.HandleLog($"修改访问密钥");
                        if (!StaticConfigManager.LoadConfig_Plaintext())
                        {
                            StaticTools.HandleLog("按Enter键退出");
                            try
                            {
                                Console.ReadLine();
                            }
                            catch { }
                            return 1;
                        }
                        break;
                    case StaticConfigManager.ConfigFilesStateEnum.WhereIsThePlaintextConfig:
                        StaticTools.HandleLog($"当前配置文件状态：明文部分缺失");
                        StaticTools.HandleLog("请检查配置文件是否被意外删除");
                        StaticTools.HandleLog($"若你想删除配置文件，请将两个文件一并删除");
                        StaticTools.HandleLog($"若你忘记密码，只能删除所有配置，无任何手段找回密码");
                        StaticTools.HandleLog($"请退出程序，处理配置文件异常");
                        StaticTools.HandleLog("按Enter键退出");
                        try
                        {
                            Console.ReadLine();
                        }
                        catch { }
                        return 1;
                    case StaticConfigManager.ConfigFilesStateEnum.WhereIsTheCiphertextConfig:
                        StaticTools.HandleLog($"当前配置文件状态：密文部分缺失");
                        StaticTools.HandleLog("请检查配置文件是否被意外删除");
                        StaticTools.HandleLog($"若你想删除配置文件，请将两个文件一并删除");
                        StaticTools.HandleLog($"若你忘记密码，只能删除所有配置，无任何手段找回密码");
                        StaticTools.HandleLog($"请退出程序，处理配置文件异常");
                        StaticTools.HandleLog("按Enter键退出");
                        try
                        {
                            Console.ReadLine();
                        }
                        catch { }
                        return 1;
                    case StaticConfigManager.ConfigFilesStateEnum.Welcome:
                        StaticTools.HandleLog($"当前配置文件状态：未初始化");
                        StaticTools.HandleLog($"此模式下，你可以：");
                        StaticTools.HandleLog($"配置监听地址及端口号");
                        StaticTools.HandleLog($"配置访问密钥");
                        break;
                }
                while (true)
                {
                    // 构建可选地址列表
                    var options = new List<string>
                    {
                        "All",
                        "0.0.0.0",
                        "::",
                        "127.0.0.1",
                        "::1"
                    };
                    try
                    {
                        var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
                        foreach (var iPAddress in hostEntry.AddressList)
                        {
                            options.Add(iPAddress.ToString());
                        }
                    }
                    catch (SocketException ex)
                    {
                        StaticTools.HandleLog($"获取本机可用地址时发生Socket异常：{ex.Message}，错误码：{ex.SocketErrorCode}，堆栈：{ex.StackTrace}");
                    }
                    catch (Exception ex)
                    {
                        StaticTools.HandleLog($"获取本机可用地址时发生异常：{ex.Message}，堆栈：{ex.StackTrace}");
                    }

                    // 输出列表供用户选择
                    StaticTools.HandleLog("请选择要监听的 IP 地址 (输入对应序号):");
                    for (int i = 0; i < options.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                StaticTools.HandleLog($"[{i}] {options[i]} (所有IPv4和IPv6地址)");
                                break;
                            case 1:
                                StaticTools.HandleLog($"[{i}] {options[i]} (所有IPv4地址)");
                                break;
                            case 2:
                                StaticTools.HandleLog($"[{i}] {options[i]} (所有IPv6地址)");
                                break;
                            case 3:
                                StaticTools.HandleLog($"[{i}] {options[i]} (本机IPv4回环)");
                                break;
                            case 4:
                                StaticTools.HandleLog($"[{i}] {options[i]} (本机IPv6回环)");
                                break;
                            default:
                                StaticTools.HandleLog($"[{i}] {options[i]}");
                                break;
                        }
                    }

                    StaticTools.HandleLog("请输入序号: ");
                    string? input = Console.ReadLine();
                    while (true)
                    {
                        if (!int.TryParse(input, out int selectedIndex) || selectedIndex < 0 || selectedIndex >= options.Count)
                        {
                            StaticTools.HandleLog("输入无效");
                            continue;
                        }
                        StaticConfig_Plaintext.ListenAddress = options[selectedIndex];
                        StaticTools.HandleLog($"选择的地址：{StaticConfig_Plaintext.ListenAddress}");
                        break;
                    }

                    StaticTools.HandleLog("请设置前端的连接端口，纯数字（1~65535）");
                    StaticTools.HandleLog("应避免使用网络服务中的常用端口号");
                    StaticTools.HandleLog("如Web(80)，SSH(22)，以及其它具有特殊作用的端口");
                    StaticTools.HandleLog("若使用低端口号（1~1023），启动监听需要以管理员身份运行");

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
                    if (port < 1 || port > 65535)
                    {
                        StaticTools.HandleLog("超出端口号范围（1~65535）");
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
                    StaticConfig_Plaintext.ListenPort = port;
                    StaticConfigManager.SaveConfig_Plaintext();
                    break;
                }
                while (!StaticConfig_Plaintext.SaltOfCipherConfigKey.Equals(Array.Empty<byte>()))
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
                        if (input.Length < 8 && !RunningStateRecorder.Debug)//方便调试
                        {
                            StaticTools.HandleLog("密钥长度过短，应大于或等于8个字符");
                            continue;
                        }
                        byte[] keyBytes;
                        try
                        {
                            keyBytes = Encoding.UTF8.GetBytes(input);
                        }
                        catch (Exception ex)
                        {
                            StaticTools.HandleLog("使用UTF8编码输入的旧密钥失败");
                            StaticTools.HandleLog($"异常:{ex.Message}");
                            StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                            continue;
                        }

                        byte[] saltedCipherConfigKeyHash = ConfigCrypto.DeriveKey(keyBytes, StaticConfig_Plaintext.SaltOfCipherConfigKey);
                        if (!StaticConfigManager.LoadConfig_Ciphertext(saltedCipherConfigKeyHash))
                        {
                            StaticTools.HandleLog("按Enter键退出");
                            try
                            {
                                Console.ReadLine();
                            }
                            catch { }
                            return 1;
                        }
                        //解密、加载成功
                        CryptographicOperations.ZeroMemory(saltedCipherConfigKeyHash.AsSpan());
                        StaticConfig_Plaintext.SaltOfCipherConfigKey = [];
                        break;
                    }
                    else
                    {
                        break;
                    }
                }
                while (StaticConfig_Plaintext.SaltOfCipherConfigKey.Equals(Array.Empty<byte>()))
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
                    if (string.IsNullOrEmpty(choice))
                    {
                        StaticTools.HandleLog("请选择其中之一");
                        continue;
                    }
                    choice = choice.ToUpper();
                    if (choice != "Y" && choice != "N")
                    {
                        StaticTools.HandleLog("请选择其中之一");
                        continue;
                    }

                    byte[] keyBytes = new byte[18];
                    if (choice == "Y")
                    {
                        using var rng = RandomNumberGenerator.Create();
                        {
                            rng.GetBytes(keyBytes);
                        }//统一进行UTF8编码
                        string key = Convert.ToBase64String(keyBytes);//用作展示
                        keyBytes = Encoding.UTF8.GetBytes(key);//base64绝对编码成功
                        StaticTools.HandleLog("以后请使用以下密钥登录");
                        StaticTools.HandleLog(key, true);//已做保护，密钥不会记录在日志里
                    }
                    else
                    {
                        while (true)
                        {
                            StaticTools.HandleLog("请输入你想设置的访问密钥");
                            string? key;
                            try
                            {
                                key = Console.ReadLine();
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

                            if (!string.IsNullOrEmpty(key))
                            {

                                if (key.Length < 8 && !RunningStateRecorder.Debug)//方便调试
                                {
                                    StaticTools.HandleLog("密钥长度过短，应大于或等于8个字符");
                                    continue;
                                }
                                try
                                {
                                    keyBytes = Encoding.UTF8.GetBytes(key);
                                    StaticTools.HandleLog("以后请使用以下密钥登录");
                                    StaticTools.HandleLog(key, true);//已做保护，密钥不会记录在日志里
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    StaticTools.HandleLog("使用UTF8编码输入的密钥失败");
                                    StaticTools.HandleLog($"异常:{ex.Message}");
                                    StaticTools.HandleLog($"堆栈跟踪:{ex.StackTrace}");
                                    continue;//重来
                                }
                            }
                            else
                            {
                                StaticTools.HandleLog("输入为空");
                            }
                        }
                    }

                    byte[] saltOfCipherConfigKey = new byte[16];
                    byte[] saltOfLoginKey = new byte[16];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(saltOfCipherConfigKey);
                        rng.GetBytes(saltOfLoginKey);
                    }
                    byte[] saltedConfigKeyHash = ConfigCrypto.DeriveKey(keyBytes, saltOfCipherConfigKey);
                    byte[] saltedLoginKeyHash = ConfigCrypto.DeriveKey(keyBytes, saltOfLoginKey);
                    StaticConfig_Plaintext.SaltOfCipherConfigKey = saltOfCipherConfigKey;
                    StaticConfig_Ciphertext.SaltedLoginKeyHash = saltedLoginKeyHash;
                    StaticConfig_Ciphertext.SaltOfLoginKey = saltOfLoginKey;
                    if (!StaticConfigManager.SaveConfig_Plaintext() || !StaticConfigManager.SaveConfig_Ciphertext(saltedConfigKeyHash))
                    {//保存失败
                        StaticConfigManager.DeleteAllConfigFile();
                    }
                    StaticTools.HandleLog("已生成配置文件（一明文，一密文）");
                    StaticTools.HandleLog("请妥善保管密钥，丢失后无法找回");
                    StaticTools.HandleLog("将清空控制台文本，请记下密钥后再按下Enter");
                    try { Console.ReadLine(); } catch { }
                    try { Console.Clear(); } catch { }
                }
                //if (!StaticConfigManagerClass.SaveAPPConfig())
                //{
                //    StaticTools.HandleLog("按Enter键退出");
                //    try
                //    {
                //        Console.ReadLine();
                //    }
                //    catch { }
                //    return 1;
                //}
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
            if (!StaticConfigManager.LoadConfig_Plaintext())
            {
                StaticTools.HandleLog("按Enter键退出");//保留原因：后台运行时，console.readline会直接报错跳过，前台运行时，显示报错信息
                try
                {
                    Console.ReadLine();
                }
                catch { }
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

            StaticTools.HandleLog($"将在{StaticConfig_Plaintext.ListenAddress}:{StaticConfig_Plaintext.ListenPort}上监听前端连接请求");
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
