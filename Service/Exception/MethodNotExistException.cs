﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SfBaseTcp.Net.Service
{
	[Serializable]
    public class MethodNotExistException : Exception
    {
        public MethodNotExistException(string channelName, string methodName, IPEndPoint endPoint)
            : base("方法不存在。")
        {
            Source = endPoint.ToString() + "/" + channelName + "/" + methodName;
        }
    }
}
