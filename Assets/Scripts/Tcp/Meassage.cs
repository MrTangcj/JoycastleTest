using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;

namespace Tcp
{
    public class Message
    {
        public int DataLen;
        public byte[] Data;
        public bool IsComplete {
            get
            {
                if (DataLen == 0) return false;
                return DataLen == Data.Length;
            }
        }
        public Message(int dataLen, byte[] data)
        {
            DataLen = dataLen;
            Data = data;
        }
        public static byte[] GetBytes(string str)
        {
            //将信息长度封装在数据头
            byte[] dataStr = System.Text.Encoding.UTF8.GetBytes(str);
            byte[] dataLen = BitConverter.GetBytes(dataStr.Length);
            return dataLen.Concat(dataStr).ToArray();
        }
    }
    
    public class MessageReceiver
    {
        public const int HeadLen = 4;
        public int DataBufferLen;
        //当前_dataBuffer可用空间索引
        public int Offset = 0;
        public byte[] DataBuffer;
        //如果有数据残缺不完整的信息储存在这里
        public Message IncompleteMsg = null;
        public int RemainingSize => DataBufferLen - Offset;
        public List<Message> MsgList = new List<Message>();

        public MessageReceiver(int dataBufferLen = 1024)
        {
            DataBufferLen = dataBufferLen;
            DataBuffer = new byte[DataBufferLen];
        }

        public void UntiePack(int dataLength)
        {
            int dataSize = dataLength;
            dataSize += Offset;
            //IncompleteMsg为空，说明没有需要分包的数据
            if (IncompleteMsg == null)
            {
                SplitData(DataBuffer, dataSize);
                return;
            }
            int remainingDataLength = IncompleteMsg.DataLen - IncompleteMsg.Data.Length;
            if (remainingDataLength <= dataSize)
            {
                //当前数据足够补充完整
                Message msg = IncompleteMsg;
                byte[] realData = new byte[msg.DataLen];
                Array.Copy(msg.Data, 0, realData, 0, msg.Data.Length);
                Array.Copy(DataBuffer, 0, realData, msg.Data.Length, remainingDataLength);
                msg.Data = realData;
                //IncompleteMsg被补充完整，加入MsgList
                MsgList.Add(msg);
                IncompleteMsg = null;
                Offset = 0;
                DataBuffer = new byte[DataBufferLen];

                //处理剩下数据
                byte[] remainingDataBuffer = new byte[dataSize - remainingDataLength];
                Array.Copy(DataBuffer, remainingDataLength, remainingDataBuffer, 0, remainingDataBuffer.Length);
                SplitData(remainingDataBuffer, remainingDataBuffer.Length);
            }
            else
            {
                //当前数据不够完整，还需要下次数据
                byte[] realData = new byte[IncompleteMsg.Data.Length + dataSize];
                Array.Copy(IncompleteMsg.Data, 0, realData, 0, IncompleteMsg.Data.Length);
                Array.Copy(DataBuffer, 0, realData, IncompleteMsg.Data.Length, dataSize);
                IncompleteMsg.Data = realData;
                Offset = 0;
                DataBuffer = new byte[DataBufferLen];
            }
        }

        //处理data前4位就是数据长度的数据
        private void SplitData(byte[] data, int dataSize)
        {
            //每次递归创建新对象太多了，用迭代更合适
            if (dataSize <= HeadLen)
            {
                //说明包头部还没有接收完
                byte[] temp = new byte[DataBufferLen];
                Array.Copy(data, 0, temp, 0, dataSize);
                DataBuffer = temp;
                Offset = dataSize;
                return;
            }

            byte[] head = new byte[HeadLen];
            byte[] remainingData = new byte[dataSize - HeadLen];
            Array.Copy(data, 0, head, 0, HeadLen);
            Array.Copy(data, HeadLen, remainingData, 0, dataSize - HeadLen);
            int dataLength = BitConverter.ToInt32(head, 0);
            if (remainingData.Length > dataLength)
            {
                //说明数据中包含一个完整的包，需要处理粘包
                byte[] msgData = new byte[dataLength];
                Array.Copy(remainingData, 0, msgData, 0, dataLength);
                Message msg = new Message(dataLength, msgData);
                MsgList.Add(msg);
                byte[] last = new byte[remainingData.Length - dataLength];
                Array.Copy(remainingData, dataLength, last, 0, last.Length);
                //递归调用处理粘包
                SplitData(last, last.Length);
            }
            else
            {
                //数据（可能）不完整，保存等待后续信息
                Message msg = new Message(dataLength, remainingData);
                //如果数据完整，加入List，如果不完整赋值给IncompleteMsg
                if (dataLength < remainingData.Length)
                    IncompleteMsg = msg;
                else
                    MsgList.Add(msg);
                Offset = 0;
                DataBuffer = new byte[DataBufferLen];
            }
        }

        public void Clear()
        {
            if (MsgList.Count > 0)
            {
                MsgList.Clear();
            }
        }
        
        
    }
}
