using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Math;
using NUnit.Framework;
using SSHSharp;

namespace SSHSharp.Tests
{
    [TestFixture]
    public class TestBuffer
    {
        [Test]
        public void ConstructorShouldInitializeBufferToEmptyByDefault()
        {
            var buffer = new Buffer();
            Assert.That(buffer.IsEmpty);
            Assert.AreEqual(0, buffer.Position);
        }

        [Test]
        public void ConstructorWithStringShouldInitializeBufferToTheString()
        {
            var buffer = new Buffer("hello");
            Assert.That(!buffer.IsEmpty);
            Assert.AreEqual("hello", buffer.ToString());
            Assert.AreEqual(0, buffer.Position);
        }

        [Test]
        public void ReadWithoutArgumentShouldReadToEnd()
        {
            var buffer = new Buffer("hello world");
            Assert.AreEqual("hello world", buffer.Read());
            Assert.That(buffer.IsEof);
            Assert.AreEqual(11, buffer.Position);
        }

        [Test]
        public void ReadWithArgumentThatIsLessThanLengthShouldReadThatManyBytes()
        {
            var buffer = new Buffer("hello world");
            Assert.AreEqual("hello", buffer.Read(5));
            Assert.AreEqual(5, buffer.Position);
        }

        [Test]
        public void ReadWithArgumentThatIsMoreThanLengthShouldReadNoMoreThanLength()
        {
            var buffer = new Buffer("hello world");
            Assert.AreEqual("hello world", buffer.Read(500));
            Assert.AreEqual(11, buffer.Position);
        }

        [Test]
        public void ReadAtEofShouldReturnEmptyString()
        {
            var buffer = new Buffer("hello");
            buffer.Position = 5;
            Assert.AreEqual("", buffer.Read());
        }

        [Test]
        public void ConsumeWithoutArgumentShouldResizeBufferToStartAtPosition()
        {
            var buffer = new Buffer("hello world");
            buffer.Read(5);
            Assert.AreEqual(5, buffer.Position);
            Assert.AreEqual(11, buffer.Length);
            buffer.Consume();
            Assert.AreEqual(0, buffer.Position);
            Assert.AreEqual(6, buffer.Length);
            Assert.AreEqual(" world", buffer.ToString());
        }

        [Test]
        public void ConsumeWithArgumentShouldResizeBufferStartingAtN()
        {
            var buffer = new Buffer("hello world");
            Assert.AreEqual(0, buffer.Position);
            buffer.Consume(5);
            Assert.AreEqual(0, buffer.Position);
            Assert.AreEqual(6, buffer.Length);
            Assert.AreEqual(" world", buffer.ToString());
        }

        [Test]
        public void ReadHardShouldReadAndConsumeAndReturnReadPortion()
        {
            var buffer = new Buffer("hello world");
            Assert.AreEqual("hello", buffer.ReadHard(5));
            Assert.AreEqual(0, buffer.Position);
            Assert.AreEqual(6, buffer.Length);
            Assert.AreEqual(" world", buffer.ToString());
        }

        [Test]
        public void AvailableShouldReturnLengthAfterPositionToEndOfString()
        {
            var buffer = new Buffer("hello world");
            buffer.Read(5);
            Assert.AreEqual(6, buffer.Available);
        }

        [Test]
        public void ClearBangShouldResetBufferContentsAndCounters()
        {
            var buffer = new Buffer("hello world");
            buffer.Read(5);
            buffer.Clear();
            Assert.AreEqual(0, buffer.Position);
            Assert.AreEqual(0, buffer.Length);
            Assert.AreEqual("", buffer.ToString());
        }

        [Test]
        public void AppendShouldAppendArgumentWithoutChangingPositionAndShouldReturnSelf()
        {
            var buffer = new Buffer("hello world");
            buffer.Read(5);
            buffer.Append(" again");
            Assert.AreEqual(5, buffer.Position);
            Assert.AreEqual(12, buffer.Available);
            Assert.AreEqual(17, buffer.Length);
            Assert.AreEqual("hello world again", buffer.ToString());
        }

