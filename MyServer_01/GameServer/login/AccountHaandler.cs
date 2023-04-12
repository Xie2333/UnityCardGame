using MyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol.Code;
using Protocol.Dto;
using GameServer.Database;
using System.Runtime.InteropServices;

namespace GameServer.login
{
    public class AccountHaandler : IHandler
    {
        public void Disconnect(ClientPeer client)
        {
            Console.WriteLine("断开连接");
            DatabaseManager.OffLine(client);
        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {

                case AccountCode.Register_CREQ:
                    Console.WriteLine("Register_CREQ");
                    Register(client, value as AccountDto);
                    break;
                case AccountCode.Login_CREQ:
                    Console.WriteLine("Login_CREQ");
                    Login(client, value as AccountDto);
                    break;
                case AccountCode.GetUserInfo_CREQ:
                    Console.WriteLine("GetUserInfo_CREQ");
                    GetUserInfo(client);
                    break;
                case AccountCode.GetRankList_CREQ:
                    Console.WriteLine("GetRankList_CREQ");
                    GetRankList(client);
                    break;
                case AccountCode.UpdateCoinCount_CREQ:
                    Console.WriteLine("UpdateCoinCount_CREQ");
                    UpdateCoinCount(client, (int)value);
                    break;
 
                default:
                    break;
            }
        }
        /// <summary>
        /// 客户端发送更新金币数量的请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="coinCount"></param>
        private void UpdateCoinCount(ClientPeer client,int coinCount)
        {
            SingleExecute.Instance.Execute(() =>
            {
                int count = DatabaseManager.UpdateCoinCount(client.Id, coinCount);
                client.SendMsg(OpCode.Account,AccountCode.UpdateCoinCount_SRES,count);
            });
        }

        /// <summary>
        /// 客户端获取用户信息的请求
        /// </summary>
        /// <param name="client"></param>
        private void GetUserInfo(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                UserDto dto = DatabaseManager.CreateUserDto(client.Id);
                //发送用户信息
                client.SendMsg(OpCode.Account, AccountCode.GetUserInfo_SRES, dto);

            });
        }

        /// <summary>
        /// 客户端注册处理
        /// 该函数执行时如果多线程，则不会发送信息
        /// </summary>
        /// <param name="dto"></param>
        private void Register(ClientPeer client, AccountDto dto)
        {
            //单线程执行
            //防止多个线程同时访问数据出错
            SingleExecute.Instance.Execute(() =>
            {
                //判断用户名是否被注册了
                if (DatabaseManager.IsExistUserName(dto.userName))
                {
                    Console.WriteLine("Register用户名被注册了");
                    client.SendMsg(OpCode.Account, AccountCode.Register_SRES, -1);
                    return;
                }
                Console.WriteLine("Register用户名未注册了");
                //可以进行注册
                DatabaseManager.CreateUser(dto.userName, dto.password);
                //向客户端发送信息
                client.SendMsg(OpCode.Account, AccountCode.Register_SRES, 0);
            });
        }

        /// <summary>
        /// 客户端登录请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dto"></param>
        private void Login(ClientPeer client,AccountDto dto)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (DatabaseManager.IsExistUserName(dto.userName) == false)
                {
                    Console.WriteLine("Login用户名不存在");
                    //用户名不存在
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -1);
                    return;
                }
                if (DatabaseManager.IsMatch(dto.userName, dto.password)==false)
                {
                    Console.WriteLine("Login密码错误");
                    //密码错误
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -2);
                    return;
                }

                if (DatabaseManager.IsOnline(dto.userName))
                {
                    Console.WriteLine("Login该用户在线");
                    //该用户在线
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -3);
                    return;
                }
                Console.WriteLine("Login登录成功");
                //登录成功
                DatabaseManager.Login(dto.userName, client);
                client.SendMsg(OpCode.Account, AccountCode.Login_SRES, 0);
            }
            );
        }

        /// <summary>
        /// 客户端请求排行榜数据处理
        /// </summary>
        /// <param name="client"></param>
        private void GetRankList(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                RankListDto dto = DatabaseManager.GetRankListDto();

                client.SendMsg(OpCode.Account, AccountCode.GetRankList_SRES, dto);
            });
        }
    }
}

