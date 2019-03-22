using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Net.Security;

namespace SfBaseTcp.Net.Sockets
{
    /// <summary>
    /// TCP客户端
    /// </summary>
    public class TCPClient : SocketBase
    {
        /// <summary>
        /// 实例化TCP客户端。
        /// </summary>
        public TCPClient()
            : base(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), new SocketHandler())
        {
			this.Disconnect();
        }

        public bool IsUseAuthenticate { get; set; }

        #region 连接

        /// <summary>
        /// 连接至服务器。
        /// </summary>
        /// <param name="endpoint">服务器终结点。</param>
        public void Connect(IPEndPoint endpoint)
        {
            //判断是否已连接
            if (IsConnected)
                throw new InvalidOperationException("已连接至服务器。");
            if (endpoint == null)
                throw new ArgumentNullException("endpoint");
			//锁定自己，避免多线程同时操作
			if (isConnecting) return;
			isConnecting = true;

			lock (this)
            {
                SocketAsyncState state = new SocketAsyncState();
				//Socket异步连接
				try
				{
					Socket.BeginConnect(endpoint, EndConnect, state).AsyncWaitHandle.WaitOne();
				}
				catch 
				{
					Disconnect();
				}
                //等待异步全部处理完成
                while (!state.Completed) 
                {
                    Thread.Sleep(1);
                }
            }
			isConnecting = false;

		}
		private bool isConnecting = false;
        /// <summary>
        /// 异步连接至服务器。
        /// </summary>
        /// <param name="endpoint"></param>
        public void ConnectAsync(IPEndPoint endpoint)
        {
            //判断是否已连接
            if (IsConnected)
                throw new InvalidOperationException("已连接至服务器。");
            if (endpoint == null)
                throw new ArgumentNullException("endpoint");
            //锁定自己，避免多线程同时操作
            lock (this)
            {
                SocketAsyncState state = new SocketAsyncState();
                //设置状态为异步
                state.IsAsync = true;
                //Socket异步连接
                Socket.BeginConnect(endpoint, EndConnect, state);
            }
        }

        private void EndConnect(IAsyncResult result)
        {
            SocketAsyncState state = (SocketAsyncState)result.AsyncState;

            try
            {
                Socket.EndConnect(result);
            }
            catch
            {
                //出现异常，连接失败。
                state.Completed = true;
				ConnectCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Disconnect));
				return;
            }
            //连接成功。
            //创建Socket网络流
            Stream = new NetworkStream(Socket);
            if (IsUseAuthenticate)
            {
                NegotiateStream negotiate = new NegotiateStream(Stream);
                negotiate.AuthenticateAsClient();
                while (!negotiate.IsMutuallyAuthenticated)
                {
                    Thread.Sleep(10);
                }
            }
            //连接完成
            state.Completed = true;
			ConnectCompleted?.Invoke(this, new SocketEventArgs(this, SocketAsyncOperation.Connect));

			//开始接收数据
			if (IsConnected)
            Handler.BeginReceive(Stream, EndReceive, state);
        }

        #endregion

        /// <summary>
        /// 连接完成时引发事件。
        /// </summary>
        public event EventHandler<SocketEventArgs> ConnectCompleted;
    }
}
