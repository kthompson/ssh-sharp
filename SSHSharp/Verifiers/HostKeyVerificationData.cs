using SSHSharp.Transport;

namespace SSHSharp.Verifiers
{
    public class HostKeyVerificationData
    {
        public string Fingerprint { get; set; }
        public PeerInfo Peer { get; set; }
        public HostKey Key { get; set; }
        public Session Session { get; set; }
    }
}