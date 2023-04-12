using Protocol.Constant;
using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    /// <summary>
    /// 玩家传输模型
    /// </summary>
    [Serializable]
    public class PlayerDto
    {
        public int userId { get; set; }
        public string userName { get; set; }

        public int stakeSum { get; set; }

        public Identity identity { get; set; }

        public List<CardDto> cardList;

        public CardType cardType { get; set; }

        public PlayerDto(int userId, string userName)
        {
            this.userId = userId;
            this.userName = userName;
            stakeSum = 0;
            identity = Identity.Normal;
            cardList = new List<CardDto>();
            cardType = CardType.None;
        }
        /// <summary>
        /// 添加卡牌
        /// </summary>
        /// <param name="dto"></param>
        public void AddCard(CardDto dto)
        {
            cardList.Add(dto);
        }
        /// <summary>
        /// 移除卡牌
        /// </summary>
        /// <param name="dto"></param>
        public void RemoveCard(CardDto dto)
        {
            cardList.Remove(dto);
        }
    }
}
