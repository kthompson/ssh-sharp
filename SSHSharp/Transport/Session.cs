using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Transport
{
    [Incomplete]
    public class Session
    {
        public const int DefaultPort = 22;

        public int Port
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public PeerInfo Peer
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
