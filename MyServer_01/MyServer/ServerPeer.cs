using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    public class ServerPeer
    {
        //服务器Socket
        private Socket serverSocket;
        //计量器
        private Semaphore semaphore;

        //客户端对象连接池
        private ClientPeerPool clientPeerPool;

        //应用层
        //定义接口
        private IApplication application;
        //设置应用层
        public void SetAppLication(IApplication application)
        {
            this.application = application;
        }

        //开启服务器
        public void StartServer(string ip, int port, int maxClient)
        {
            try
            {
                //创建连接池
                clientPeerPool = new ClientPeerPool(maxClient);
                for (int i = 0; i < maxClient; i++)
                {
                    ClientPeer temp = new ClientPeer();
                    //将ReceiveProcessCompleted方法 注册进 委托
                    temp.receiveCompleted = ReceiveProcessCompleted;

                    temp.ReceiveArgs.Completed += ReceiveArgs_Completed;
                    clientPeerPool.Enqueue(temp);
                }

                //用来管理线程拥塞控制
                semaphore = new Semaphore(maxClient, maxClient);

                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定到对应到IP和端口号的进程
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                //最大的监听数
                serverSocket.Listen(maxClient);
                Console.WriteLine("服务器启动成功");
                StartAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //接收客户端连接
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += E_Completed;
            }
            //异步接收客户端连接
            //result为true则正在接收客户端连接 连接成功后会调用Completed事件
            //result为false，则接收成功
            bool result = serverSocket.AcceptAsync(e);
            if (result == false)
            {
                ProcessAccept(e);
            }
        }
        //处理连接请求
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            //控制线程
            semaphore.WaitOne();
            //获取客户端socket，这是虚拟的客户端，只是负责数据通信的客户端
            //调用用户池出队列方法
            ClientPeer client = clientPeerPool.Dequeue();

            client.clientSocket = e.AcceptSocket;

            Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端连接成功");

            //接收消息
            StartReceive(client);
            e.AcceptSocket = null;
            //循环监听客户端连接
            StartAccept(e);
        }
        //异步接收客户端的连接完成后触发
        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        //接收消息
        private void StartReceive(ClientPeer client)
        {
            try
            {
                //开始一个异步请求，以便接收来自连接的数据
                bool result = client.clientSocket.ReceiveAsync(client.ReceiveArgs);
                if (result == false)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        //异步接收数据完成后的调用
        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }
        //处理接收的方法
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            //e.UserToken获取或设置与异步套接字操作相关联的用户或者应用程序对象
            ClientPeer client = e.UserToken as ClientPeer;
            //判断数据是否接收成功
            //client.ReceiveArgs.BytesTransferred获取套接字上的字节数
            //client.ReceiveArgs.SocketError==SocketError.Success接收成功
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                //目标缓冲区 client.ReceiveArgs.BytesTransferred传输的字节数
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];
                //将指定数目的字节从起始于特定偏移量的源数组复制到起始于特定偏移量的目标数组
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);
                //将接收到的数组拷贝到packet中

                //让ClientPeer自身处理接收到的数据
                client.ProcesReceive(packet);
                //调用伪递归
                StartReceive(client);
            }
            //断开连接
            else
            {
                //没有传输字节数，代表断开连接
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    //表示客户端主动断开连接
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        Disconnet(client, "客户端主动断开连接");
                    }
                    //因为网络异常被动断开连接
                    else
                    {
                        Disconnet(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }
        //一条消息处理完成后的回调
        private void ReceiveProcessCompleted(ClientPeer client, NetMsg msg)
        {
            //交给应用层处理这个消息
            application.Receive(client, msg);
        }

        //断开连接的方法
        private void Disconnet(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("客户端为空，无法断开连接");
                }
                //输出断开连接的端口号
                Console.WriteLine(client.clientSocket.RemoteEndPoint + " 客户端断开连接 ：原因：" + reason);

                //
                application.Disconnect(client);

                //让客户端自己处理断开连接
                client.Disconnect();

                //对象池回收断开的客户端
                clientPeerPool.Enqueue(client);
                //计量器释放一个用户数
                semaphore.Release();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
