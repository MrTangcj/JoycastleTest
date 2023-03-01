using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Tcp
{
   public class ServerClient
   {
      public TcpClient client;
      public NetworkStream stream;
      public ServerClient(TcpClient tcpClient)
      {
         client = tcpClient;
         stream = client.GetStream();
      }
      public void Close()
      {
         client.Close();
         client = null;
         stream.Close();
         stream = null;
      }

      public void BeginReceive(byte[] buffer,int offset,int size,AsyncCallback callback,object state)
      {
         stream.BeginRead(buffer, offset, size, callback, state);
      }
      public void SendMessage(string content)
      {
         byte[] data = Message.GetBytes(content);
         stream.Write(data, 0, data.Length);
      }
   }
   public class Server : MonoBehaviour
   {
      private TcpListener _listener;
      private ServerClient _client;
      //显示信息，可以设计一个消息分发器，其他类可以监听相应协议，分发器将收到的信息通过观察者模式分发给订阅者
      public Action<string> OnStatus;
      //客户端列表，Ip为Key
      private Dictionary<string, ServerClient> _clientList = new Dictionary<string, ServerClient>();
      //数据接收器列表，Ip为Key
      private Dictionary<string, MessageReceiver> _msgReceiverList = new Dictionary<string, MessageReceiver>();
      private void Start()
      {
         //充当Main方法
         Init();
      }

      void Init()
      {
         _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9000);
         _listener.Start();
         Debug.Log("开始异步监听");
         _listener.BeginAcceptTcpClient(Accept, _listener);
      }

      //收到监听后的异步回调
      void Accept(IAsyncResult result)
      {
         var listener = result.AsyncState as TcpListener;
         TcpClient client = listener.EndAcceptTcpClient(result);
         Debug.Log("服务器链接成功");
         string clientIp = client.Client.RemoteEndPoint.ToString();
         if (!String.IsNullOrEmpty(clientIp))
         {
            ServerClient serverClient = new ServerClient(client);
            _clientList.Add(clientIp, serverClient);
            MessageReceiver receiver = new MessageReceiver();
            _msgReceiverList.Add(clientIp, receiver);
            serverClient.BeginReceive(receiver.DataBuffer, receiver.Offset, receiver.RemainingSize, Receive,
               serverClient);
         }
         //接收其他客户端的链接
         listener.BeginAcceptTcpClient(Accept, listener);
      }

      void Receive(IAsyncResult result)
      {
         var serverClient = result.AsyncState as ServerClient;
         try
         {
            string clientIp = serverClient.client.Client.RemoteEndPoint.ToString();
            int dataSize = serverClient.stream.EndRead(result);
            //获取数据长度
            MessageReceiver receiver = _msgReceiverList[clientIp];
            if (dataSize > 0)
            {
               receiver.UntiePack(dataSize);
               foreach (Message msg in receiver.MsgList)
               {
                  //可以设计一个消息分发器，其他类可以监听相应协议，分发器将收到的信息通过观察者模式分发给订阅者
                  string content = Encoding.Default.GetString(msg.Data, 0, msg.DataLen);
                  Debug.Log($"服务器收到消息:{content}");
                  //OnStatus?.Invoke(content);
               }
               receiver.Clear();
            }
            //继续接收下一条信息
            serverClient.BeginReceive(receiver.DataBuffer, receiver.Offset, receiver.RemainingSize, Receive,
               serverClient);
         }
         catch (SocketException e)
         {
            Debug.Log(e);
            Close(serverClient.client.Client.RemoteEndPoint.ToString());
         }
      }

      public void SendMessageToClient(string ip, string content)
      {
         if (_clientList.TryGetValue(ip, out var serverClient))
         {
            byte[] data = Message.GetBytes(content);
            serverClient.stream.Write(data, 0, data.Length);
            Debug.Log($"服务器消息发送:{content}");
         }
      }
      void Close(string ip)
      {
         Debug.Log($"客户端 {ip} 断开连接");
         _clientList.Remove(ip);
         _msgReceiverList.Remove(ip);
      }
   }
}
