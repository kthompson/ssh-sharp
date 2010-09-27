using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Transport
{
    public class SessionOptions
    {
        public string UserKnownHostsFile { get; set; }
        public string GlobalKnownHostsFile { get; set; }

        public string HostKeyAlias { get; set; }
    }
}
