using GameServer.Database;
using MyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache
{
    /// <summary>
    /// 匹配缓存层
    /// </summary>
    public  class MatchCache
    {
        /// <summary>
        /// 正在匹配的用户ID和房间ID的映射字典
        /// </summary>
        public Dictionary<int, int> userIdRoomIdDic = new Dictionary<int, int>();
        /// <summary>
        /// 正在匹配的房间ID与之对应的房间数据模型之间的映射字典
        /// </summary>
        public Dictionary<int, MatchRoom>roomIdModelDic = new Dictionary<int, MatchRoom>();
        /// <summary>
        /// 重用房间队列
        /// </summary>
        public Queue<MatchRoom> roomQueue = new Queue<MatchRoom>();

        /// <summary>
        /// 线程安全的房间ID
        /// </summary>
        private ThreadSafeInt roomId = new ThreadSafeInt(-1);

        /// <summary>
        /// 进入匹配房间
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public MatchRoom Enter(ClientPeer client)
        {
            Console.WriteLine("MatchRoom Enter"+client.UserName + "进入房间");
            //先遍历正在匹配的房间数据模型有没有未满的房间，如果有，则加入进去
            foreach(var mr in roomIdModelDic.Values)
            {
                if (mr.IsFull())
                {
                    continue;
                }
                mr.Enter(client);
                userIdRoomIdDic.Add(client.Id, mr.roomId);
                Console.WriteLine(client.UserName + "进入房间"+mr.roomId);
                return mr;
            }
            //如果没有适合的房间，则直接生成一个房间加入
            MatchRoom room = null;
            if(roomQueue.Count > 0)
            {
                room = roomQueue.Dequeue();
            }
            else
            {
                room = new MatchRoom(roomId.Add_Get());
            }
            room.Enter(client);
            roomIdModelDic.Add(room.roomId,room);
            userIdRoomIdDic.Add(client.Id,room.roomId);
            Console.WriteLine(client.UserName + "进入房间" + room.roomId);
            return room;
        }
        /// <summary>
        /// 离开匹配房间
        /// </summary>
        /// <param name="userId"></param>
        public MatchRoom Leave(int userId)
        {
            int roomId = userIdRoomIdDic[userId];
            MatchRoom room = roomIdModelDic[roomId];
            room.Leave(DatabaseManager.GetClientPeerByUserId(userId));
            userIdRoomIdDic.Remove(userId);
            //如果玩家离开房间后为空 加入重用队列 并从匹配房间列表中移除
            if (room.isEmpty())
            {
                roomIdModelDic.Remove(roomId);
                roomQueue.Enqueue(room);
            }
            return room;
        }
        /// <summary>
        /// 判断玩家是否在匹配房间中
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsMatching(int userId)
        {
            return userIdRoomIdDic.ContainsKey(userId);
        }
        /// <summary>
        /// 获取玩家所在房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MatchRoom GetRoom(int userId)
        {
            int roomId = userIdRoomIdDic[userId];
            return roomIdModelDic[roomId];
        }
        /// <summary>
        /// 销毁房间，游戏开始时调用
        /// </summary>
        /// <param name="room"></param>
        public void DestoryRoom(MatchRoom room)
        {
            roomIdModelDic.Remove(room.roomId);
            foreach(var client in room.clientList)
            {
                userIdRoomIdDic.Remove(client.Id);
            }
            room.clientList.Clear();
            room.readyUIdList.Clear();
            roomQueue.Enqueue(room);
        }
    }
}
