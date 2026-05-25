using System.Net.Sockets;
using System.Security.Cryptography;

namespace PMCSsE_Communicator
{
    internal class ClientInfo
    {
        internal ClientInfo( TcpClient tcpClient, NetworkStream networkStream)
        {
            TcpClient = tcpClient;
            NetworkStream = networkStream;
        }
        internal HandShakeProcess_Server HandShakeProcess=HandShakeProcess_Server.Beginning;
        internal TcpClient TcpClient;
        internal NetworkStream NetworkStream;
        internal CancellationTokenSource CloseConnectionTokenSource=new();
        internal Aes? Aes;
        /// <summary>
        /// 默认设置下不允许递归
        /// </summary>
        internal Lock TasksQueueLock = new();
        internal Queue<Func<Task>> TasksQueue = [];
    }
}
