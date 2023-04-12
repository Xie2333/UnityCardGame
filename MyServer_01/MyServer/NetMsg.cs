using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    //网络消息类
    //发送消息都发送这个类，接收消息后转换为这个类
    public class NetMsg
    {
        //操作码
        public int opCode { get; set; }
        //子操作码
        public int subCode { get; set; }
        //传递的参数
        public object value { get; set; }

        public NetMsg()
        {

        }
        public NetMsg(int opCode, int subCode, object value)
        {
            this.opCode = opCode;
            this.subCode = subCode;
            this.value = value;
        }
        //改变的方法
        public void Change(int opCode, int subCode, object value)
        {
            this.opCode = opCode;
            this.subCode = subCode;
            this.value = value;
        }
    }
}
