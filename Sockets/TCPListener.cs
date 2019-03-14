﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SfBaseTcp.Net.Sockets
{
    /// <summary>
    /// TCP监听端
    /// </summary>
    public class TCPListener : IEnumerable<TCPListenerClient>, IDisposable
    {
        private Socket socket;
        private HashSet<TCPListenerClient> clients;

        /// <summary>
        /// 实例化TCP监听者。
        /// </summary>
        public TCPListener()
        {
            clients = new HashSet<TCPListenerClient>();
            IsStarted = false;
            Handler = new SocketHandler();
            IsUseAuthenticate = false;
        }

        public ISocketHandler Handler { get; set; }

        public bool IsUseAuthenticate { get; set; }

        public int Count { get { return clients.Count; } }

        private int port;
        /// <summary>
        /// 监听端口。
        /// </summary>
        public int Port
        {
            get { return port; }
            set
            {
                if (value < 0 || value > 65535)
                    throw new ArgumentOutOfRangeException(port + "不是有效端口。");
                port = value;
            }
        }

        /// <summary>
        /// 服务启动中
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// 开始服务。
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (IsStarted)
                    throw new InvalidOperationException("已经开始服务。");
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定端口
                //可以引发端口被占用异常
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                //监听队列
                socket.Listen(ushort.MaxValue);
                //如果端口是0，则是随机端口，把这个端口赋值给port
                port = ((IPEndPoint)socket.LocalEndPoint).Port;
                //服务启动中设置为true
                IsStarted = true;
                //开始异步监听
                socket.BeginAccept(EndAccept, null);
            }
        }

        //异步监听结束
        private void EndAccept(IAsyncResult result)
        {
            Socket clientSocket = null;

            //获得客户端Socket
            try
            {
                clientSocket = socket.EndAccept(result);
                socket.BeginAccept(EndAccept, null);
            }
            catch
            {

            }

            if (clientSocket == null)
                return;

            //实例化客户端类
            TCPListenerClient client = new TCPListenerClient(this, clientSocket);
            //增加事件钩子
            client.SendCompleted += client_SendCompleted;
            client.ReceiveCompleted += client_ReceiveCompleted;
            client.DisconnectCompleted += client_DisconnectCompleted;

            //增加客户端
            lock (clients)
                clients.Add(client);

			//客户端连接事件
			AcceptCompleted?.Invoke(this, new SocketEventArgs(client, SocketAsyncOperation.Accept));
		}

        /// <summary>
        /// 停止服务。
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (!IsStarted)
                    throw new InvalidOperationException("没有开始服务。");
                foreach (TCPListenerClient client in clients)
                {
                    client.Disconnect();
                    client.DisconnectCompleted -= client_DisconnectCompleted;
                    client.ReceiveCompleted -= client_ReceiveCompleted;
                    client.SendCompleted -= client_SendCompleted;
                }
                socket.Close();
                socket = null;
                IsStarted = false;
            }
        }

        /// <summary>
        /// 接收完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> ReceiveCompleted;
        /// <summary>
        /// 接受客户完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> AcceptCompleted;
        /// <summary>
        /// 客户断开完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> DisconnectCompleted;
        /// <summary>
        /// 发送完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> SendCompleted;

        //客户端断开连接
        private void client_DisconnectCompleted(object sender, SocketEventArgs e)
        {
            //移除客户端
            lock (clients)
                clients.Remove((TCPListenerClient)e.Socket);

            e.Socket.DisconnectCompleted -= client_DisconnectCompleted;
            e.Socket.ReceiveCompleted -= client_ReceiveCompleted;
            e.Socket.SendCompleted -= client_SendCompleted;
			DisconnectCompleted?.Invoke(this, e);
		}

        //收到客户端发送的数据
        private void client_ReceiveCompleted(object sender, SocketEventArgs e)
        {
			ReceiveCompleted?.Invoke(this, e);
		}

        //向客户端发送数据完成
        private void client_SendCompleted(object sender, SocketEventArgs e)
        {
			SendCompleted?.Invoke(this, e);
		}

        /// <summary>
        /// 获取客户端泛型。
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TCPListenerClient> GetEnumerator()
        {
            return clients.GetEnumerator();
        }

        /// <summary>
        /// 获取客户端泛型。
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return clients.GetEnumerator();
        }

		#region IDisposable Support
		private bool disposedValue = false; // 要检测冗余调用

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Stop();
				}

				disposedValue = true;
			}
		}

		// 添加此代码以正确实现可处置模式。
		public void Dispose()
		{
			// 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
			Dispose(true);
		}
		#endregion


	}
}
