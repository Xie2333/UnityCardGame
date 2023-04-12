using MyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol.Code;
using GameServer.login;
using System.Net.Http.Headers;

namespace GameServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServerPeer server = new ServerPeer();
            server.SetAppLication(new NetMsgCenter());
            server.StartServer("127.0.0.1", 6666,100);

            //数据库建立连接
            Database.DatabaseManager.StartConnect();

            Console.ReadKey();
        }
    }
}