        [Test]
        public void RemainderAsBufferShouldReturnANewBufferFilledWithTheTextAfterTheCurrentPosition()
        {
            var buffer = new Buffer("hello world");
            buffer.Read(6);
            var b2 = buffer.RemainerAsBuffer();
            Assert.AreEqual(6, buffer.Position);
            Assert.AreEqual(0, b2.Position);
            Assert.AreEqual("world", b2.ToString());
        }

        [Test]
        public void ReadInt64ShouldReturn8ByteInteger()
        {
            var buffer = new Buffer("\xff\xee\xdd\xcc\xbb\xaa\x99\x88");
            var value = buffer.ReadInt64();
            Assert.NotNull(value);
            Assert.AreEqual(unchecked((Int64)0xffeeddccbbaa9988), value.Value);
            Assert.AreEqual(8, buffer.Position);
        }

        [Test]
        public void ReadInt64ShouldReturnNilOnPartialRead()
        {
            var buffer = new Buffer("\0\0\0\0\0\0\0");
            Assert.IsNull(buffer.ReadInt64());
            Assert.That(buffer.IsEof);
        }

        [Test]
        public void TestReadInt32ShouldReturn4ByteInteger()
        {
            var buffer = new Buffer("\xff\xee\xdd\xcc\xbb\xaa\x99\x88");
            Assert.AreEqual(unchecked((Int32)0xffeeddcc), buffer.ReadInt32());
            Assert.AreEqual(4, buffer.Position);
        }

        [Test]
        public void ReadInt32ShouldReturnNilOnPartialRead()
        {
            var buffer = new Buffer("\0\0\0");
            Assert.IsNull(buffer.ReadInt32());
            Assert.That(buffer.IsEof);
        }

        [Test]
        public void ReadByteShouldReturnSingleByteInteger()
        {
            var buffer = new Buffer("\xfe\xdc");
            Assert.AreEqual((byte)0xfe, buffer.ReadByte());
            Assert.AreEqual(1, buffer.Position);
        }

        [Test]
        public void ReadByteShouldReturnNilAtEof()
        {
            var buffer = new Buffer();
            Assert.IsNull(buffer.ReadByte());
        }

        [Test]
        public void ReadStringShouldReadLengthAndDataFromBuffer()
        {
            var buffer = new Buffer("\0\0\0\x0bhello world");
            Assert.AreEqual("hello world", buffer.ReadString());
        }

        [Test]
        public void ReadStringShouldReturnNilIf4ByteLengthCannotBeRead()
        {
            Assert.IsNull(new Buffer("\0\x1").ReadString());
        }

        [Test]
        public void ReadBoolShouldReturnTrueIfNonZeroByteIsRead()
        {
            var buffer = new Buffer("\x1\x2\x3\x4\x5\x6");
            6.Times(() => Assert.AreEqual(true, buffer.ReadBoolean()));
        }

        [Test]
        public void ReadBoolShouldReturnFalseIfZeroByteIsRead()
        {
            var buffer = new Buffer("\x0");
            Assert.AreEqual(false, buffer.ReadBoolean());
        }

        [Test]
        public void TestReadBoolShouldReturnNilAtEof()
        {
            Assert.IsNull(new Buffer().ReadBoolean());
        }

        [Test]
        public void ReadBufferShouldReadAStringAndReturnItWrappedInABuffer()
        {
            var buffer = new Buffer("\0\0\0\x0bhello world");

            var b2 = buffer.ReadBuffer();

            Assert.AreEqual(0, b2.Position);
            Assert.AreEqual(11, b2.Length);
            Assert.AreEqual("hello world", b2.Read());
        }

        [Test]
        public void ResetBangShouldResetPositionTo0()
        {
            var buffer = new Buffer("hello world");
            buffer.Read(5);
            Assert.AreEqual(5, buffer.Position);
            buffer.Reset();
            Assert.AreEqual(0, buffer.Position);
        }

