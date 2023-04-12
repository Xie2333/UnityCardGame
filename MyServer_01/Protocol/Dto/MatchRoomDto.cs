using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    /// <summary>
    /// 匹配房间传输模型
    /// </summary>
    [Serializable]
    public class MatchRoomDto
    {
        /// <summary>
        /// 存放用户id和userDto的映射
        /// </summary>
        public Dictionary<int,UserDto> userIdUserDtoDic { get; private set;}
        /// <summary>
        /// 准备的用户id
        /// </summary>
        public List<int> readyUserIdList { get;set;}
        /// <summary>
        /// 进入房间顺序的用户ID列表
        /// </summary>
        public List<int> enterOrderUserList { get;private set;}
        /// <summary>
        /// 左边玩家ID
        /// </summary>
        public int LeftPlayerId { get;private set;}
        /// <summary>
        /// 右边玩家ID
        /// </summary>
        public int RightPlayerId { get;private set;}

        public MatchRoomDto()
        {
            readyUserIdList = new List<int>();
            userIdUserDtoDic = new Dictionary<int,UserDto>();
            enterOrderUserList = new List<int>();
        }
        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="dto"></param>
        public void Enter(UserDto dto)
        {
            userIdUserDtoDic.Add(dto.UserId,dto);
            enterOrderUserList.Add(dto.UserId);
        }
        /// <summary>
        /// 离开房间
        /// </summary>
        /// <param name="userId"></param>
        public void Leave(int userId)
        {
            userIdUserDtoDic.Remove(userId);
            readyUserIdList.Remove(userId);
            enterOrderUserList.Remove(userId);
        }
        /// <summary>
        /// 准备方法
        /// </summary>
        public void Ready(int userId)
        {
            readyUserIdList.Add(userId);    
        }
        /// <summary>
        /// 取消准备
        /// </summary>
        /// <param name="userId"></param>
        public void UnReady(int userId)
        {
            readyUserIdList.Remove(userId);
        }
        /// <summary>
        /// 重置位置，为三个玩家排序
        /// </summary>
        /// <param name="myUserId"></param>
        public void ResetPosition(int myUserId)
        {
            RightPlayerId = -1;
            LeftPlayerId = -1; 
            
            if(enterOrderUserList.Count == 1)
            {
                return;
            }
            if(enterOrderUserList.Count == 2)
            {
                //x a 
                if (enterOrderUserList[0] == myUserId)
                {
                    RightPlayerId = enterOrderUserList[1];
                }
                //a x
                if (enterOrderUserList[1] == myUserId)
                {
                    LeftPlayerId = enterOrderUserList[0];
                }
            }
            if(enterOrderUserList.Count == 3)
            {
                //x a b
                if(enterOrderUserList[0] == myUserId)
                {
                    RightPlayerId=enterOrderUserList[1];
                    LeftPlayerId = enterOrderUserList[2];
                }
                //a x b
                if (enterOrderUserList[1] == myUserId)
                {
                    RightPlayerId = enterOrderUserList[2];
                    LeftPlayerId = enterOrderUserList[0];
                }
                //a b x
                if (enterOrderUserList[2] == myUserId)
                {
                    RightPlayerId = enterOrderUserList[0];
                    LeftPlayerId = enterOrderUserList[1];
                }
            }
        }










    }
}
