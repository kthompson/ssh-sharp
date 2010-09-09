using System.Net;

namespace SSHSharp
{
    public class PeerInfo
    {
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }
        public string Host { get; set; }
        public string Canonized { get; set; }
    }
}