using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Authentication
{
    /// <summary>
    /// Describes the constants used by the Net::SSH::Authentication components
    /// of the Net::SSH library. Individual authentication method implemenations
    /// may define yet more constants that are specific to their implementation.
    /// </summary>
    public class Constants
    {
        public const int UserauthRequest = 50;
        public const int UserauthFailure = 51;
        public const int UserauthSuccess = 52;
        public const int UserauthBanner = 53;

        public const int UserauthPasswdChangereq = 60;
        public const int UserauthPkOk = 60;

        //public const int USERAUTH_METHOD_RANGE = 60..79;
    }
}
