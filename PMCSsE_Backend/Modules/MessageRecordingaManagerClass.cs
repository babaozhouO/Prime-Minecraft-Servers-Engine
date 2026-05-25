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
using PMCSsE_Communicator;
using System.Text;

namespace PMCSsE_Backend.Modules
{
    /// <summary>
    /// 聊天消息记录管理器类，用于管理特定MC服务器的聊天消息记录及其索引文件。
    /// </summary>
    internal class MessageRecordingsManagerClass
    {
        /// <summary>
        /// 当前MC服务器的配置信息。
        /// </summary>
        private readonly MCServerManagerConfig SingleMCServerManagerConfigInfo;

        /// <summary>
        /// 当前MC服务器聊天消息记录文件的完整路径。
        /// </summary>
        private readonly string ThisMCServerChatMessageRecordingsFilePath;

        /// <summary>
        /// 当前MC服务器聊天消息记录索引文件的完整路径。
        /// </summary>
        private readonly string ThisMCServerChatMessageRecordingsIndexFilePath;

        /// <summary>
        /// 存储每条消息记录在文件中的结束位置列表，用于快速定位和读取消息。
        /// </summary>
        private readonly List<long> EachMessageRecordingEndingPositionList;

        internal event Action<string, string, string> ReportLog = delegate { };

        internal bool ManagerStarted = false;
        /// <summary>
        /// 初始化聊天消息记录管理器实例。
        /// 根据提供的服务器配置信息构建消息记录和索引文件路径，并尝试加载已有的索引数据。
        /// 若目录不存在则创建目录，若索引文件存在则读取索引内容。
        /// </summary>
        internal MessageRecordingsManagerClass(MCServerManagerConfig singleMCServerManagerConfigInfo)
        {
            SingleMCServerManagerConfigInfo = singleMCServerManagerConfigInfo;
            ThisMCServerChatMessageRecordingsFilePath = Path.Combine(Paths.MessageRecordingsDir, $"MCServer - {SingleMCServerManagerConfigInfo.ManagerID}", "MeaasageRecordings.json");
            ThisMCServerChatMessageRecordingsIndexFilePath = Path.Combine(Paths.MessageRecordingsDir, $"MCServer - {SingleMCServerManagerConfigInfo.ManagerID}", "MeaasageRecordings.json.index");
            EachMessageRecordingEndingPositionList = [];
        }

        internal void Initialize()
        {
            ReportLog("信息", "聊天记录管理器", $"聊天记录存储文件路径:{ThisMCServerChatMessageRecordingsFilePath}");
            ReportLog("信息", "聊天记录管理器", $"聊天记录索引文件路径:{ThisMCServerChatMessageRecordingsIndexFilePath}");


            string serverRecordingsDirectory = Path.Combine(Paths.MessageRecordingsDir, $"MCServer - {SingleMCServerManagerConfigInfo.ManagerID}");

            try
            {
                if (!Directory.Exists(serverRecordingsDirectory))
                {
                    Directory.CreateDirectory(serverRecordingsDirectory);
                    ReportLog("信息", "聊天记录管理器", $"聊天记录存储目录不存在，已创建[{serverRecordingsDirectory}]");
                }
            }
            catch (Exception ex)
            {
                ReportLog("错误", "聊天记录管理器", $"创建目录时发生异常：{ex.Message}");
                return;
            }
            try
            {
                // 如果聊天记录文件和索引文件都存在，则尝试读取索引文件内容
                if (File.Exists(ThisMCServerChatMessageRecordingsFilePath) && File.Exists(ThisMCServerChatMessageRecordingsIndexFilePath))
                {
                    // 逐行读取索引文件内容并转换为long类型添加到列表中
                    List<string> strings = [.. File.ReadAllLines(ThisMCServerChatMessageRecordingsIndexFilePath)];
                    foreach (string s in strings)
                    {
                        EachMessageRecordingEndingPositionList.Add(Convert.ToInt64(s));
                    }
                }
            }
            catch (Exception ex)
            {
                ReportLog("错误", "聊天记录管理器", $"读取索引文件时发生异常:{ex.Message}");
                return;
            }
            ManagerStarted = true;

        }

