using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Verifiers
{
    /// <summary>
    /// The Null host key verifier simply allows every key it sees, without
    /// bothering to verify. This is simple, but is not particularly secure.</summary>
    public class Null : IHostKeyVerifier
    {
        public bool Verify(HostKeyVerificationData arguments)
        {
            return true;
        }
    }
}
