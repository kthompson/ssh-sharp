require 'common'
require 'net/ssh/buffer'

class TestBuffer < Test::Unit::TestCase

  def test_from_should_require_an_even_number_of_arguments
    assert_raises(ArgumentError) { Net::SSH::Buffer.from("this") }
  end

  def test_from_should_build_new_buffer_from_definition
    buffer = Net::SSH::Buffer.from(:byte, 1, :long, 2, :int64, 3, :string, "4", :bool, true, :bool, false, :bignum, OpenSSL::BN.new("1234567890", 10), :raw, "something")
    assert_equal "\1\0\0\0\2\0\0\0\0\0\0\0\3\0\0\0\0014\1\0\000\000\000\004I\226\002\322something", buffer.to_s
  end

  def test_from_with_array_argument_should_write_multiple_of_the_given_type
    buffer = Net::SSH::Buffer.from(:byte, [1,2,3,4,5])
    assert_equal "\1\2\3\4\5", buffer.to_s
  end

end