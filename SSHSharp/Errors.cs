using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using SSHSharp.Verifiers;

namespace SSHSharp
{
    /// <summary>
    /// A general exception class, to act as the ancestor of all other SSHSharp
    /// exception classes.</summary>
    public class Exception : ApplicationException
    {
        public Exception()
        {
        }

        public Exception(string message)
            : base(message)
        {

        }
    }

    /// <summary>
    /// This exception is raised when authentication fails (whether it be
    /// public key authentication, password authentication, or whatever).</summary>
    public class AuthenticationFailedException : Exception
    {
    }

    /// <summary>
    /// This exception is raised when the remote host has disconnected
    /// unexpectedly.</summary>
    public class DisconnectException : Exception
    {

    }

    /// <summary>
    /// This exception is primarily used internally, but if you have a channel
    /// request handler (see SSHSharp.Connection.Channel#on_request) that you
    /// want to fail in such a way that the server knows it failed, you can
    /// raise this exception in the handler and Net::SSH will translate that into
    /// a "channel failure" message.</summary>
    public class ChannelRequestFailedException : Exception
    {
    }

    /// <summary>
    /// This is exception is primarily used internally, but if you have a channel
    /// open handler (see SSHSharp.Connection.Session#on_open_channel) and you
    /// want to fail in such a way that the server knows it failed, you can
    /// raise this exception in the handler and SSHSharp will translate that into
    /// a "channel open failed" message.
    /// </summary>
    public class ChannelOpenFailedException : Exception
    {
        public int Code { get; private set; }
        public string Reason { get; private set; }

        public ChannelOpenFailedException(int code, string reason)
            : base(string.Format("{0} ({1})", reason, code))
        {
            this.Code = code;
            this.Reason = reason;
        }
    }

    /// <summary>
    /// Raised when the cached key for a particular host does not match the
    /// key given by the host, which can be indicative of a man-in-the-middle
    /// attack. When rescuing this exception, you can inspect the key fingerprint
    /// and, if you want to proceed anyway, simply call the remember_host!
    /// method on the exception, and then retry.
    /// </summary>
    public class HostKeyMismatchException : Exception
    {
        public HostKeyMismatchException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// The callback to use when RememberHost is called
        /// </summary>
        public Action Callback { get; set; }
        public HostKeyVerificationData VerificationData { get; set; }

        /// <summary>
        /// Returns the fingerprint of the key for the host, which either was not
        /// found or did not match.</summary>
        public string Fingerprint
        {
            get
            {
                if (this.VerificationData != null)
                    return this.VerificationData.Fingerprint;

                return null;
            }
        }

        /// <summary>
        /// Returns the host name for the remote host, as reported by the socket.
        /// </summary>
        public string Host
        {
            get
            {
                if (this.VerificationData != null && this.VerificationData.Peer != null)
                    return this.VerificationData.Peer.Host;

                return null;
            }
        }

        /// <summary>
        /// Returns the port number for the remote host, as reported by the socket.
        /// </summary>
        public int? Port
        {
            get
            {
                if (this.VerificationData != null && this.VerificationData.Peer != null)
                    return this.VerificationData.Peer.Port;

                return null;
            }
        }

        /// <summary>
        /// Returns the IP address of the remote host, as reported by the socket.
        /// </summary>
        public IPAddress IPAddress
        {
            get
            {
                if (this.VerificationData != null && this.VerificationData.Peer != null)
                    return this.VerificationData.Peer.IPAddress;

                return null;
            }
        }

        /// <summary>
        /// Returns the key itself, as reported by the remote host.
        /// </summary>
        public Key Key
        {
            get
            {
                if (this.VerificationData != null)
                    return this.VerificationData.Key;

                return null;
            }
        }

        /// <summary>
        /// Tell Net::SSH to record this host and key in the known hosts file, so
        /// that subsequent connections will remember them.
        /// </summary>
        public void RememberHost()
        {
            this.Callback.Invoke();
        }
    }
}