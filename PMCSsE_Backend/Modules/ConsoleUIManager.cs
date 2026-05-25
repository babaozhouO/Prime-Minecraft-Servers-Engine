//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PMCSsE_Backend.Modules
//{
//    internal static class ConsoleUIManager
//    {
//        private static string RSAPublicKeyHash = "";
//        private static readonly Queue<string> ConnectionLogs = [];
//        private static void RefreshConsoleUI()
//        {
//            Console.Clear();
//            Console.WriteLine($"RSA公钥指纹：{RSAPublicKeyHash}");
//            Console.WriteLine("连接日志：");
//            string[] logs;
//            lock (ConnectionLogs)
//            {
//                logs = [.. ConnectionLogs];
//            }
//            foreach (var log in logs)
//            {
//                Console.WriteLine(log);
//            }

//        }
//        internal static void SetRSAPublicKeyHash(string RSAQKH)
//        {
//            RSAPublicKeyHash = RSAQKH;
//            RefreshConsoleUI();
//        }
//        internal static void AppendConnectionLog(string log)
//        {
//            lock (ConnectionLogs)
//            {
//                ConnectionLogs.Enqueue(log);
//                while (ConnectionLogs.Count > 20)
//                {
//                    ConnectionLogs.Dequeue();
//                }
//            }
//            RefreshConsoleUI();
//        }
//    }
//}
