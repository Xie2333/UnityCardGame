using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto.Fight
{
    /// <summary>
    /// 卡牌传输模型
    /// </summary>
    [Serializable]
    public class CardDto
    {
        public string cardName { get; set; }
        public int weight { get; set; }
        public int color { get; set; }

        public CardDto(string cardName, int weight, int color)
        {
            this.cardName = cardName;
            this.weight = weight;
            this.color = color;
        }
    }
}
