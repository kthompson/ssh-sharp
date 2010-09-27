using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SSHSharp.Verifiers;

namespace SSHSharp.Transport
{
    /// <summary>
    /// The transport layer represents the lowest level of the SSH protocol, and
    /// implements basic message exchanging and protocol initialization. It will
    /// never be instantiated directly (unless you really know what you're about),
    /// but will instead be created for you automatically when you create a new
    /// SSH session via Net::SSH.start.
    /// </summary>
    [Incomplete]
    public class Session
    {
        /// <summary>
        /// The standard port for the SSH protocol.
        /// </summary>
        public const int DefaultPort = 22;

        /// <summary>
        /// The port number to connect to, as given in the options to the constructor.
        /// If no port number was given, this will default to DefaultPort.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// The host to connect to, as given to the constructor.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// The underlying socket object being used to communicate with the remote
        /// host.
        /// </summary>
        public NetworkStream Socket { get; private set; }

        /// <summary>
        /// The ServerVersion instance that encapsulates the negotiated protocol
        /// version.
        /// </summary>
        public ServerVersion ServerVersion { get; private set; }

        /// <summary>
        /// The host-key verifier object used to verify host keys, to ensure that
        /// the connection is not being spoofed.
        /// </summary>
        public IHostKeyVerifier HostKeyVerifier { get; private set; }

        /// <summary>
        /// The hash of options that were given to the object at initialization.
        /// </summary>
        public SessionOptions Options { get; set; }

        /// <summary>
        /// Instantiates a new transport layer abstraction. This will block until
        /// the initial key exchange completes, leaving you with a ready-to-use
        /// transport session.
        /// </summary>
        /// <param name="host"></param>
        public Session(string host) 
            : this(host, new SessionOptions())
        {
        }

        /// <summary>
        /// Instantiates a new transport layer abstraction. This will block until
        /// the initial key exchange completes, leaving you with a ready-to-use
        /// transport session.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="options"></param>
        public Session(string host, SessionOptions options)
        {
                
        }

        public PeerInfo Peer { get; set; }


        public string HostAsString()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Instantiates a new host-key verification class, based on the value of
        /// the parameter. When true or nil, the default Lenient verifier is
        /// returned. If it is false, the Null verifier is returned, and if it is
        /// :very, the Strict verifier is returned. If the argument happens to
        /// respond to :verify, it is returned directly. Otherwise, an exception
        /// is raised.
        /// </summary>
        /// <param name="paranoid"></param>
        /// <returns></returns>
        private IHostKeyVerifier SelectHostKeyVerifier(object paranoid)
        {
            if (paranoid == null)
                return new Lenient();

            if (paranoid.Equals("very"))
                return new Strict();

            if(paranoid is IHostKeyVerifier)
                return (IHostKeyVerifier) paranoid;

            if(paranoid is bool)
            {
                var b = (bool) paranoid;
                if (b)
                    return new Lenient();
                
                return new Null();
            }

            throw new ArgumentException(string.Format("argument paranoid is not valid: {0}", paranoid), "paranoid");
        }
    }
}
