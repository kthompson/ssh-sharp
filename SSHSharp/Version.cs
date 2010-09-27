using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp
{
    public class Version
    {
        /// <summary>
        /// The major component of this version of the Net::SSH library
        /// </summary>
        public static readonly int Major = 0;

        /// <summary>
        ///  The minor component of this version of the Net::SSH library
        /// </summary>
        public static readonly int Minor = 1;

        /// <summary>
        /// The build component of this version of the Net::SSH library
        /// </summary>
        public static readonly int Build = 0;

        /// <summary>
        /// The current version of the SSHSharp library as a Version instance
        /// </summary>
        public static readonly System.Version Current = new System.Version(Major, Minor, Build);

        /// <summary>
        /// The current version of the SSHSharp library as a String
        /// </summary>
        public static readonly string String = Current.ToString();
    }
}
