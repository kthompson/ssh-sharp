using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace SSHSharp.Transport
{
    /// <summary>
    /// Negotiates the SSH protocol version and trades information about server
    /// and client. This is never used directly--it is always called by the
    /// transport layer as part of the initialization process of the transport
    /// layer.
    /// 
    /// Note that this class also encapsulates the negotiated version, and acts as
    /// the authoritative reference for any queries regarding the version in effect.
    /// </summary>
    public class ServerVersion
    {
        /// <summary>
        /// The SSH version string as reported by SSHSharp
        /// </summary>
        public static readonly string ProtoVersion = string.Format("SSH-2.0-CSharp/SSHSharp_{0} {1}", SSHSharp.Version.Current, Environment.OSVersion.Platform);

        /// <summary>
        /// Any header text sent by the server prior to sending the version.
        /// </summary>
        public string Header { get; private set; }
        /// <summary>
        /// The version string reported by the server.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// Instantiates a new ServerVersion and immediately (and synchronously)
        /// negotiates the SSH protocol in effect, using the given socket.
        /// </summary>
        public ServerVersion(NetworkStream socket)
        {
            this.Header = string.Empty;
            this.Version = null;
            Negotiate(socket);
        }

        /// <summary>
        /// Negotiates the SSH protocol to use, via the given socket. If the server
        /// reports an incompatible SSH version (e.g., SSH1), this will raise an
        /// exception.
        /// </summary>
        private void Negotiate(NetworkStream socket)
        {
            Trace.TraceInformation("negotiating protocol version");
            var sb = new StringBuilder();

            var reader = new StreamReader(socket);

            while(true)
            {
                this.Version = reader.ReadLine();
                if(this.Version == null || new Regex("^SSH-").IsMatch(this.Version))
                    break;
                //TODO: should we be using AppendLine?
                sb.Append(this.Version);
            }

            this.Header = sb.ToString();
            this.Version = this.Version.TrimEnd();

            Trace.TraceInformation("remote is `{0}'", this.Version);

            if (!new Regex(@"^SSH-(1\.99|2\.0)-").IsMatch(this.Version))
                throw new Exception(string.Format("incompatible SSH version `{0}'", this.Version));

            Trace.TraceInformation("local is `{0}'", ProtoVersion);

            new StreamWriter(socket).Write(string.Format("{0}\r\n", this.Version));

        }
    }
}
