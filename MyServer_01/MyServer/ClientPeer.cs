using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    public class ClientPeer
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        //public string Password { get; set; }

        public Socket clientSocket { get; set; }

        //创建异步套节字(Socket)对象
        //接收的异步套节字操作
        public SocketAsyncEventArgs ReceiveArgs { get; set; }

        public ClientPeer()
        {
            msg = new NetMsg();
            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.UserToken = this;
            //设置数据缓冲区
            ReceiveArgs.SetBuffer(new byte[2048], 0, 2048);
        }
        //接收到消息之后，存储到数据缓存区
        private List<byte> cache = new List<byte>();
        //是否正在处理接收的数据
        private bool isProcessingReceive = false;

        //消息处理完成后的委托
        //回调给服务器的委托(那个客户端，消息对象)
        public delegate void ReceiveCompleted(ClientPeer client, NetMsg msg);

        public ReceiveCompleted receiveCompleted;

        //ClientPeer处理数据方法
        public void ProcesReceive(byte[] packet)
        {
            cache.AddRange(packet);
            if (isProcessingReceive == false)
            {
                ProcessData();
            }
        }
        //处理数据方法
        private void ProcessData()
        {
            isProcessingReceive = true;
            //解析包，从缓冲区中取出一个完整的包
            byte[] packet = EncodeTool.DecodePacket(ref cache);
            if (packet == null)
            {
                isProcessingReceive = false;
                return;
            }
            //拿到包转化为NetMsg
            NetMsg msg = EncodeTool.DecodeMsg(packet);
            //拿到类后需要回调给服务器端
            if (receiveCompleted != null)
            {
                receiveCompleted(this, msg);
            }
            //伪递归，继续取出数据
            ProcessData();
        }

        //客户端断开连接
        public void Disconnect()
        {
            cache.Clear();
            isProcessingReceive = false;
            //停止掉发送和接收
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            clientSocket = null;
        }

        private NetMsg msg;
        //发送数据
        public void SendMsg(int opCode, int subCode, object value)
        {
            msg.Change(opCode, subCode, value);
            //将msg转化为字节数组
            byte[] data = EncodeTool.EncodeMsg(msg);
            //把数据构造为包
            byte[] packet = EncodeTool.EncodePacket(data);
            SendMsg(packet);
        }
        public void SendMsg(byte[] packet)
        {
            try
            {
                Console.WriteLine("向客户端发送信息");
                clientSocket.Send(packet);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
