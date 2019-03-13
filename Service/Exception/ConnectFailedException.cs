using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SfBaseTcp.Net.Service
{
    public class ConnectFailedException : Exception
    {
        public ConnectFailedException(string errorMsg)
            : base(errorMsg)
        {
        }
    }
}
