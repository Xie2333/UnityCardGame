using MyServer;
using Protocol.Constant;
using Protocol.Dto;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 战斗房间
    /// </summary>
    public class FightRoom
    {

        /// <summary>
        /// 房间ID，唯一标识
        /// </summary>
        public int RoomId { get; set; }
        /// <summary>
        /// 玩家列表
        /// </summary>
        public List<PlayerDto> playerList;
        /// <summary>
        /// 牌库
        /// </summary>
        public CardLibrary cardLibrary;
        /// <summary>
        /// 回合管理类
        /// </summary>
        public RoundModel roundModel;
        /// <summary>
        /// 离开的玩家列表
        /// </summary>
        public List<int> LeaveUserIdList;
        /// <summary>
        /// 弃牌玩家列表
        /// </summary>
        public List<int> giveUpCardUserIdList;
        /// <summary>
        /// 顶注
        /// </summary>
        public int topStakes;
        /// <summary>
        /// 底注
        /// </summary>
        public int bottomStakes;
        /// <summary>
        /// 上一位玩家下注的数量
        /// </summary>
        public int LastPlayerStakesCount;
        /// <summary>
        /// 总下注数
        /// </summary>
        public int stakesSum;
        /// <summary>
        /// 庄家在玩家列表中的下标
        /// </summary>
        private int bankerIndex = -1;

        public FightRoom(int roomId, List<ClientPeer> clientPeers)
        {
            this.RoomId = roomId;
            playerList = new List<PlayerDto>();
            foreach (var client in clientPeers)
            {
                PlayerDto dto = new PlayerDto(client.Id, client.UserName);
                playerList.Add(dto);
            }
            cardLibrary = new CardLibrary();
            roundModel = new RoundModel();
            LeaveUserIdList = new List<int>();
            giveUpCardUserIdList = new List<int>();
            stakesSum = 0;
        }

        public void Init(List<ClientPeer> clienList)
        {
            stakesSum = 0;
            playerList.Clear();
            foreach (var client in clienList)
            {
                PlayerDto dto = new PlayerDto(client.Id, client.UserName);
                playerList.Add(dto);
            }

        }
        public void ResetPosition(int bankerId)
        {
            if (playerList[0].userId == bankerId)
            {
                PlayerDto dto = playerList[1];
                playerList[1] = playerList[2];
                playerList[2] = dto;
            }
            if (playerList[1].userId == bankerId)
            {
                PlayerDto dto = playerList[0];
                playerList[0] = playerList[2];
                playerList[2] = dto;
            }
            if (playerList[2].userId == bankerId)
            {
                PlayerDto dto = playerList[0];
                playerList[0] = playerList[1];
                playerList[1] = dto;
            }
        }
        /// <summary>
        /// 销毁房间 重置房间数据
        /// </summary>
        public void Destory()
        {
            playerList.Clear();
            cardLibrary.Init();
            roundModel.Init();
            LeaveUserIdList.Clear();
            giveUpCardUserIdList.Clear();
            stakesSum = 0;
            bankerIndex = -1;
        }
        /// <summary>
        /// 广播消息
        /// </summary>
        public void Broadcast(int opCode, int subCode, object value, ClientPeer exceptClient = null)
        {
            NetMsg msg = new NetMsg(opCode, subCode, value);
            //将msg转化为字节数组
            byte[] data = EncodeTool.EncodeMsg(msg);
            byte[] packet = EncodeTool.EncodePacket(data);
            foreach (var player in playerList)
            {
                //客户端连接对象
                ClientPeer client = Database.DatabaseManager.GetClientPeerByUserId(player.userId);
                if (client == exceptClient)
                {
                    continue;
                }
                client.SendMsg(packet);
            }
        }
        /// <summary>
        /// 是否离开房间
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsLeaveRoom(int userId)
        {
            return LeaveUserIdList.Contains(userId);
        }
        /// <summary>
        /// 是否弃牌
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsGiveUpCard(int userId)
        {
            return giveUpCardUserIdList.Contains(userId);
        }
        /// <summary>
        /// 轮换下注
        /// </summary>
        /// <returns>下一次下注的玩家ID</returns>
        public int Turn()
        {
            int currentUserId = roundModel.CurrentStakesUserId;
            int nextUserId = GetNextUserId(currentUserId);
            roundModel.Trun(nextUserId);
            return nextUserId;
        }
        /// <summary>
        /// 获取下一次下注的玩家ID
        /// </summary>
        /// <param name="currentId"></param>
        /// <returns></returns>
        private int GetNextUserId(int currentId)
        {
            for(int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].userId == currentId)
                {
                    //i == 2
                    if (i == playerList.Count - 1)
                    {
                        return playerList[0].userId;
                    }else
                    return playerList[i + 1].userId;
                }
            }
            return -1;
        }
        /// <summary>
        /// 更新玩家下注总数
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="stakesCount"></param>
        public int UpdatePlayerStakesSum(int userId,int stakesCount)
        {
            foreach(var player in playerList)
            {
                if(player.userId == userId)
                {
                    player.stakeSum += stakesCount;
                    return player.stakeSum;
                }
            }
            return 0;
        }
        /// <summary>
        /// 选择庄家
        /// </summary>
        public ClientPeer SetBanker()
        {
            Random rand = new Random();
            int ranIndex = rand.Next(0, playerList.Count);
            bankerIndex = ranIndex;
            int userId = playerList[ranIndex].userId;
            playerList[ranIndex].identity = Identity.Banker;
            //默认庄家为默认下注者
            roundModel.Start(userId);
            ClientPeer bankerClient = Database.DatabaseManager.GetClientPeerByUserId(userId);
            string userName = bankerClient.UserName;
            Console.WriteLine("庄家为：" + userName);
            return bankerClient;
        }
        /// <summary>
        /// 发牌方法 
        /// </summary>
        public void DealCards()
        {
            for (int i = 0; i < 9; i++)
            {
                playerList[bankerIndex].AddCard(cardLibrary.DealCard());
                bankerIndex++;
                if (bankerIndex > playerList.Count - 1)
                {
                    bankerIndex = 0;
                }
            }
        }
        /// <summary>
        /// 对牌排序
        /// </summary>
        /// <param name="cardList"></param>
        private void SortCard(ref List<CardDto> cardList)
        {
            for (int i = 0; i < cardList.Count - 1; i++)
            {
                for (int j = 0; j < cardList.Count - 1 - i; j++)
                {
                    CardDto temp = cardList[j];
                    cardList[j] = cardList[j + 1];
                    cardList[j + 1] = temp;
                }
            }
        }
        /// <summary>
        /// 对房间内所有玩家进行排序
        /// </summary>
        public void SortAllPlayerCard()
        {
            foreach (var player in playerList)
            {
                SortCard(ref player.cardList);
            }
        }
        /// <summary>
        /// 获取所有玩家牌型
        /// </summary>
        public void GetAllPlayerCardType()
        {
            foreach(var player in playerList)
            {
                player.cardType = GetCardType(player.cardList);
            }
        }

        /// <summary>
        /// 获取牌型
        /// </summary>
        /// <param name="cardList"></param>
        private CardType GetCardType(List<CardDto> cardList)
        {
            CardType temp = CardType.None;
            //532
            if (cardList[0].weight == 5 && cardList[1].weight == 3 && cardList[2].weight == 2)
            {
                temp = CardType.Max;
            }
            //baozi
            if (cardList[0].weight == cardList[1].weight && cardList[0].weight == cardList[2].weight)
            {
                temp = CardType.Baozi;
            }
            else
            //Shunjin
            if (cardList[0].color == cardList[1].color && cardList[0].color == cardList[2].color
                && cardList[0].weight == cardList[1].weight + 1 && cardList[0].weight == cardList[2].weight + 2
                )
            {
                temp = CardType.Shunjin;
            }
            else
            //Jinhua
            if (cardList[0].color == cardList[1].color && cardList[0].color == cardList[2].color)
            {
                temp = CardType.Jinhua;
            }
            else
            //Shunzi
            if (cardList[0].weight == cardList[1].weight + 1 && cardList[0].weight == cardList[2].weight + 2)
            {
                temp = CardType.Shunzi;
            }
            else
            //Duizi
            if (cardList[0].weight == cardList[1].weight || cardList[1].weight == cardList[2].weight)
            {
                temp = CardType.Duizi;
            }
            else
            {
                temp = CardType.Min;
            }

            return temp;
        }
    }
}
