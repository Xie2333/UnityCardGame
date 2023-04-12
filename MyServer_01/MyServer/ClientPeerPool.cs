using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    //客户端对象连接池
    public class ClientPeerPool
    {
        private Queue<ClientPeer> clientPeerQueue;
        public ClientPeerPool(int maxCount)
        {
            clientPeerQueue = new Queue<ClientPeer>(maxCount);
        }
        //入队列
        public void Enqueue(ClientPeer client)
        {
            clientPeerQueue.Enqueue(client);
        }
        //出队列
        public ClientPeer Dequeue()
        {
            return clientPeerQueue.Dequeue();
        }
    }
}

