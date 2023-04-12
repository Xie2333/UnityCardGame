using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    /// <summary>
    /// 用户信息传输模型
    /// </summary>
    [Serializable]
    public class UserDto
    {
        public int UserId;
        public string UserName;
        public string IconName;
        public int CoinCount;

        public UserDto(int userId, string userName, string iconName, int coinCount)
        {
            UserId = userId;
            UserName = userName;
            IconName = iconName;
            CoinCount = coinCount;
        }
        public void Change(int userId, string userName, string iconName, int coinCount)
        {
            UserId = userId;
            UserName = userName;
            IconName = iconName;
            CoinCount = coinCount;
        }
    }
}
