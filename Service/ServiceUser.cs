using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SfBaseTcp.Net.Communication;
using System.Threading;
using System.Reflection;

namespace SfBaseTcp.Net.Service
{
    public class ServiceUser
    {
        private bool connected;
        private Dictionary<ServiceChannel, ServiceContext> Contexts;
        internal ServerClient Client;

        internal ServiceUser(ServerClient client, Credential credential)
        {
            Client = client;
            Session = new ServiceSessionState();
            Identity = credential;
            Connected = true;
            Contexts = new Dictionary<ServiceChannel, ServiceContext>();
        }

        internal void Close()
        {
            Connected = false;
			Disconnect?.Invoke(this, null);
		}

        public ServiceContext GetContext(ServiceChannel channel)
        {
            if (!Contexts.ContainsKey(channel))
                Contexts.Add(channel, new ServiceContext(this, channel));
            return Contexts[channel];
        }

        public ServiceSessionState Session { get; private set; }

        public Credential Identity { get; private set; }
		public bool Connected { get => connected; set => connected = value; }

		public event EventHandler Disconnect;
    }
}
