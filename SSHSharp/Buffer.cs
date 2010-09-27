using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Math;
using Mono.Security.Authenticode;

namespace SSHSharp
{
    /// <summary>
    /// SSHSharp.Buffer is a flexible class for building and parsing binary
    /// data packets. It provides a stream-like interface for sequentially
    /// reading data items from the buffer, as well as a useful helper method
    /// for building binary packets given a signature.
    /// 
    /// Writing to a buffer always appends to the end, regardless of where the
    /// read cursor is. Reading, on the other hand, always begins at the first
    /// byte of the buffer and increments the read cursor, with subsequent reads
    /// taking up where the last left off.
    /// 
    /// As a consumer of the SSHSharp library, you will rarely come into contact
    /// with these buffer objects directly, but it could happen. Also, if you
    /// are ever implementing a protocol on top of SSH (e.g. SFTP), this buffer
    /// class can be quite handy.
    /// </summary>
    [Incomplete]
    public class Buffer
    {
        #region properties

        // TODO: should this really be a string builder?

        /// <summary>
        /// exposes the raw content of the buffer
        /// </summary>
        public StringBuilder Content { get; private set; }

        /// <summary>
        /// the current position of the pointer in the buffer
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Returns the length of the buffer's content.
        /// </summary>
        public int Length
        {
            get { return this.Content.Length; }
        }

        /// <summary>
        /// Returns the number of bytes available to be read (e.g., how many bytes
        /// remain between the current position and the end of the buffer). 
        /// </summary>
        public int Available
        {
            get { return this.Length - this.Position; }
        }

        /// <summary>
        /// Returns +true+ if the buffer contains no data (e.g., it is of zero length).
        /// </summary>
        public bool IsEmpty
        {
            get { return this.Content.Length == 0; }
        }

        /// <summary>
        /// Returns true if the pointer is at the end of the buffer. Subsequent
        /// reads will return nil, in this case.
        /// </summary>
        public bool IsEof
        {
            get { return this.Position >= this.Length; }
        }

        #endregion

        #region ctor
        /// <summary>
        /// Creates a new buffer, initialized to the given content. The position
        /// is initialized to the beginning of the buffer. 
        /// </summary>
        /// <param name="content"></param>
        public Buffer(string content = "")
        {
            this.Content = new StringBuilder(content);
            this.Position = 0;
        }

        #endregion

        /// <summary>
        /// Returns a copy of the buffer's content.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Content.ToString();
        }

        /// <summary>
        /// Compares the contents of the two buffers, returning +true+ only if they
        /// are identical in size and content.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var buffer = obj as Buffer;
            if (buffer == null)
                return false;

