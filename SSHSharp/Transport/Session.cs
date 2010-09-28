using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        /// The Algorithms instance used to perform key exchanges.
        /// </summary>
        public Algorithms Algorithms { get; private set; }

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
            this.Host = host;
            this.Port = options.Port ?? DefaultPort;

            this.Options = options;

            Trace.TraceWarning("establishing connection to {0}:{1}", this.Host, this.Port);
            var factory = options.Proxy ?? ConnectFactory;

            if(options.Timeout != null)
                throw new NotSupportedException("Timeout is not currently supported");

            var socket = Timeout.Wait(options.Timeout ?? 0, () => factory(this.Host, this.Port));

            Trace.TraceInformation("Connection established.");

            this.HostKeyVerifier = SelectHostKeyVerifier(options.Paranoid);

            this.ServerVersion = new ServerVersion(socket);
            
            this.Algorithms = new Algorithms(this, options);

            Wait(() => this.Algorithms.IsInitialized);
        }

        private static PacketStream ConnectFactory(string host, int port)
        {
            var c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            c.Connect(host, port);
            return new PacketStream(c, true);
        }

        /// <summary>
        /// Waits (blocks) until the given block returns true. If no block is given,
        /// this just waits long enough to see if there are any pending packets. Any
        /// packets read are enqueued (see #push).
        /// </summary>
        /// <param name="func"></param>
        private void Wait(Func<bool> func)
        {
            while (true)
            {
                if (func != null && func())
                    break;

                var message = PollMessage("nonblock", false);
                if (message != null)
                    Push(message);

                if(func == null)
                    break;
            }
        }

        private void Push(object message)
        {
            throw new NotImplementedException();
        }

        private object PollMessage(string nonblock, bool b)
        {
            throw new NotImplementedException();
        }

        public PeerInfo Peer { get; set; }

        private string _hostAsString;
        /// <summary>
        /// Returns the host (and possibly IP address) in a format compatible with
        /// SSH known-host files.
        /// </summary>
        /// <returns></returns>
        public string HostAsString()
        {
            if (_hostAsString != null)
                return _hostAsString;

            var s = this.Host;
            if (this.Port != DefaultPort)
                s = string.Format("[{0}]:{1}", s, this.Port);

            // if socket.peer_ip != host
            if (this.Peer.IPAddress.ToString() != this.Host)
            {
                var s2 = this.Peer.IPAddress.ToString();
                if (this.Port != DefaultPort)
                    s2 = string.Format("[{0}]:{1}", s2, this.Port);

                return s + "," + s2;
            }

            return s;
        }

        /// <summary>
        /// Returns true if the underlying socket has been closed.
        /// </summary>
        public bool IsClosed
        {
            get { return this.Socket.CanRead || this.Socket.CanWrite; }
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
