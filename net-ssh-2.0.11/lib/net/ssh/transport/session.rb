require 'socket'
require 'timeout'

require 'net/ssh/loggable'
require 'net/ssh/transport/algorithms'
require 'net/ssh/transport/constants'
require 'net/ssh/transport/packet_stream'

module Net; module SSH; module Transport

  
  class Session
    include Constants, Loggable

   
    

   

   

    # Returns the host (and possibly IP address) in a format compatible with
    # SSH known-host files.
    def host_as_string
      @host_as_string ||= begin
        string = "#{host}"
        string = "[#{string}]:#{port}" if port != DEFAULT_PORT
        if socket.peer_ip != host
          string2 = socket.peer_ip
          string2 = "[#{string2}]:#{port}" if port != DEFAULT_PORT
          string << "," << string2
        end
        string
      end
    end

    # Returns true if the underlying socket has been closed.
    def closed?
      socket.closed?
    end

    # Cleans up (see PacketStream#cleanup) and closes the underlying socket.
    def close
      socket.cleanup
      socket.close
    end

    # Performs a "hard" shutdown of the connection. In general, this should
    # never be done, but it might be necessary (in a rescue clause, for instance,
    # when the connection needs to close but you don't know the status of the
    # underlying protocol's state).
    def shutdown!
      error { "forcing connection closed" }
      socket.close
    end

    # Returns a new service_request packet for the given service name, ready
    # for sending to the server.
    def service_request(service)
      Net::SSH::Buffer.from(:byte, SERVICE_REQUEST, :string, service)
    end

    # Requests a rekey operation, and blocks until the operation completes.
    # If a rekey is already pending, this returns immediately, having no
    # effect.
    def rekey!
      if !algorithms.pending?
        algorithms.rekey!
        wait { algorithms.initialized? }
      end
    end

    # Returns immediately if a rekey is already in process. Otherwise, if a
    # rekey is needed (as indicated by the socket, see PacketStream#if_needs_rekey?)
    # one is performed, causing this method to block until it completes.
    def rekey_as_needed
      return if algorithms.pending?
      socket.if_needs_rekey? { rekey! }
    end

    # Returns a hash of information about the peer (remote) side of the socket,
    # including :ip, :port, :host, and :canonized (see #host_as_string).
    def peer
      @peer ||= { :ip => socket.peer_ip, :port => @port.to_i, :host => @host, :canonized => host_as_string }
    end

    # Blocks until a new packet is available to be read, and returns that
    # packet. See #poll_message.
    def next_message
      poll_message(:block)
    end

    # Tries to read the next packet from the socket. If mode is :nonblock (the
    # default), this will not block and will return nil if there are no packets
    # waiting to be read. Otherwise, this will block until a packet is
    # available. Note that some packet types (DISCONNECT, IGNORE, UNIMPLEMENTED,
    # DEBUG, and KEXINIT) are handled silently by this method, and will never
    # be returned.
    #
    # If a key-exchange is in process and a disallowed packet type is
    # received, it will be enqueued and otherwise ignored. When a key-exchange
    # is not in process, and consume_queue is true, packets will be first
    # read from the queue before the socket is queried.
    def poll_message(mode=:nonblock, consume_queue=true)
      loop do
        if consume_queue && @queue.any? && algorithms.allow?(@queue.first)
          return @queue.shift
        end

        packet = socket.next_packet(mode)
        return nil if packet.nil?

        case packet.type
        when DISCONNECT
          raise Net::SSH::Disconnect, "disconnected: #{packet[:description]} (#{packet[:reason_code]})"

        when IGNORE
          debug { "IGNORE packet recieved: #{packet[:data].inspect}" }

        when UNIMPLEMENTED
          lwarn { "UNIMPLEMENTED: #{packet[:number]}" }

        when DEBUG
          send(packet[:always_display] ? :fatal : :debug) { packet[:message] }

        when KEXINIT
          algorithms.accept_kexinit(packet)

        else
          return packet if algorithms.allow?(packet)
          push(packet)
        end
      end
    end



    # Adds the given packet to the packet queue. If the queue is non-empty,
    # #poll_message will return packets from the queue in the order they
    # were received.
    def push(packet)
      @queue.push(packet)
    end

    # Sends the given message via the packet stream, blocking until the
    # entire message has been sent.
    def send_message(message)
      socket.send_packet(message)
    end

    # Enqueues the given message, such that it will be sent at the earliest
    # opportunity. This does not block, but returns immediately.
    def enqueue_message(message)
      socket.enqueue_packet(message)
    end

    # Configure's the packet stream's client state with the given set of
    # options. This is typically used to define the cipher, compression, and
    # hmac algorithms to use when sending packets to the server.
    def configure_client(options={})
      socket.client.set(options)
    end

    # Configure's the packet stream's server state with the given set of
    # options. This is typically used to define the cipher, compression, and
    # hmac algorithms to use when reading packets from the server.
    def configure_server(options={})
      socket.server.set(options)
    end

    # Sets a new hint for the packet stream, which the packet stream may use
    # to change its behavior. (See PacketStream#hints).
    def hint(which, value=true)
      socket.hints[which] = value
    end

    public

      # this method is primarily for use in tests
      attr_reader :queue #:nodoc:


  end
end; end; end