        internal void AppendMessageRecording(ChatMessageClass ChatMessage)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(ChatMessage, JsonComputeOptions.jsonSerializerOptions1) + Environment.NewLine;
            try
            {
                File.AppendAllText(ThisMCServerChatMessageRecordingsFilePath, json);
                ReportLog("调试", "聊天记录管理器", $"已写入消息，索引：{ChatMessage.Index}，json文本：{json.Replace("\n","").Replace("\r", "")}");
            }
            catch (Exception ex)
            {
                ReportLog("错误", "聊天记录管理器", $"写入消息json时发生异常:{ex.Message}");
            }
            long MessageRecordingBytesLenth = (EachMessageRecordingEndingPositionList.Count == 0 ? 0 : EachMessageRecordingEndingPositionList[^1] + 1) + Encoding.UTF8.GetBytes(json).LongLength;
            long MessageRecordingEndingPosition = MessageRecordingBytesLenth - 1;
            EachMessageRecordingEndingPositionList.Add(MessageRecordingEndingPosition);
            try
            {
                if (File.Exists(ThisMCServerChatMessageRecordingsIndexFilePath))
                {
                    File.AppendAllText(ThisMCServerChatMessageRecordingsIndexFilePath, $"\n{MessageRecordingEndingPosition}");
                    ReportLog("调试", "聊天记录管理器", $"成功写入索引");
                }
                else
                {
                    File.AppendAllText(ThisMCServerChatMessageRecordingsIndexFilePath, $"{MessageRecordingEndingPosition}");
                    ReportLog("调试", "聊天记录管理器", $"成功写入索引");
                }

            }
            catch (Exception ex)
            {
                ReportLog("错误", "聊天记录管理器", $"写入消息索引时发生异常:{ex.Message}");
            }
        }

        internal int GetMessagesCount()
        {
            return EachMessageRecordingEndingPositionList.Count;
        }
        internal ChatMessageClass? ReadMessage(int index)
        {
            if (EachMessageRecordingEndingPositionList.Count - 1 < index || EachMessageRecordingEndingPositionList.Count == 0)
            {
                ReportLog("错误", "聊天记录管理器", "尝试读取不存在的消息或索引损坏");
                return null;
            }
            long StartPosition;
            if (index == 0)
            {
                StartPosition = 0;
            }
            else
            {
                StartPosition = EachMessageRecordingEndingPositionList[index - 1] + 1;
            }
            long EndPosition = EachMessageRecordingEndingPositionList[index];
            byte[] buffer = new byte[EndPosition - StartPosition + 1];
            FileStream? fileStream = null;
            try
            {
                fileStream = new(ThisMCServerChatMessageRecordingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                {
                    Position = StartPosition
                };
                int BytesRead = 0;
                while (true)
                {
                    BytesRead += fileStream.Read(buffer, BytesRead, buffer.Length - BytesRead);
                    if (BytesRead == buffer.Length)
                    {
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                ReportLog("错误", "聊天记录管理器", $"读取消息时发生异常:{ex.Message}");
            }
            finally
            {
                fileStream?.Dispose();
            }
            string json = Encoding.UTF8.GetString(buffer).TrimEnd('\r', '\n');

            ChatMessageClass? chatMessage = System.Text.Json.JsonSerializer.Deserialize<ChatMessageClass>(json, JsonComputeOptions.jsonSerializerOptions1);

            return chatMessage;
        }
        internal List<ChatMessageClass>? ReadMessage(int startindex, int endindex)
        {
            if (startindex < 0 || EachMessageRecordingEndingPositionList.Count == 0)
            {
                ReportLog("错误", "聊天记录管理器", "尝试读取不存在的消息或索引损坏");
                return null;
            }
            if (endindex > EachMessageRecordingEndingPositionList.Count - 1)
            {
                endindex = EachMessageRecordingEndingPositionList.Count - 1;
            }
            List<ChatMessageClass> chatMessages = [];
            FileStream fileStream = new(ThisMCServerChatMessageRecordingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            for (int index = startindex; index <= endindex; index++)
            {
                long StartPosition;
                if (index == 0)
                {
                    StartPosition = 0;
                }
                else
                {
                    StartPosition = EachMessageRecordingEndingPositionList[index - 1] + 1;
                }
                long EndPosition = EachMessageRecordingEndingPositionList[index];
                byte[] buffer = new byte[EndPosition - StartPosition + 1];
                try
                {
                    fileStream.Position = StartPosition;
                    int BytesRead = 0;
                    while (true)
                    {
                        BytesRead += fileStream.Read(buffer, BytesRead, buffer.Length - BytesRead);
                        if (BytesRead == buffer.Length)
                        {
                            break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    ReportLog("错误", "聊天记录管理器", $"读取消息[索引为{index}]时发生异常:{ex.Message}");
                }
                string json = Encoding.UTF8.GetString(buffer).TrimEnd('\r', '\n');
                ChatMessageClass? chatMessage = System.Text.Json.JsonSerializer.Deserialize<ChatMessageClass>(json, JsonComputeOptions.jsonSerializerOptions1);
                if (chatMessage == null) { continue; }
                chatMessages.Add(chatMessage);
            }
            fileStream.Dispose();
            return chatMessages;
        }

        internal void Dispose()
        {
            EachMessageRecordingEndingPositionList.Clear();
            ReportLog = delegate { };
        }
    }
}
