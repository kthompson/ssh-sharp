using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Transport
{
    public class Constants
    {
        //
        // Transport layer generic messages
        //

        public const int Disconnect = 1;
        public const int Ignore = 2;
        public const int Unimplemented = 3;
        public const int Debug = 4;

        public const int ServiceRequest = 5;
        public const int ServiceAccept = 6;

        //--
        // Algorithm negotiation messages
        //++

        public const int Kexinit = 20;
        public const int Newkeys = 21;

        //--
        // Key exchange method specific messages
        //++

        public const int KexdhInit = 30;
        public const int KexdhReply = 31;
    }
}
