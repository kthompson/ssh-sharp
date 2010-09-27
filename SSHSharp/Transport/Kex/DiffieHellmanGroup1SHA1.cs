﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Math;

namespace SSHSharp.Transport.Kex
{
    /// <summary>
    /// A key-exchange service implementing the "diffie-hellman-group1-sha1"
    /// key-exchange algorithm.
    /// </summary>
    public class DiffieHellmanGroup1SHA1
    {
        /// <summary>
        /// The value of 'P', as a string, in hexadecimal
        /// </summary>
        public static readonly string Ps = "FFFFFFFF" + "FFFFFFFF" + "C90FDAA2" + "2168C234" +
                                            "C4C6628B" + "80DC1CD1" + "29024E08" + "8A67CC74" +
                                            "020BBEA6" + "3B139B22" + "514A0879" + "8E3404DD" +
                                            "EF9519B3" + "CD3A431B" + "302B0A6D" + "F25F1437" +
                                            "4FE1356D" + "6D51C245" + "E485B576" + "625E7EC6" +
                                            "F44C42E9" + "A637ED6B" + "0BFF5CB6" + "F406B7ED" +
                                            "EE386BFB" + "5A899FA5" + "AE9F2411" + "7C4B1FE6" +
                                            "49286651" + "ECE65381" + "FFFFFFFF" + "FFFFFFFF";
        /// <summary>
        /// The radix in which P_s represents the value of P
        /// </summary>
        public static readonly int Pr = 16;

        /// <summary>
        /// The group constant
        /// </summary>
        public static readonly int Gc = 2;


        public BigInteger P { get; private set; }
        public BigInteger G { get; private set; }

        public DiffieHellmanGroup1SHA1(object algorithms, object connection, object data)
        {
            
        }
    }
}