        [Test]
        public void ReadToShouldReturnNilIfPatternDoesNotExistInBuffer()
        {
            var buffer = new Buffer("one two three");
            Assert.Null(buffer.ReadTo("\n"));
        }

        [Test]
        public void ReadToShouldGrokStringPatterns()
        {
            var buffer = new Buffer("one two three");
            Assert.AreEqual("one tw", buffer.ReadTo("tw"));
            Assert.AreEqual(6, buffer.Position);
        }

        [Test]
        public void ReadToShouldGrokFixnumPatterns()
        {
            var buffer = new Buffer("one two three");
            Assert.AreEqual("one tw", buffer.ReadTo('w'));
            Assert.AreEqual(6, buffer.Position);
        }

        [Test]
        public void ReadToShouldGrokRegexPatterns()
        {
            var buffer = new Buffer("one two three");
            Assert.AreEqual("one tw", buffer.ReadTo(new Regex("tw")));
            Assert.AreEqual(6, buffer.Position);
        }

        [Test]
        public void ReadBignumShouldReadOpensslFormattedBignum()
        {
            var buffer = new Buffer("\0\0\0\x4I\x96\x2\xd2");

            var bn = buffer.ReadBigNum();
            Assert.AreEqual("1234567890", bn.ToString());
        }

        [Test]
        public void ReadBignumShouldReturnNilIfLengthCannotBeRead()
        {
            Assert.Null(new Buffer("\0\x1\x2").ReadBigNum());
        }

        [Test]
        public void ReadKeyBlobShouldReadDsaKeys()
        {
            RandomDss(buffer => buffer.ReadKeyBlob("ssh-dss"));
        }

        [Test]
        public void ReadKeyBlobShouldReadRsaKeys()
        {
            RandomRsa(buffer => buffer.ReadKeyBlob("ssh-rsa"));
        }

        [Test]
        public void ReadKeyShouldReadDsaKeyTypeAndKeyblob()
        {
            RandomDss(buffer => new Buffer()
                                    .WriteString("ssh-dss")
                                    .Write(buffer.ToString())
                                    .ReadKey());
        }

        [Test]
        public void ReadKeyShouldReadRsaKeyTypeAndKeyblob()
        {
            RandomRsa(buffer => new Buffer()
                                    .WriteString("ssh-rsa")
                                    .Write(buffer.ToString())
                                    .ReadKey());
        }

        [Test]
        public void WriteShouldWriteArgumentsDirectlyToEndBuffer()
        {
            var buffer = new Buffer("start");
            buffer.Write("hello", " ", "world");
            Assert.AreEqual("starthello world", buffer.ToString());
            Assert.AreEqual(0, buffer.Position);
        }
        [Test]
        public void WriteInt64ShouldWriteArgumentsAs8ByteIntegersToEndOfBuffer()
        {
            var buffer = new Buffer("start");
            buffer.WriteInt64(unchecked((Int64)0xffeeddccbbaa9988), 0x7766554433221100);
            Assert.AreEqual("start\xff\xee\xdd\xcc\xbb\xaa\x99\x88\x77\x66\x55\x44\x33\x22\x11\x00", buffer.ToString());
        }

        [Test]
        public void WriteInt32ShouldWriteArgumentsAs4ByteIntegersToEndOfBuffer()
        {
            var buffer = new Buffer("start");
            buffer.WriteInt32(unchecked((Int32)0xffeeddcc), unchecked((Int32)0xbbaa9988));
            Assert.AreEqual("start\xff\xee\xdd\xcc\xbb\xaa\x99\x88", buffer.ToString());
        }

        [Test]
        public void WriteByteShouldWriteArgumentsAs1ByteIntegersToEndOfBuffer()
        {
            var buffer = new Buffer("start");
            buffer.WriteByte(1, 2, 3, 4, 5);
            Assert.AreEqual("start\x1\x2\x3\x4\x5", buffer.ToString());
        }

        [Test]
        public void WriteBoolShouldWriteArgumentsAs1ByteBooleanValuesToEndOfBuffer()
        {
            var buffer = new Buffer("start");
            buffer.WriteBool(null, false, true, 1, new Object());
            Assert.AreEqual("start\0\0\x1\x1\x1", buffer.ToString());
        }

