require 'net/ssh/buffer'
require 'net/ssh/errors'
require 'net/ssh/loggable'
require 'net/ssh/transport/server_version'

require 'net/ssh/authentication/pageant' if File::ALT_SEPARATOR && !(RUBY_PLATFORM =~ /java/)

module Net; module SSH; module Authentication


  # This class implements a simple client for the ssh-agent protocol. It
  # does not implement any specific protocol, but instead copies the
  # behavior of the ssh-agent functions in the OpenSSH library (3.8).
  #
  # This means that although it behaves like a SSH1 client, it also has
  # some SSH2 functionality (like signing data).
  class Agent
    include Loggable

    # A simple module for extending keys, to allow comments to be specified
    # for them.
    module Comment
      attr_accessor :comment
    end


  
    # Creates a new Agent object, using the optional logger instance to
    # report status.
    def initialize(logger=nil)
      self.logger = logger
    end
	



    # Return an array of all identities (public keys) known to the agent.
    # Each key returned is augmented with a +comment+ property which is set
    # to the comment returned by the agent for that key.
    def identities
      type, body = send_and_wait(SSH2_AGENT_REQUEST_IDENTITIES)
      raise AgentError, "could not get identity count" if agent_failed(type)
      raise AgentError, "bad authentication reply: #{type}" if type != SSH2_AGENT_IDENTITIES_ANSWER

      identities = []
      body.read_long.times do
        key = Buffer.new(body.read_string).read_key
        key.extend(Comment)
        key.comment = body.read_string
        identities.push key
      end

      return identities
    end

    # Closes this socket. This agent reference is no longer able to
    # query the agent.
    def close
      @socket.close
    end

    # Using the agent and the given public key, sign the given data. The
    # signature is returned in SSH2 format.
    def sign(key, data)
      type, reply = send_and_wait(SSH2_AGENT_SIGN_REQUEST, :string, Buffer.from(:key, key), :string, data, :long, 0)

      if agent_failed(type)
        raise AgentError, "agent could not sign data with requested identity"
      elsif type != SSH2_AGENT_SIGN_RESPONSE
        raise AgentError, "bad authentication response #{type}"
      end

      return reply.read_string
    end

    private

      # Returns the agent socket factory to use.
      def agent_socket_factory
        if File::ALT_SEPARATOR
          Pageant::Socket
        else
          UNIXSocket
        end
      end

      # 
      def send_packet(type, *args)
        buffer = Buffer.from(*args)
        data = [buffer.length + 1, type.to_i, buffer.to_s].pack("NCA*")
        debug { "sending agent request #{type} len #{buffer.length}" }
        @socket.send data, 0
      end

  
      def read_packet
        buffer = Net::SSH::Buffer.new(@socket.read(4))
        buffer.append(@socket.read(buffer.read_long))
        type = buffer.read_byte
        debug { "received agent packet #{type} len #{buffer.length-4}" }
        return type, buffer
      end
 
  end

end; end; end
