using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SfBaseTcp.Net.Communication
{
    public class Credential
    {
        public Credential(byte[] username, byte[] password)
        {
            Username = username;
            Password = password;
        }

        public byte[] Username { get; private set; }

        public byte[] Password { get; private set; }
    }
}
