using MyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.login
{
    public interface IHandler
    {
        //断开连接
        void Disconnect(ClientPeer client);

        //接收方法
        void Receive(ClientPeer client, int subCode, object value);

    }
}
