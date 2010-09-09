using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Verifiers
{
    [Incomplete]
    public class Strict : IHostKeyVerifier
    {
        public virtual bool Verify(HostKeyVerificationData arguments)
        {
            throw new NotImplementedException();   
        }
    }
}
