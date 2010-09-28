using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SSHSharp.Transport
{
    public class PacketStream : NetworkStream
    {

        /// <summary>
        /// The map of "hints" that can be used to modify the behavior of the packet
        /// stream. For instance, when authentication succeeds, an "authenticated"
        /// hint is set, which is used to determine whether or not to compress the
        /// data when using the "delayed" compression algorithm.
        /// </summary>
        public Dictionary<string,string> Hints { get; private set; }

        /// <summary>
        /// The client state object, which encapsulates the algorithms used to build
        /// packets to send to the server.
        /// </summary>
        public State Client { get; private set; }
        
        /// <summary>
        /// The server state object, which encapsulates the algorithms used to interpret
        /// packets coming from the server.
        /// </summary>
        public State Server { get; private set; }

        internal PacketStream(Socket socket, bool ownsSocket)
            : base(socket, ownsSocket)
        {
            this.Hints = new Dictionary<string, string>();
            this.Server = new State(this, StateMode.Server);
            this.Client = new State(this, StateMode.Client);
            //@packet = null;
            InitializeBufferedIo();
        }

        private void InitializeBufferedIo()
        {
            throw new NotImplementedException();
        }
    }
}
