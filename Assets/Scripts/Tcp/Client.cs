using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Tcp
{
    public class Client : MonoBehaviour
    {
        private TcpClient _client;
        private MessageReceiver _receiver;
        private NetworkStream _stream;
        public Action<string> OnStatus;
        private void Start()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string ip, int port)
        {
            _client.BeginConnect(IPAddress.Parse(ip), port, Connect, _client);
        }

        void Connect(IAsyncResult result)
        {
            TcpClient client = result.AsyncState as TcpClient;
            client.EndConnect(result);
            if (_client.Connected)
            {
                Debug.Log("客户端连接成功");
                _receiver = new MessageReceiver();
                _stream = _client.GetStream();
                _stream.BeginRead(_receiver.DataBuffer, _receiver.Offset, _receiver.RemainingSize, Receive,
                    this);
            }
            else
            {
                Debug.Log("连接失败");
            }
        }

        void Receive(IAsyncResult result)
        {
            //Client client = result.AsyncState as Client;
            try
            {
                int dataSize = _stream.EndRead(result);
                if (dataSize > 0)
                {
                    _receiver.UntiePack(dataSize);
                    foreach (Message msg in _receiver.MsgList)
                    {
                        //可以设计一个消息分发器，其他类可以监听相应协议，分发器将收到的信息通过观察者模式分发给订阅者
                        string content = Encoding.Default.GetString(msg.Data, 0, msg.DataLen);
                        Debug.Log($"客户端收到消息:{content}");
                        //OnStatus?.Invoke(content);
                    }
                    _receiver.Clear();
                }
                //继续接收下一条信息
                _stream.BeginRead(_receiver.DataBuffer, _receiver.Offset, _receiver.RemainingSize, Receive,
                    this);
            }
            catch (SocketException e)
            {
                Debug.Log(e);
            }
        }
        public void SendMessageToServer(string content)
        {
            if (_client.Connected)
            {
                byte[] data = Message.GetBytes(content);
                _stream.Write(data, 0, data.Length);
                Debug.Log($"客户端消息发送:{content}");
            }
            else
            {
                Debug.Log("连接断开");
            }
        }
        
        public void DisconnectToServer()
        {
            _stream.Close();
            _client.Close();
        }
        
    }
}
