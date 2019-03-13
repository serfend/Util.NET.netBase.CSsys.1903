﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SfBaseTcp.Net.Sockets;

namespace SfBaseTcp.Net.Communication
{
    internal class ServerAuthentication
    {
        private Server Server;

        public ServerAuthentication(Server server)
        {
            Server = server;
        }

        public void WaitForAuthentication(ISocket socket)
        {
            socket.ReceiveCompleted += socket_ReceiveCompleted;
        }

        private void socket_ReceiveCompleted(object sender, SocketEventArgs e)
        {
            
        }

        public event EventHandler<SocketEventArgs> AuthenticateCompleted;
    }
}
