using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Verifiers
{
    public interface IHostKeyVerifier
    {
        bool Verify(HostKeyVerificationData arguments);
    }
}
