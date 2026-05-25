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

namespace PMCSsE_Backend.Modules
{
    internal class SevenZipInvokerClass : IDisposable, IReportLog
    {

        private readonly System.Diagnostics.Process _7ZipProcess = new() { EnableRaisingEvents = true };

        private readonly System.Timers.Timer CheckCancellationTokenTimer = new()
        {
            AutoReset = true,
            Interval = 1000d
        };

        private bool IsProcessKilled = false;

        public event Action<string, string, string> ReportLog = delegate { };

        internal event Action ProcessExited = delegate { };
        internal SevenZipInvokerClass(CancellationTokenSource cancellationTokenSource)
        {
            CheckCancellationTokenTimer.Elapsed += (sender, e) =>//每秒检查一次cancellationtoken
            {
                if (!_7ZipProcess.HasExited)
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        _7ZipProcess.Kill();
                        IsProcessKilled = true;
                        ReportLog("用户操作", "7Zip调用器", "用户取消任务");

                    }
                }
            };
        }
        /// <summary>
        /// 完整参数
        /// </summary>
        internal void Invoke7Zip(string Parameters, string OutputPath, string InputDirectory)
        {
            if (string.IsNullOrEmpty(Parameters))
            {
                ReportLog("错误", "7Zip调用器", "参数为空");
                ReportLog("失败", "7Zip调用器", "已终止调用");
                return;
            }
            if (string.IsNullOrEmpty(OutputPath))
            {
                ReportLog("错误", "7Zip调用器", "输出路径为空");
                ReportLog("失败", "7Zip调用器", "已终止调用");
                return;
            }
            if (string.IsNullOrEmpty(InputDirectory))
            {
                ReportLog("错误", "7Zip调用器", "输入目录为空");
                ReportLog("失败", "7Zip调用器", "已终止调用");
                return;
            }
            if (!Directory.Exists(InputDirectory))
            {
                ReportLog("错误", "7Zip调用器", "输入目录不存在");
                ReportLog("失败", "7Zip调用器", "已终止调用");
                return;
            }

            string Argument = $"{Parameters} {OutputPath} {InputDirectory}";

            ReportLog("信息", "7Zip调用器", $"完整压缩参数:");
            ReportLog("信息", "7Zip调用器", Argument);
            Run7Zip(Argument);
        }
        /// <summary>
        /// 自动生成
        /// </summary>
        internal void Invoke7Zip(string CompactionLevel, List<string> ExcludedFilesList, List<string> ExcludedFileExtensionsList, List<string> ExcludedFoldersList, string OutputPath, string InputDirectory)
        {
            if (string.IsNullOrEmpty(CompactionLevel))
            {
                ReportLog("错误", "7Zip调用器", "压缩等级为空");
                return;
            }
            if (Convert.ToByte(CompactionLevel) < 0)
            {
                ReportLog("错误", "7Zip调用器", "压缩等级小于0");
                return;
            }
            if (Convert.ToByte(CompactionLevel) > 9)
            {
                ReportLog("错误", "7Zip调用器", "压缩等级大于9");
                return;
            }
            if (string.IsNullOrEmpty(OutputPath))
            {
                ReportLog("错误", "7Zip调用器", "输出路径为空");
                return;
            }
            if (string.IsNullOrEmpty(InputDirectory))
            {
                ReportLog("错误", "7Zip调用器", "输入目录为空");
                return;
            }
            if (!Directory.Exists(InputDirectory))
            {
                ReportLog("错误", "7Zip调用器", "输入目录不存在");
                return;
            }
            List<string> ArgumentsList = ["a","-r","-aoa","-t7z",$"-mx{CompactionLevel}"];
            foreach (string ExcludedFileName in ExcludedFilesList)
            {
                ArgumentsList.Add($"-x!\"{ExcludedFileName}\"");
            }
            foreach (string ExcludedFileExtension in ExcludedFileExtensionsList)
            {
                ArgumentsList.Add($"-x!\"{ExcludedFileExtension}\"");
            }
            foreach (string ExcludedFolderName in ExcludedFoldersList)
            {
                ArgumentsList.Add($"-x!\"{ExcludedFolderName}\"");
            }
            ArgumentsList.Add($"\"{OutputPath}\"");
            ArgumentsList.Add($"\"{InputDirectory}\"");
            string Argument = string.Join(' ', ArgumentsList);

            Run7Zip(Argument);
        }
        internal void Run7Zip(string Argument)
        {
            _7ZipProcess.OutputDataReceived += (sender, OutputDataReceived) =>
            {
                if (string.IsNullOrEmpty(OutputDataReceived.Data) || OutputDataReceived.Data == "                                       ")
                    return;


                ReportLog("信息", "7-Zip进程", OutputDataReceived.Data);

            };
            _7ZipProcess.ErrorDataReceived += (sender, ErrorDataReceived) =>
            {
                if (string.IsNullOrEmpty(ErrorDataReceived.Data) || ErrorDataReceived.Data == "                                       ")
                    return;


                ReportLog("信息", "7-Zip进程", ErrorDataReceived.Data);

            };
            _7ZipProcess.Exited += (sender, e) =>
            {
                CheckCancellationTokenTimer.Stop();
                _7ZipProcess.CancelOutputRead();
                _7ZipProcess.CancelErrorRead();
                if (IsProcessKilled)
                {
                    ReportLog("失败", "7Zip调用器", "因用户取消，7-Zip进程已终止");
                    ProcessExited();
                }
                else if (_7ZipProcess.ExitCode == 0)
                {
                    ReportLog("成功", "7Zip调用器", "7-Zip进程已正常退出");
                    ProcessExited();
                }
                else
                {
                    ReportLog("错误", "7Zip调用器", $"7-Zip进程异常退出，退出代码:{_7ZipProcess.ExitCode}");
                    ProcessExited();
                }
            };

            _7ZipProcess.StartInfo = new()
            {
                FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z", "7z.exe"),
                Arguments = Argument,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            if (_7ZipProcess.Start())
            {
                CheckCancellationTokenTimer.Start();
                _7ZipProcess.BeginOutputReadLine();
                _7ZipProcess.BeginErrorReadLine();
            }
            else
            {
                ProcessExited();
                ReportLog("错误", "7Zip调用器", "7-Zip进程启动失败");
                ReportLog("失败", "7Zip调用器", "已终止调用");
            }
        }

        public void Dispose()
        {
            _7ZipProcess?.Dispose();
            CheckCancellationTokenTimer?.Dispose();
            ReportLog = delegate { };
            ProcessExited = delegate { };
            GC.SuppressFinalize(this);
        }
        static void Example()
        {
            CancellationTokenSource cancellationTokenSource = new();
            SevenZipInvokerClass invoke7Zip = new(cancellationTokenSource);
            invoke7Zip.ReportLog += (Type, Sender, Log) =>
            {
                Console.WriteLine(Log);
            };
            invoke7Zip.ProcessExited += () =>
            {
                invoke7Zip.Dispose();

                //可执行其它操作
            };
            // 调用压缩方法
            invoke7Zip.Invoke7Zip("a -r -aoa -t7z -mx5 -bsp1", "D:\\Desktop\\output.7z", "D:\\Desktop\\Server");
        }
    }
}
//操作模式
//a: 添加文件到压缩包
//b: 性能测试
//d: 从压缩包中删除文件
//e: 解压文件（不保留目录结构）
//h: 计算文件哈希值
//i: 显示支持格式信息
//l: 列出压缩包内容
//rn: 重命名压缩包内文件
//t: 测试压缩包完整性
//u: 更新文件到压缩包
//x: 完整路径解压文件
//<开关>
//  -- : 停止解析开关和@列表文件
//  -ai[r[-|0]][m[-|2]][w[-]]{@列表文件|!通配符} : 包含其他压缩包
//  -ax[r[-|0]][m[-|2]][w[-]]{@列表文件|!通配符} : 排除其他压缩包
//  -ao{a|s|t|u} : 设置覆盖模式（覆盖 / 跳过 / 重命名 / 更新）
//  -an : 禁用压缩包名称字段
//  -bb[0-3] : 设置日志输出级别
//  -bd : 禁用进度指示器
//  -bs{o|e|p}{0|1|2} : 设置输出流（标准输出 / 错误 / 进度）
//  -bt : 显示执行时间统计
//  -i[r[-|0]][m[-|2]][w[-]]{@列表文件|!通配符} : 包含文件
//  -m{参数} : 设置压缩方法
//    -mmt[N] : 设置CPU线程数
//    -mx[N] : 设置压缩级别 ： -mx1（最快）... -mx9（最强）
//  -o{目录} : 设置输出目录
//  -p{密码} : 设置密码
//  -r[-|0] : 递归子目录搜索
//  -sa{a|e|s} : 设置压缩包命名模式
//  -scc{UTF-8|WIN|DOS} : 设置控制台输入/ 输出字符集
//  -scs{UTF-8|UTF-16LE|UTF-16BE|WIN|DOS|{id}} : 设置列表文件字符集
//  -scrc[CRC32|CRC64|SHA256|SHA1|XXH64|*] : 设置哈希算法（用于x / e / h命令）
//  -sdel : 压缩后删除源文件
//  -seml[.] : 通过邮件发送压缩包
//  -sfx[{名称}] : 创建自解压包
//  -si[{名称}] : 从标准输入读取数据
//  -slp : 启用大内存页模式
//  -slt : 显示技术信息（配合l命令）
//  -snh : 保留硬链接
//  -snl : 保留符号链接
//  -sni : 保留NT安全信息
//  -sns[-] : 保留NTFS备用数据流
//  -so : 输出到标准输出
//  -spd : 禁用文件名通配符匹配
//  -spe : 解压时消除根目录重复
//  -spf[2] : 使用完整文件路径
//  -ssc[-] : 启用大小写敏感模式
//  -sse : 无法打开文件时停止创建压缩包
//  -ssp : 不更改源文件最后访问时间
//  -ssw : 压缩已打开的文件
//  -stl : 使用最新修改时间设置压缩包时间戳
//  -stm{十六进制掩码} : 设置CPU线程亲和掩码
//  -stx{类型} : 排除指定类型压缩包
//  -t{类型} : 设置压缩包类型
//  -u[-][p#][q#][r#][x#][y#][z#][!新压缩包名] : 更新选项
//  -v{大小}[b|k|m|g] : 分卷压缩
//  -w[{路径}] : 设置工作目录（空路径表示临时目录）
//  -x[r[-|0]][m[-|2]][w[-]]{@列表文件|!通配符} : 排除文件
//  -y : 对所有询问自动回答"是"