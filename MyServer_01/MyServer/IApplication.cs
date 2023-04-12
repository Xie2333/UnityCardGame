using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    public interface IApplication
    {
        //断开连接
        void Disconnect(ClientPeer client);
        //接收数据
        void Receive(ClientPeer client, NetMsg msg);
    }
}
