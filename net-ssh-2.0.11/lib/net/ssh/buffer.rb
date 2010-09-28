require 'net/ssh/ruby_compat'
require 'net/ssh/transport/openssl'

module Net; module SSH


  class Buffer
    # This is a convenience method for creating and populating a new buffer
    # from a single command. The arguments must be even in length, with the
    # first of each pair of arguments being a symbol naming the type of the
    # data that follows. If the type is :raw, the value is written directly
    # to the hash.
    #
    #   b = Buffer.from(:byte, 1, :string, "hello", :raw, "\1\2\3\4")
    #   #-> "\1\0\0\0\5hello\1\2\3\4"
    #
    # The supported data types are:
    #
    # * :raw => write the next value verbatim (#write)
    # * :int64 => write an 8-byte integer (#write_int64)
    # * :long => write a 4-byte integer (#write_long)
    # * :byte => write a single byte (#write_byte)
    # * :string => write a 4-byte length followed by character data (#write_string)
    # * :bool => write a single byte, interpreted as a boolean (#write_bool)
    # * :bignum => write an SSH-encoded bignum (#write_bignum)
    # * :key => write an SSH-encoded key value (#write_key)
    #
    # Any of these, except for :raw, accepts an Array argument, to make it
    # easier to write multiple values of the same type in a briefer manner.
    def self.from(*args)
      raise ArgumentError, "odd number of arguments given" unless args.length % 2 == 0

      buffer = new
      0.step(args.length-1, 2) do |index|
        type = args[index]
        value = args[index+1]
        if type == :raw
          buffer.append(value.to_s)
        elsif Array === value
          buffer.send("write_#{type}", *value)
        else
          buffer.send("write_#{type}", value)
        end
      end

      buffer
    end
   

  end
end; end;