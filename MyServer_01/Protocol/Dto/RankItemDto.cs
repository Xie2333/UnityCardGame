using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
     
    [Serializable]
    public class RankItemDto
    {
        public string UserName;
        public int CoinCount;

        public RankItemDto(string userName,int coinCount)
        {
            UserName = userName;
            CoinCount = coinCount;
        }
        public void Change(string userName, int coinCount)
        {
            UserName = userName;
            CoinCount = coinCount;
        }
    }
}
