﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace SfBaseTcp.Net.Service
{
	[Serializable]
    public class MethodRuntimeException : Exception
    {
        public MethodRuntimeException(string channelName, string methodName, IPEndPoint endPoint)
            : base("方法运行出错。")
        {
            Source = endPoint.ToString() + "/" + channelName + "/" + methodName;
        }
    }
}
