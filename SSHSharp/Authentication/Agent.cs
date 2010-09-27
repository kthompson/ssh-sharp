using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SSHSharp.Authentication.Pageant;

namespace SSHSharp.Authentication
{
 
    /// <summary>
    /// A trivial exception class for representing agent-specific errors.
    /// </summary>
    public class AgentException : Exception
    {
        public AgentException()
        {
        }

        public AgentException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// An exception for indicating that the SSH agent is not available.
    /// </summary>
    public class AgentNotAvailableException : AgentException
    {
        public AgentNotAvailableException(string message)
            : base(message)
        {

        }

        public AgentNotAvailableException()
        {
        }
    }

    public class AgentPacket
    {
        public int Type { get; private set; }
        public Buffer Data { get; private set; }

        public AgentPacket(int type, Buffer data)
        {
            this.Type = type;
            this.Data = data;
        }
    }

    public class Agent
    {
        public const int Ssh2AgentRequestVersion = 1;
        public const int Ssh2AgentRequestIdentities = 11;
        public const int Ssh2AgentIdentitiesAnswer = 12;
        public const int Ssh2AgentSignRequest = 13;
        public const int Ssh2AgentSignResponse = 14;
        public const int Ssh2AgentFailure = 30;
        public const int Ssh2AgentVersionResponse = 103;

        public const int SshComAgent2Failure = 102;

        public const int SshAgentRequestRsaIdentities = 1;
        public const int SshAgentRsaIdentitiesAnswer1 = 2;
        public const int SshAgentRsaIdentitiesAnswer2 = 5;
        public const int SshAgentFailure = 5;

        /// <summary>
        /// The underlying socket being used to communicate with the SSH agent.
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// Instantiates a new agent object, connects to a running SSH agent,
        /// negotiates the agent protocol version, and returns the agent object.
        /// </summary>
        /// <returns></returns>
        public static Agent Connect()
        {
            var agent = new Agent();
            agent.Connect2();
            agent.Negotiate();
            return agent;
        }

        /// <summary>
        /// Connect to the agent process using the socket factory and socket name
        /// given by the attribute writers. If the agent on the other end of the
        /// socket reports that it is an SSH2-compatible agent, this will fail
        /// (it only supports the ssh-agent distributed by OpenSSH).
        /// </summary>
        /// <exception cref="AgentNotAvailableException"></exception>
        public void Connect2()
        {
            try
            {
                Trace.TraceInformation("connecting to ssh-agent");

                this.Socket = AgentSocketFactory().Open(Environment.GetEnvironmentVariable("SSH_AUTH_SOCK"));
            }
            catch (System.Exception e)
            {
                Trace.TraceError("could not connect to ssh-agent");
                throw new AgentNotAvailableException(e.Message);
            }
        }

        private object AgentSocketFactory()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to negotiate the SSH agent protocol version. Raises an error
        /// if the version could not be negotiated successfully.
        /// </summary>
        public void Negotiate()
        {
            // determine what type of agent we're communicating with
            var packet = SendAndWait(Ssh2AgentRequestVersion,
                                     new Buffer().WriteString(Transport.ServerVersion.ProtoVersion));

      //if type == SSH2_AGENT_VERSION_RESPONSE
      //  raise NotImplementedError, "SSH2 agents are not yet supported"
      //elsif type != SSH_AGENT_RSA_IDENTITIES_ANSWER1 && type != SSH_AGENT_RSA_IDENTITIES_ANSWER2
      //  raise AgentError, "unknown response from agent: #{type}, #{body.to_s.inspect}"
      //end
        }

        /// <summary>
        /// Send a new packet of the given type, with the associated data.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="buffer"></param>
        private void SendPacket(int type, object buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Read the next packet from the agent. This will return a two-part
        /// tuple consisting of the packet type, and the packet's body (which
        /// is returned as a SSHSharp.Buffer).
        /// </summary>
        /// <returns></returns>
        private AgentPacket ReadPacket()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send the given packet and return the subsequent reply from the agent.
        /// (See SendPacket and ReadPacket).
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private AgentPacket SendAndWait(int type, Buffer data)
        {
            SendPacket(type, data);
            return ReadPacket();
        }

        /// <summary>
        /// Returns +true+ if the parameter indicates a "failure" response from
        /// the agent, and +false+ otherwise.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool AgentFailed(int type)
        {
            return (type == SshAgentFailure ||
                    type == Ssh2AgentFailure ||
                    type == SshComAgent2Failure);
        }
    }
}
