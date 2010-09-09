using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SSHSharp.Verifiers
{
    /// <summary>
    /// Basically the same as the Strict verifier, but does not try to actually
    /// verify a connection if the server is the localhost and the port is a
    /// nonstandard port number. Those two conditions will typically mean the
    /// connection is being tunnelled through a forwarded port, so the known-hosts
    /// file will not be helpful (in general).
    /// </summary>
    public class Lenient : Strict
    {
        /// <summary>
        /// Tries to determine if the connection is being tunnelled, and if so,
        /// returns true. Otherwise, performs the standard strict verification.</summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public override bool Verify(HostKeyVerificationData arguments)
        {
            if (IsTunneled(arguments))
                return true;
          
            return base.Verify(arguments);
        }

        private static bool IsTunneled(HostKeyVerificationData arguments)
        {
            if(arguments.Session.Port == Transport.Session.DefaultPort)
                return false;

            var ip = arguments.Session.Peer.IPAddress;

            return IPAddress.IsLoopback(ip);
        }
    }
}
