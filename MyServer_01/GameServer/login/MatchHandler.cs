using GameServer.Cache;
using GameServer.Database;
using MyServer;
using Protocol.Code;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.login
{
    /// <summary>
    /// 开始游戏的委托
    /// </summary>
    /// <param name="clientlist">开始游戏的玩家列表</param>
    /// <param name="roomType">房间类型</param>
    public delegate void StartFight(List<ClientPeer> clientlist,int roomType);

    public class MatchHandler : IHandler
    {
        /// <summary>
        /// 匹配房间缓存集合
        /// </summary>
        private List<MatchCache> matchCacheList = Caches.matchCacheList;
        /// <summary>
        /// 定义委托
        /// </summary>
        public StartFight startFight;

        public void Disconnect(ClientPeer client)
        {
            for(int i = 0; i < matchCacheList.Count; i++)
            {
                LeaveRoom(client, i);
            }
        }
         
        public void Receive(ClientPeer client, int subCode, object value)
        {
            Console.WriteLine("MatchHandler");
            switch (subCode)
            {
                case MatchCode.Enter_CREQ:
                    EnterRoom(client,(int)value);
                    break;
                case MatchCode.Leave_CREQ:
                    LeaveRoom(client,(int)value);
                    break;
                case MatchCode.Ready_CREQ:
                    Ready(client,(int)value);
                    break;
                case MatchCode.UnReady_CREQ:
                    UnReady(client, (int)value);
                    break;
                default:
                    break;

            }
        }
        /// <summary>
        /// 取消准备方法
        /// </summary>
        private void UnReady(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (matchCacheList[roomType].IsMatching(client.Id) == false) return;
                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.UnReady(client.Id);
                //广播当前准备玩家的ID
                room.Broadcast(OpCode.Match, MatchCode.UnReady_BRO, client.Id);
            });
        }
        /// <summary>
        /// 客户端连接对象发来的准备请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void Ready(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (matchCacheList[roomType].IsMatching(client.Id) == false) return;
                MatchRoom room = matchCacheList[roomType].GetRoom(client.Id);
                room.Ready(client.Id);
                //广播当前准备玩家的ID
                room.Broadcast(OpCode.Match, MatchCode.Ready_BRO, client.Id);

                //如果当前房间玩家全部准备了,可以开始游戏了
                if (room.IsAllReady())
                {
                    startFight(room.clientList, roomType);
                    //通知房间内的所有玩家要开始游戏了
                    room.Broadcast(OpCode.Match, MatchCode.StartGame_BRO, null);
                    //销毁准备房间
                    matchCacheList[roomType].DestoryRoom(room);
                }

            });
        }
        /// <summary>
        /// 客户端进入房间请求
        /// </summary>
        /// <param name="client"></param>
        /// <param name="roomType"></param>
        private void EnterRoom(ClientPeer client,int roomType)
        {
            Console.WriteLine("EnterRoom" +client.UserName + "进入房间");
            SingleExecute.Instance.Execute(() =>
            {
                //判断客户端是否在匹配房间中，在则忽略
                if (matchCacheList[roomType].IsMatching(client.Id)) return;

                MatchRoom room = matchCacheList[roomType].Enter(client);

                //构造UserDto，用户数据传输模型
                UserDto userDto = DatabaseManager.CreateUserDto(client.Id);
                //将新进入玩家广播给其他玩家,有新玩家进入，参数为新进玩家的UserDto
                room.Broadcast(OpCode.Match, MatchCode.Enter_BRO, userDto, client);

                //给客户端一个响应，参数：房间传输模型 包含房间内正在等待的玩家以及准备的玩家id集合
                client.SendMsg(OpCode.Match, MatchCode.Enter_SRES, MakeMatchRoomDto(room));

                if(roomType == 0)
                {
                    Console.WriteLine(userDto.UserName + "进入底注为10，顶注为100的房间");
                }
                if (roomType == 1)
                {
                    Console.WriteLine(userDto.UserName + "进入底注为20，顶注为200的房间");
                }
                if (roomType == 2)
                {
                    Console.WriteLine(userDto.UserName + "进入底注为50，顶注为500的房间");
                }
            });
        }

        private MatchRoomDto MakeMatchRoomDto(MatchRoom room)
        {
            MatchRoomDto dto = new MatchRoomDto();
            for(int i = 0; i < room.clientList.Count; i++)
            {
                dto.Enter(DatabaseManager.CreateUserDto(room.clientList[i].Id));
            }
            dto.readyUserIdList = room.readyUIdList;
            return dto;
        }
        /// <summary>
        /// 客户端离开的请求
        /// </summary>
        private void LeaveRoom(ClientPeer client,int roomType)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //不在匹配房间忽略
                if (matchCacheList[roomType].IsMatching(client.Id) == false) return;
                MatchRoom room = matchCacheList[roomType].Leave(client.Id);
                room.Broadcast(OpCode.Match,MatchCode.Leave_BRO,client.Id);
            });
        }
    }
}
