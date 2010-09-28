require 'net/ssh/transport/openssl'
require 'net/ssh/transport/constants'

module Net; module SSH; module Transport; module Kex

  # A key-exchange service implementing the "diffie-hellman-group1-sha1"
  # key-exchange algorithm.
  class DiffieHellmanGroup1SHA1
    include Constants, Loggable

 


    
    attr_reader :digester
    attr_reader :algorithms
    attr_reader :connection
    attr_reader :data
    attr_reader :dh

    # Create a new instance of the DiffieHellmanGroup1SHA1 algorithm.
    # The data is a Hash of symbols representing information
    # required by this algorithm, which was acquired during earlier
    # processing.
    def initialize(algorithms, connection, data)
      @p = OpenSSL::BN.new(P_s, P_r)
      @g = G

      @digester = OpenSSL::Digest::SHA1
      @algorithms = algorithms
      @connection = connection

      @data = data.dup
      @dh = generate_key
      @logger = @data.delete(:logger)
    end

    # Perform the key-exchange for the given session, with the given
    # data. This method will return a hash consisting of the
    # following keys:
    #
    # * :session_id
    # * :server_key
    # * :shared_secret
    # * :hashing_algorithm
    #
    # The caller is expected to be able to understand how to use these
    # deliverables.
    def exchange_keys
      result = send_kexinit
      verify_server_key(result[:server_key])
      session_id = verify_signature(result)
      confirm_newkeys

      return { :session_id        => session_id, 
               :server_key        => result[:server_key],
               :shared_secret     => result[:shared_secret],
               :hashing_algorithm => digester }
    end

    private
    
      # Returns the DH key parameters for the current connection.
      def get_parameters
        [p, g]
      end

      # Returns the INIT/REPLY constants used by this algorithm.
      def get_message_types
        [KEXDH_INIT, KEXDH_REPLY]
      end

      # Build the signature buffer to use when verifying a signature from
      # the server.
      def build_signature_buffer(result)
        response = Net::SSH::Buffer.new
        response.write_string data[:client_version_string],
                              data[:server_version_string],
                              data[:client_algorithm_packet],
                              data[:server_algorithm_packet],
                              result[:key_blob]
        response.write_bignum dh.pub_key,
                              result[:server_dh_pubkey],
                              result[:shared_secret]
        response
      end

      # Generate a DH key with a private key consisting of the given
      # number of bytes.
      def generate_key #:nodoc:
        dh = OpenSSL::PKey::DH.new

        dh.p, dh.g = get_parameters
        dh.priv_key = OpenSSL::BN.rand(data[:need_bytes] * 8)

        dh.generate_key! until dh.valid?

        dh
      end

      # Send the KEXDH_INIT message, and expect the KEXDH_REPLY. Return the
      # resulting buffer.
      #
      # Parse the buffer from a KEXDH_REPLY message, returning a hash of
      # the extracted values.
      def send_kexinit #:nodoc:
        init, reply = get_message_types

        # send the KEXDH_INIT message
        buffer = Net::SSH::Buffer.from(:byte, init, :bignum, dh.pub_key)
        connection.send_message(buffer)

        # expect the KEXDH_REPLY message
        buffer = connection.next_message
        raise Net::SSH::Exception, "expected REPLY" unless buffer.type == reply

        result = Hash.new

        result[:key_blob] = buffer.read_string
        result[:server_key] = Net::SSH::Buffer.new(result[:key_blob]).read_key
        result[:server_dh_pubkey] = buffer.read_bignum
        result[:shared_secret] = OpenSSL::BN.new(dh.compute_key(result[:server_dh_pubkey]), 2)

        sig_buffer = Net::SSH::Buffer.new(buffer.read_string)
        sig_type = sig_buffer.read_string
        if sig_type != algorithms.host_key
          raise Net::SSH::Exception,
            "host key algorithm mismatch for signature " +
            "'#{sig_type}' != '#{algorithms.host_key}'"
        end
        result[:server_sig] = sig_buffer.read_string

        return result
      end

      # Verify that the given key is of the expected type, and that it
      # really is the key for the session's host. Raise Net::SSH::Exception
      # if it is not.
      def verify_server_key(key) #:nodoc:
        if key.ssh_type != algorithms.host_key
          raise Net::SSH::Exception,
            "host key algorithm mismatch " +
            "'#{key.ssh_type}' != '#{algorithms.host_key}'"
        end

        blob, fingerprint = generate_key_fingerprint(key)

        unless connection.host_key_verifier.verify(:key => key, :key_blob => blob, :fingerprint => fingerprint, :session => connection)
          raise Net::SSH::Exception, "host key verification failed"
        end
      end

      def generate_key_fingerprint(key)
        blob = Net::SSH::Buffer.from(:key, key).to_s
        fingerprint = OpenSSL::Digest::MD5.hexdigest(blob).scan(/../).join(":")

        [blob, fingerprint]
      rescue ::Exception => e
        [nil, "(could not generate fingerprint: #{e.message})"]
      end

      # Verify the signature that was received. Raise Net::SSH::Exception
      # if the signature could not be verified. Otherwise, return the new
      # session-id.
      def verify_signature(result) #:nodoc:
        response = build_signature_buffer(result)

        hash = @digester.digest(response.to_s)

        unless result[:server_key].ssh_do_verify(result[:server_sig], hash)
          raise Net::SSH::Exception, "could not verify server signature"
        end

        return hash
      end

      # Send the NEWKEYS message, and expect the NEWKEYS message in
      # reply.
      def confirm_newkeys #:nodoc:
        # send own NEWKEYS message first (the wodSSHServer won't send first)
        response = Net::SSH::Buffer.new
        response.write_byte(NEWKEYS)
        connection.send_message(response)

        # wait for the server's NEWKEYS message
        buffer = connection.next_message
        raise Net::SSH::Exception, "expected NEWKEYS" unless buffer.type == NEWKEYS
      end
  end

end; end; end; end
