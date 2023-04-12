using Microsoft.SqlServer.Server;
using MyServer;
using MySql.Data.MySqlClient;
using MySql.Data.MySqlClient.Memcached;
using MySqlX.XDevAPI.Common;
using Protocol.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Database
{
    public class DatabaseManager
    {
        private static MySqlConnection sqlConnect;

        private static Dictionary<int, ClientPeer> idClientDic;

        private static RankListDto rankListDto;


        public static void StartConnect()
        {
            idClientDic = new Dictionary<int, ClientPeer>();

            string conStr = "database=zjhgame;server=114.115.202.184;port=3306;user=root;password=123456;";
            //string conStr = "database=zjhgame;server=127.0.0.1;port=3306;user=root;password=1234;";

            sqlConnect = new MySqlConnection(conStr);

            try
            {
                sqlConnect.Open(); ;//建立连接，可能出现异常,使用try catch语句
                Console.WriteLine("已经建立连接");
                //在这里可以使用代码对数据库进行增删查改的操作
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);//有错则报出错误
            }

        }
        /// <summary>
        /// 判断是否存在该用户名
        /// </summary>
        public static bool IsExistUserName(string username)
        {
            rankListDto = new RankListDto();
            //创建sql语句
            MySqlCommand cmd = new MySqlCommand("select UserName from userinfo where UserName = @name", sqlConnect);
            //将@name替换为username
            cmd.Parameters.AddWithValue("name", username);

            //读取查询结果 ExecuteReader执行并查询
            MySqlDataReader reder = cmd.ExecuteReader();
            bool result = reder.HasRows;
            reder.Close();
            
            Console.WriteLine("IsExistUserName用户是否存在：" + result);

            return result;
        }
        /// <summary>
        /// 创建用户信息
        /// </summary>
        public static void CreateUser(string userNmae, string pwd)
        {
            //创建sql语句
            MySqlCommand cmd = new MySqlCommand("insert into userinfo set UserName=@name,Password=@pwd,Online=0,IconName=@iconName", sqlConnect);
            cmd.Parameters.AddWithValue("name", userNmae);
            cmd.Parameters.AddWithValue("pwd", pwd);
            Random ran = new Random();
            int index = ran.Next(0, 19);
            cmd.Parameters.AddWithValue("iconName", "headIcon_" + index.ToString());

            Console.WriteLine("CreateUser创建用户成功");

            //只需要执行，不需要查询
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 用户名密码是否匹配
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        public static bool IsMatch(string userName,string pwd)
        {
            MySqlCommand cmd = new MySqlCommand("select * from userinfo where UserName = @name", sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            //判断是否查询到数据
            if (reader.HasRows)
            {
                //读取一行
                reader.Read();
                //查找到的密码是否等于发送的密码
                bool result = (reader.GetString("Password") == pwd);
                reader.Close();

                Console.WriteLine("IsMatch查询用户成功");

                return result;
            }
            Console.WriteLine("IsMatch查询用户失败");
            reader.Close();
            return false;
        }
        /// <summary>
        /// 查询是否在线
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsOnline(string userName)
        {
            MySqlCommand cmd = new MySqlCommand("select Online from userinfo where UserName=@name", sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            //判断是否查询到数据
            if (reader.HasRows)
            {
                //读取一行
                reader.Read();
                bool result = reader.GetBoolean("Online");

                reader.Close();

                Console.WriteLine("IsOnline用户是否在线" + result);
                return result;
            }
            Console.WriteLine("IsOnline用户不存在");
            reader.Close();
            return false;
        }
        /// <summary>
        /// 登录上线
        /// </summary>
        /// <param name="userName"></param>
        public static void Login(string userName,ClientPeer client)
        {
            MySqlCommand cmd = new MySqlCommand("update userinfo set Online=1 where Username=@name",sqlConnect);
            cmd.Parameters.AddWithValue("name", userName);
            cmd.ExecuteNonQuery();

            MySqlCommand cmd1 = new MySqlCommand("select * from userinfo where UserName=@name", sqlConnect);
            cmd1.Parameters.AddWithValue("name", userName);

            MySqlDataReader reader = cmd1.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                int id = reader.GetInt32("Id");
                client.Id = id;
                client.UserName = userName;

                //判断id是否有加入字典里面
                if (idClientDic.ContainsKey(id) == false)
                {
                    idClientDic.Add(id, client);
                }
                reader.Close();
            }
            reader.Close(); 
        }
        /// <summary>
        /// 用户下线
        /// </summary>
        /// <param name="client"></param>
        public static void OffLine(ClientPeer client)
        {
            MySqlCommand cmd = new MySqlCommand("update userinfo set Online=0 where Username=@name", sqlConnect);
            cmd.Parameters.AddWithValue("name", client.UserName);
            cmd.ExecuteNonQuery();
            if (idClientDic.ContainsKey(client.Id))
            {
                idClientDic.Remove(client.Id);
            }
        }

        /// <summary>
        /// 使用用户id获取客户端连接对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ClientPeer GetClientPeerByUserId(int id)
        {
            if (idClientDic.ContainsKey(id))
            {
                return idClientDic[id];
            }
            return null;
        }
        /// <summary>
        /// 构建用户信息传输模型
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static UserDto CreateUserDto(int userId)
        {
            MySqlCommand cmd = new MySqlCommand("select * from userinfo where Id=@id", sqlConnect);
            cmd.Parameters.AddWithValue("id", userId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                UserDto dto = new UserDto(userId, reader.GetString("UserName"), reader.GetString("IconName"), reader.GetInt32("Coin"));

                reader.Close();
                return dto;
            }
            reader.Close();
            return null;
        }

        /// <summary>
        /// 获取排行榜信息
        /// </summary>
        /// <returns></returns>
        public static RankListDto GetRankListDto()
        {
            MySqlCommand cmd = new MySqlCommand("select UserName,Coin from userinfo order by Coin desc",sqlConnect);
            MySqlDataReader reader = cmd.ExecuteReader();
            rankListDto.Clear();
            
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    RankItemDto dto = new RankItemDto(reader.GetString("UserName"), reader.GetInt32("Coin"));
                    rankListDto.Add(dto);
                }
                reader.Close();
                return rankListDto;
            }
            reader.Close();
            return null; 
        }

        /// <summary>
        /// 更新金币数量
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int UpdateCoinCount(int userId,int value)
        {
            //查询账号金币数
            MySqlCommand cmd = new MySqlCommand("select Coin from userinfo where Id=@id", sqlConnect);
            cmd.Parameters.AddWithValue("id", userId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                int count = reader.GetInt32("Coin");
                reader.Close();

                int afterCoin = 0;
                if (value < 0)
                {
                    if (count < -value)
                    {
                        afterCoin = 0;
                    }
                    else
                    {
                        afterCoin = value + count;
                    }
                }
                else
                {
                    afterCoin = value + count;
                }
                MySqlCommand cmd1 = new MySqlCommand("update userinfo set Coin=@Coin where Id=@id", sqlConnect);
                cmd1.Parameters.AddWithValue("id", userId);
                cmd1.Parameters.AddWithValue("Coin", afterCoin);
                cmd1.ExecuteNonQuery();
                return afterCoin;
            }
            reader.Close();
            return 0;
        }
    }
}