            return this.ToString() == buffer.ToString();
        }

        public override int GetHashCode()
        {
            return this.Content.GetHashCode();
        }

        /// <summary>
        /// Resets the pointer to the start of the buffer. Subsequent reads will
        /// begin at position 0.
        /// </summary>
        public void Reset()
        {
            this.Position = 0;
        }

        /// <summary>
        /// Resets the buffer, making it empty. Also, resets the read position to
        /// 0.
        /// </summary>
        public void Clear()
        {
            this.Content.Clear();
            this.Position = 0;
        }

        /// <summary>
        /// Consumes n bytes from the buffer, where n is the current position
        /// unless otherwise specified. This is useful for removing data from the
        /// buffer that has previously been read, when you are expecting more data
        /// to be appended. It helps to keep the size of buffers down when they
        /// would otherwise tend to grow without bound.
        /// </summary>
        /// <returns>the buffer object itself</returns>
        public Buffer Consume(int num)
        {
            if (num>= this.Length)
            {
                this.Clear();
            }
            else if (num > 0)
            {
                this.Content.Remove(0, num);
                this.Position -= num;

                if(this.Position < 0) 
                    this.Position = 0;

            }
            return this;
        }

        /// <summary>
        /// Consumes n bytes from the buffer, where n is the current position
        /// unless otherwise specified. This is useful for removing data from the
        /// buffer that has previously been read, when you are expecting more data
        /// to be appended. It helps to keep the size of buffers down when they
        /// would otherwise tend to grow without bound.
        /// </summary>
        /// <returns>the buffer object itself</returns>
        public Buffer Consume()
        {
            return this.Consume(this.Position);
        }

        /// <summary>
        /// Appends the given text to the end of the buffer. Does not alter the
        /// read position. Returns the buffer object itself.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public Buffer Append(string text)
        {
            this.Content.Append(text);
            return this;
        }

        /// <summary>
        /// Returns all text from the current pointer to the end of the buffer as
        /// a new Net::SSH::Buffer object.
        /// </summary>
        /// <returns></returns>
        public Buffer RemainerAsBuffer()
        {
            return new Buffer(this.Content.ToString(this.Position, this.Available));
        }

        /// <summary>
        /// Reads and returns the next +count+ bytes from the buffer, starting from
        /// the read position. If +count+ is +nil+, this will return all remaining
        /// text in the buffer. This method will increment the pointer.
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        public string Read(int? cnt = null)
        {
            var count = cnt ?? this.Length;

            if (this.Position + count > this.Length)
                count = this.Length - this.Position;

            this.Position += count;

            return this.Content.ToString(this.Position - count, count);
        }

        /// <summary>
        /// Reads (as #Read) and returns the given number of bytes from the buffer,
        /// and then consumes (as #Consume) all data up to the new read position.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public string ReadHard(int? count = null)
        {
            var data = this.Read(count);
            this.Consume();
            return data;
        }

        /// <summary>
        /// Return the next 8 bytes as a 64-bit integer (in network byte order).
        /// Returns nil if there are less than 8 bytes remaining to be read in the
        /// buffer.
        /// </summary>
        /// <returns></returns>
        public Int64? ReadInt64()
        {
            Int64? hi = ReadInt32();
            if (hi == null)
                return null;

            Int64? lo = ReadInt32();
            if (lo == null)
                return null;

            return (hi.Value << 32) + (lo.Value & 0xffffffff);
        }

        /// <summary>
        /// Return the next four bytes as a long integer (in network byte order).
        /// Returns nil if there are less than 4 bytes remaining to be read in the
        /// buffer.
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use ReadInt32 instead")]
        public Int32? ReadLong()
        {
            return ReadInt32();
        }

        /// <summary>
        /// Return the next four bytes as a long integer (in network byte order).
        /// Returns nil if there are less than 4 bytes remaining to be read in the
        /// buffer.
        /// </summary>
        /// <returns></returns>
        public Int32? ReadInt32()
        {
            var bs = this.ReadBytes(4);
            if(bs == null)
                return null;

            return bs.AsInt32();
        }

        /// <summary>
        /// Read and return the next byte in the buffer. Returns nil if called at
        /// the end of the buffer.
        /// </summary>
        /// <returns></returns>
        public byte? ReadByte()
        {
            var b = this.ReadBytes(1);
            if (b == null)
                return null;

            return b[0];
        }

        public byte[] ReadBytes(int count)
        {
            var s = this.Read(count);
            if(s == null || s.Length != count)
                return null;

            return s.ToByteArray();
            //return Encoding.Default.GetBytes(s);
        }

        /// <summary>
        /// Read a single byte and convert it into a boolean, using 'C' rules
        /// (i.e., zero is false, non-zero is true).
        /// </summary>
        /// <returns></returns>
        public bool? ReadBoolean()
        {
            var b = this.ReadByte();
            if (b == null)
                return null;

            return b != 0;
        }

        /// <summary>
        /// Read and return an SSH2-encoded string. The string starts with a long
        /// integer that describes the number of bytes remaining in the string.
        /// Returns nil if there are not enough bytes to satisfy the request.
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            var length = this.ReadInt32();
            if (length == null)
                return null;

            return Read(length);
        }

        /// <summary>
        /// Reads the next string from the buffer, and returns a new Buffer
        /// object that wraps it.
        /// </summary>
        /// <returns></returns>
        public Buffer ReadBuffer()
        {
            return new Buffer(this.ReadString());
        }

        /// <summary>
        /// Read a BigNum from the buffer, in SSH2 format. It is
        /// essentially just a string, which is reinterpreted to be a BigNum in
        /// binary format.
        /// </summary>
        /// <returns></returns>
        public BigInteger ReadBigNum()
        {
            var length = this.ReadInt32();
            if (length == null)
                return null;

            var bytes = ReadBytes(length.Value);

            return new BigInteger(bytes);
        }

        /// <summary>
        /// Read a keyblob of the given type from the buffer, and return it as
        /// a key. Only RSA and DSA keys are supported.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public Key ReadKeyBlob(string type)
        {
            switch (type)
            {
                case "ssh-dss":
                    var dsakey = new DsaKey();
                    dsakey.P = ReadBigNum();
                    dsakey.Q = ReadBigNum();
                    dsakey.G = ReadBigNum();
                    dsakey.X = ReadBigNum(); //pub
                    return dsakey;
                case "ssh-rsa":
                    var rsakey = new RsaKey();

                    rsakey.Exponent = ReadBigNum();
                    rsakey.Modulus = ReadBigNum();

                    return rsakey;
                default:
                    throw new NotSupportedException(string.Format("unsupported key type `{0}'", type));
            }
        }

        /// <summary>
        /// Read a key from the buffer. The key will start with a string
        /// describing its type. The remainder of the key is defined by the
        /// type that was read.
        /// </summary>
        /// <returns></returns>
        public Key ReadKey()
        {
            var type = ReadString();
            if (type == null)
                return null;

            return ReadKeyBlob(type);
        }

        /// <summary>
        /// Reads all data up to and including the given pattern. 
        /// Returns nil if nothing matches. Increments the position to point
        /// immediately after the pattern, if it does match. Returns all data up to
        /// and including the text that matched the pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public string ReadTo(string pattern)
        {
            var index = this.Content.IndexOf(pattern, this.Position);
            if (index == -1)
                return null;

            return Read(index + pattern.Length);
        }

        /// <summary>
        /// Reads all data up to and including the given pattern. 
        /// Returns nil if nothing matches. Increments the position to point
        /// immediately after the pattern, if it does match. Returns all data up to
        /// and including the text that matched the pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public string ReadTo(int pattern)
        {
            var index = this.Content.IndexOf(pattern, this.Position);
            if (index == -1)
                return null;

            return Read(index + 1);
        }

        /// <summary>
        /// Reads all data up to and including the given pattern. 
        /// Returns nil if nothing matches. Increments the position to point
        /// immediately after the pattern, if it does match. Returns all data up to
        /// and including the text that matched the pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public string ReadTo(Regex pattern)
        {
            Match match = pattern.Match(this.Content, this.Position);
            
            if (match == null)
                return null;

            return Read(match.Index + match.Length);
        }

        /// <summary>
        /// Writes the given data literally into the string. Does not alter the
        /// read position. Returns the buffer object.
        /// </summary>
        /// <param name="strings"></param>
        /// <returns></returns>
        public Buffer Write(params string[] strings)
        {
            foreach (var s in strings)
                this.Content.Append(s);

            return this;
        }

        /// <summary>
        /// Writes each argument to the buffer as an SSH2-encoded string. Each
        /// string is prefixed by its length, encoded as a 4-byte long integer.</summary>
        /// Does not alter the read position. Returns the buffer object.
        /// <param name="strings"></param>
        /// <returns></returns>
        public Buffer WriteString(params string[] strings)
        {
            foreach (var s in strings)
            {
                WriteInt32(s.Length);
                Write(s);
            }
            return this;
        }

        /// <summary>
        /// Writes each argument to the buffer as a network-byte-order-encoded
        /// 64-bit integer (8 bytes). Does not alter the read position. Returns the</summary>
        /// buffer object.
        /// <param name="longs"></param>
        /// <returns></returns>
        public Buffer WriteInt64(params long[] longs)
        {
            foreach (var l in longs)
                this.WriteByte(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(l)));

            return this;
        }

        /// <summary>
        /// Writes each argument to the buffer as a bignum (SSH2-style). No
        /// checking is done to ensure that the arguments are, in fact, bignums.
        /// Does not alter the read position. Returns the buffer object.
        /// </summary>
        /// <param name="bns"></param>
        /// <returns></returns>
        public Buffer WriteBigNum(params BigInteger[] bns)
        {
            foreach (var digits in bns.Select(bn => bn.GetBytes()))
            {
                WriteInt32(digits.Length);
                WriteByte(digits);
            }

            return this;
        }

        /// <summary>
        /// Writes each argument to the buffer as a network-byte-order-encoded
        /// long (4-byte) integer. Does not alter the read position. Returns the</summary>
        /// buffer object.
        /// <param name="ints"></param>
        /// <returns></returns>
        public Buffer WriteInt32(params int[] ints)
        {
            foreach (var i in ints)
                this.WriteByte(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i)));

            return this;
        }

        /// <summary>
        /// Writes each argument to the buffer as a byte. Does not alter the read
        /// position. Returns the buffer object.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public Buffer WriteByte(params byte[] bytes)
        {
            foreach (var b in bytes)
                this.Content.Append((char)b);

            return this;
        }

        /// <summary>
        /// Writes each argument to the buffer as a byte. Does not alter the read
        /// position. Returns the buffer object.
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public Buffer WriteBool(params object[] objects)
        {
            foreach (var o in objects)
            {
                if (o == null)
                    this.WriteByte(BitConverter.GetBytes(false));
                else if (o is Boolean)
                    this.WriteByte(BitConverter.GetBytes((Boolean)o));
                else if(o is IConvertible)
                    this.WriteByte(BitConverter.GetBytes(Convert.ToInt32(o) != 0));
                else
                    this.WriteByte(BitConverter.GetBytes(true));
            }
                
            return this;
        }

        /// <summary>
        /// Writes the given arguments to the buffer as SSH2-encoded keys. Does not
        /// alter the read position. Returns the buffer object.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Buffer WriteKey(params Key[] keys)
        {
            foreach (var key in keys)
            {
                this.Write(key.ToBlob());
            }

            return this;
        }
    }
}
