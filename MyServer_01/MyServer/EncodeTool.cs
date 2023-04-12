using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    public class EncodeTool
    {
        //构造包头 + 包尾
        public static byte[] EncodePacket(byte[] data)
        {
            //创建内存流对象，使用using可以不用自己手动释放内存流对象
            //需要 ms.Close();
            using (MemoryStream ms = new MemoryStream())
            {
                //创建二进制写的对象
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    //写入包的长度
                    bw.Write(data.Length);
                    //写入包的数据
                    bw.Write(data);
                    byte[] packet = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, packet, 0, (int)ms.Length);
                    return packet;
                }
            }
        }
        //解析包的方法
        //从缓冲区中取出一个完整的包
        //ref表面需要更新 cache 缓存
        public static byte[] DecodePacket(ref List<byte> cache)
        {
            if (cache.Count < 4) return null;
            using (MemoryStream ms = new MemoryStream(cache.ToArray()))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    //从当前流中读取4字节有符号整数，并使流的当前位置提升4字节
                    //前4个字节为当前流的长度，所以实际数据需要向后移动4字节
                    int length = br.ReadInt32();
                    //获取剩余字节长度 ms.length(当前字节流长度) - ms.Position(游标当前位置)
                    int remainLength = (int)(ms.Length - ms.Position);
                    if (length > remainLength)
                    {
                        //缓冲区长度无法构成包
                        return null;
                    }
                    //能构成包的情况下，获取数据
                    byte[] data = br.ReadBytes(length);
                    //更新数据缓存
                    cache.Clear();
                    //将指定元素添加到List末尾
                    cache.AddRange(br.ReadBytes(remainLength));
                    return data;
                }

            }
        }

        //将NetMsg类转换为字节数组,发送出去
        public static byte[] EncodeMsg(NetMsg msg)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                //创建二进制写的对象
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(msg.opCode);
                    bw.Write(msg.subCode);
                    if (msg.value != null)
                    {
                        bw.Write(EncodeObj(msg.value));
                    }
                    byte[] data = new byte[ms.Length];
                    Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int)ms.Length);
                    return data;
                }
            }
        }
        //将字节数组转化为NetMsg(网络消息类)
        public static NetMsg DecodeMsg(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                //创建二进制读取对象
                using (BinaryReader br = new BinaryReader(ms))
                {
                    NetMsg msg = new NetMsg();
                    //读取前4个字节，并且将游标向后移动4个字节
                    msg.opCode = br.ReadInt32();
                    //读取前4个字节，并且将游标向后移动4个字节
                    msg.subCode = br.ReadInt32();
                    //判断消息本体是否为空
                    if (ms.Length - ms.Position > 0)
                    {
                        //读取的字节，需要反序列化
                        object obj = DecodeObj(br.ReadBytes((int)(ms.Length - ms.Position)));
                        msg.value = obj;
                    }
                    return msg;
                }
            }
        }
        //序列化方法
        private static byte[] EncodeObj(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                //调用序列化方法
                bf.Serialize(ms, obj);
                //将obj对象转化为字节流存入ms中
                byte[] data = new byte[ms.Length];
                Buffer.BlockCopy(ms.GetBuffer(), 0, data, 0, (int)ms.Length);
                return data;
            }
        }
        //反序列化方法
        private static object DecodeObj(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return bf.Deserialize(ms);
            }
        }
    }
}
