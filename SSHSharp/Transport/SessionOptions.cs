using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SSHSharp.Transport
{
    public class SessionOptions
    {
        public string UserKnownHostsFile { get; set; }
        public string GlobalKnownHostsFile { get; set; }

        public string HostKeyAlias { get; set; }

        public int? Port { get; set; }
        public int? Timeout { get; set; }

        public Func<string, int, Stream> Proxy { get; set; }

        /// <summary>
        /// When true or nil, the default Lenient verifier is
        /// returned. If it is false, the Null verifier is returned, and if it is
        /// "very", the Strict verifier is returned. If the argument happens to
        /// respond to :verify, it is returned directly. Otherwise, an exception
        /// is raised.
        /// </summary>
        public object Paranoid { get; set; }
    }
}
