using Protocol.Dto.Fight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Cache.Fight
{
    /// <summary>
    /// 牌库
    /// </summary>
    public class CardLibrary
    {
        private Queue<CardDto> cardQueue = new Queue<CardDto>();

        public CardLibrary()
        {
            //初始化牌
            Initcard();
            Shuffle();
        }
        //初始化牌
        private void Initcard()
        {
            cardQueue.Clear();
            for(int color = 0; color < 4; color++)
            {
                for(int weight = 2;weight < 15; weight++)
                {
                    string cardName = "card_" + color + "_" + weight;

                    CardDto dto = new CardDto(cardName,weight,color);

                    cardQueue.Enqueue(dto);
                }
            }
        }
        /// <summary>
        /// 洗牌
        /// </summary>
        private void Shuffle()
        {
            List<CardDto> cardList = cardQueue.ToList<CardDto>();

            Random ran = new Random();
            for (int i = 0; i < cardList.Count; i++)
            {
                int ranValue = ran.Next(0, cardList.Count);
                CardDto temp = cardList[i];
                cardList[i] = cardList[ranValue];
                cardList[ranValue] = temp;
            }

            cardQueue.Clear();
            foreach(var card in cardList)
            {
                cardQueue.Enqueue(card);
            }
        }

        public void Init()
        {
            Initcard();
            Shuffle();
        }

        public CardDto DealCard()
        {
            if (cardQueue.Count < 9)
            {
                Init();
            }
            return cardQueue.Dequeue();
        }
    }
}
