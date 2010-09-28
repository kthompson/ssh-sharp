using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Connection
{
    /// <summary>
    /// Definitions of constants that are specific to the connection layer of the
    /// SSH protocol.
    /// </summary>
    public class Constants
    {
        //--
        // Connection protocol generic messages
        //++

        public const int GlobalRequest = 80;
        public const int RequestSuccess = 81;
        public const int RequestFailure = 82;

        //--
        // Channel related messages
        //++

        public const int ChannelOpen = 90;
        public const int ChannelOpenConfirmation = 91;
        public const int ChannelOpenFailure = 92;
        public const int ChannelWindowAdjust = 93;
        public const int ChannelData = 94;
        public const int ChannelExtendedData = 95;
        public const int ChannelEof = 96;
        public const int ChannelClose = 97;
        public const int ChannelRequest = 98;
        public const int ChannelSuccess = 99;
        public const int ChannelFailure = 100;
    }
}
