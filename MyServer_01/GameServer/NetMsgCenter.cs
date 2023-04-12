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
    //网络消息处理中心，分发消息到定义的模块
    class NetMsgCenter : IApplication
    {
        private AccountHaandler accountHaandler = new AccountHaandler();
        private MatchHandler matchHandler = new MatchHandler();
        private ChatHandler chatHandler = new ChatHandler();
        private FightHandler fightHandler = new FightHandler();

        public NetMsgCenter()
        {
            //将fightHandler.StartFight注册到matchHandler.startFight中，当matchHandler.startFight调用时，将调用fightHandler.StartFight
            matchHandler.startFight += fightHandler.StartFight;
        }

        //断开连接
        public void Disconnect(ClientPeer client)
        {
            //与new顺序相反
            fightHandler.Disconnect(client);
            chatHandler.Disconnect(client);
            matchHandler.Disconnect(client);
            accountHaandler.Disconnect(client);

        }

        //接收消息
        public void Receive(ClientPeer client, NetMsg msg)
        {
            Console.WriteLine(msg.opCode);
            switch (msg.opCode)
            {
                case OpCode.Account:
                    accountHaandler.Receive(client, msg.subCode, msg.value);
                    break;
                case OpCode.Match:
                    matchHandler.Receive(client, msg.subCode, msg.value);
                    break;
                case OpCode.Chat:
                    chatHandler.Receive(client, msg.subCode, msg.value);
                    break;
                case OpCode.Fight:
                    fightHandler.Receive(client, msg.subCode, msg.value);
                    break;
                default:
                    break;
            }
        }
    }
}
