using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Math;

namespace SSHSharp
{
    public abstract class Key
    {
        public abstract string SshType { get; }
        /// <summary>
        /// Converts the key to a blob, according to the SSH2 protocol.
        /// </summary>
        /// <returns></returns>
        public abstract string ToBlob();

        public abstract bool SshDoVerify(byte[] sig, string data);
        public abstract string SshDoSign(byte[] data);

    }

    public class RsaKey : Key
    {

        public BigInteger Exponent { get; set; }
        public BigInteger Modulus { get; set; }

        /// <summary>
        /// Returns "ssh-rsa", which is the description of this key type used by the
        /// SSH2 protocol.
        /// </summary>
        public override string SshType
        {
            get { return "ssh-rsa"; }
        }

        private string _blob;
        /// <summary>
        /// Converts the key to a blob, according to the SSH2 protocol.
        /// </summary>
        /// <returns></returns>
        public override string ToBlob()
        {
            return _blob ??
                  (_blob = new Buffer().WriteString(this.SshType).WriteBigNum(this.Exponent, this.Modulus).ToString());
        }

        public override bool SshDoVerify(byte[] sig, string data)
        {
            throw new NotImplementedException();
        }

        public override string SshDoSign(byte[] data)
        {
            throw new NotImplementedException();
        }
    }

    public class DsaKey : Key
    {
        public BigInteger P { get; set; }
        public BigInteger Q { get; set; }
        public BigInteger G { get; set; }
        public BigInteger X { get; set; }

        /// <summary>
        /// Returns "ssh-dss", which is the description of this key type used by the
        /// SSH2 protocol.
        /// </summary>
        public override string SshType
        {
            get { return "ssh-dss"; }
        }

        private string _blob;
        /// <summary>
        /// Converts the key to a blob, according to the SSH2 protocol.
        /// </summary>
        /// <returns></returns>
        public override string ToBlob()
        {
            return _blob ??
                  (_blob = new Buffer().WriteString(this.SshType).WriteBigNum(this.P, this.Q, this.G, this.X).ToString());
        }

        public override bool SshDoVerify(byte[] sig, string data)
        {
            throw new NotImplementedException();
        }

        public override string SshDoSign(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