        [Test]
        public void WriteDssKeyShouldWriteArgumentToEndOfBuffer()
        {
            var buffer = new Buffer("start");

            var key = new DsaKey();

            key.P = new BigInteger(new byte[] {0x0, 0xff, 0xee, 0xdd, 0xcc, 0xbb, 0xaa, 0x99, 0x88});
            key.Q = new BigInteger(new byte[] {0x77, 0x66, 0x55, 0x44, 0x33, 0x22, 0x11, 0x00});
            key.G = new BigInteger(new byte[] {0, 0xff, 0xdd, 0xbb, 0x99, 0x77, 0x55, 0x33, 0x11});
            key.X = new BigInteger(new byte[] {0xee, 0xcc, 0xaa, 0x88, 0x66, 0x44, 0x22, 0x00});

            buffer.WriteKey(key);
            Assert.AreEqual(
                "start\0\0\0\x7ssh-dss\0\0\0\x8\xff\xee\xdd\xcc\xbb\xaa\x99\x88\0\0\0\x8\x77\x66\x55\x44\x33\x22\x11\x00\0\0\0\x8\xff\xdd\xbb\x99\x77\x55\x33\x11\0\0\0\x8\xee\xcc\xaa\x88\x66\x44\x22\x00",
                buffer.ToString());

        }

        [Test]
        public void WriteRsaKeyShouldWriteArgumentToEndOfBuffer()
        {
            var buffer = new Buffer("start");

            var key = new RsaKey();

            key.Exponent = new BigInteger(new byte[] { 0xff, 0xee, 0xdd, 0xcc, 0xbb, 0xaa, 0x99, 0x88 });
            key.Modulus = new BigInteger(new byte[] { 0x77, 0x66, 0x55, 0x44, 0x33, 0x22, 0x11, 0x00 });

            buffer.WriteKey(key);
            Assert.AreEqual(
                "start\0\0\0\x7ssh-rsa\0\0\0\x8\xff\xee\xdd\xcc\xbb\xaa\x99\x88\0\0\0\x8\x77\x66\x55\x44\x33\x22\x11\x00",
                buffer.ToString());
        }

        [Test]
        public void WriteBignumShouldWriteArgumentsAsSshFormattedBignumValuesToEndOfBuffer()
        {
            var buffer = new Buffer("start");
            buffer.WriteBigNum(new BigInteger(new byte[] {0x49, 0x96, 0x02, 0xD2}));
            Assert.AreEqual("start\0\0\0\x4I\x96\x2\xD2", buffer.ToString());
        }

        #region helper functions

        private static void RandomDss(Func<Buffer, Key> yield)
        {
            var n1 = RandomBigNum();
            var n2 = RandomBigNum();
            var n3 = RandomBigNum();
            var n4 = RandomBigNum();

            var buffer = new Buffer()
                .WriteBigNum(n1)
                .WriteBigNum(n2)
                .WriteBigNum(n3)
                .WriteBigNum(n4);

            var key = (DsaKey)yield(buffer);

            Assert.AreEqual("ssh-dss", key.SshType);
            Assert.AreEqual(n1, key.P);
            Assert.AreEqual(n2, key.Q);
            Assert.AreEqual(n3, key.G);
            Assert.AreEqual(n4, key.X);
        }
        private static void RandomRsa(Func<Buffer, Key> yield)
        {
            var n1 = RandomBigNum();
            var n2 = RandomBigNum();

            var buffer = new Buffer()
                .WriteBigNum(n1)
                .WriteBigNum(n2);

            var key = (RsaKey)yield(buffer);

            Assert.AreEqual("ssh-rsa", key.SshType);
            Assert.AreEqual(n1, key.Exponent);
            Assert.AreEqual(n2, key.Modulus);
        }

        private static BigInteger RandomBigNum()
        {
            return BigInteger.GenerateRandom(128);
        }
        #endregion
    }
}