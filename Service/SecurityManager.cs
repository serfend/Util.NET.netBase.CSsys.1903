﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SfBaseTcp.Net.Service
{
    public class SecurityManager
    {
        public SecurityMode Mode { get; set; }

        public MessageCredential Credential { get; set; }
    }
}
