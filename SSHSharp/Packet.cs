using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp
{
    /// <summary>
    /// A specialization of Buffer that knows the format of certain common
    /// packet types. It auto-parses those packet types, and allows them to
    /// be accessed via the #[] accessor.
    /// 
    ///   data = some_channel_request_packet
    ///   packet = Net::SSH::Packet.new(data)
    /// 
    ///   p packet.type #-> 98 (CHANNEL_REQUEST)
    ///   p packet[:request]
    ///   p packet[:want_reply]
    /// 
    /// This is used exclusively internally by Net::SSH, and unless you're doing
    /// protocol-level manipulation or are extending Net::SSH in some way, you'll
    /// never need to use this class directly.
    /// </summary>
    internal class Packet : Buffer
    {
        public static Dictionary<int, Tuple<string, Type>[]> Types { get; private set; }


        /// <summary>
        /// Register a new packet type that should be recognized and auto-parsed by
        /// Net::SSH::Packet. Note that any packet type that is not preregistered
        /// will not be autoparsed.
        /// 
        /// The +pairs+ parameter must be either empty, or an array of two-element
        /// tuples, where the first element of each tuple is the name of the field,
        /// and the second is the type.
        /// 
        ///   register DISCONNECT, ["reason_code", long], [:description, :string], [:language, :string]
        /// </summary>
        /// <param name="type"></param>
        /// <param name="pairs"></param>
        public static void Register(int type, params Tuple<string, Type>[] pairs)
        {
            Types.Add(type, pairs);
        }

        static Packet()
        {

            Register(Transport.Constants.Disconnect, T("reason_code", typeof(long)),
                                                     T("description", typeof(string)),
                                                     T("language", typeof(string)));

            Register(Transport.Constants.Ignore, T("data", typeof(string)));

            Register(Transport.Constants.Unimplemented, T("number", typeof(long)));

            Register(Transport.Constants.Debug, T("always_display", typeof(bool)),
                                                T("message", typeof(string)),
                                                T("language", typeof(string)));

            Register(Transport.Constants.ServiceAccept, T("service_name", typeof(string)));

            Register(Authentication.Constants.UserauthBanner, T("message", typeof(string)),
                                                              T("language", typeof(string)));

            Register(Authentication.Constants.UserauthFailure, T("authentications", typeof(string)),
                                                               T("partial_success", typeof(bool)));

            Register(Connection.Constants.GlobalRequest, T("request_type", typeof(string)),
                                                         T("want_reply", typeof(bool)),
                                                         T("request_data", typeof(Buffer)));

            Register(Connection.Constants.ChannelOpen, T("channel_type", typeof(string)),
                                                       T("remote_id", typeof(long)), 
                                                       T("window_size", typeof(long)),
                                                       T("packet_size", typeof(long)));

            Register(Connection.Constants.ChannelOpenConfirmation, T("local_id", typeof(long)),
                                                                   T("remote_id", typeof(long)),
                                                                   T("window_size", typeof(long)),
                                                                   T("packet_size", typeof(long)));

            Register(Connection.Constants.ChannelOpenFailure, T("local_id", typeof(long)),
                                                              T("reason_code", typeof(long)), 
                                                              T("description", typeof(string)), 
                                                              T("language", typeof(string)));

            Register(Connection.Constants.ChannelWindowAdjust, T("local_id", typeof(long)),
                                                               T("extra_bytes", typeof(long)));

            Register(Connection.Constants.ChannelData, T("local_id", typeof(long)), T("data", typeof(string)));
            Register(Connection.Constants.ChannelExtendedData, T("local_id", typeof(long)),
                                                               T("data_type", typeof(long)), T("data", typeof(string)));

            Register(Connection.Constants.ChannelEof, T("local_id", typeof(long)));
            Register(Connection.Constants.ChannelClose, T("local_id", typeof(long)));
            Register(Connection.Constants.ChannelRequest, T("local_id", typeof(long)), T("request", typeof(string)),
                     T("want_reply", typeof(bool)), T("request_data", typeof(Buffer)));

            Register(Connection.Constants.ChannelSuccess, T("local_id", typeof(long)));
            Register(Connection.Constants.ChannelFailure, T("local_id", typeof(long)));
        }

        private static Tuple<T1, T2> T<T1, T2>(T1 a, T2 b)
        {
            return Tuple.Create(a, b);
        }
    }
}
