using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SSHSharp
{
    public static class Extensions
    {
        public static void Times(this int i, Action action)
        {
            var count = i;
            while (count-- > 0)
                action();
        }

        public static int IndexOf(this StringBuilder source, string pattern)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return IndexOf(source, pattern, 0, source.Length);
        }

        public static int IndexOf(this StringBuilder source, string pattern, int startIndex)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return IndexOf(source, pattern, startIndex, source.Length - startIndex);
        }

        public static int IndexOf(this StringBuilder source, string pattern, int startIndex, int count)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (pattern == null)
                throw new ArgumentNullException("pattern");

            if (startIndex > source.Length)
                throw new ArgumentOutOfRangeException("startIndex");

            if (source.Length == 0)
                return pattern.Length == 0 ? 0 : -1;

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            if ((count < 0) || (startIndex > (source.Length - count)))
                throw new ArgumentOutOfRangeException("count");

            var patternLength = pattern.Length;
            var patternLengthM1 = patternLength - 1;

            var cache = CalculateCache(pattern);

            // calculate md2
            var lastPattern = pattern[patternLength - 1];
            var md2 = patternLength;
            for (var i = 0; i < patternLength; i++)
                if (lastPattern == pattern[i])
                    md2 = patternLength - i;


            for (var i = patternLengthM1; i < source.Length; i++)
            {
                if (lastPattern == source[i])
                {
                    // last character matched, match the rest
                    for (var i2 = 0; i2 < patternLengthM1; i2++)
                    {
                        if (pattern[i2] == source[i - patternLength + 1])
                            continue;

                        // see if character under cursor is "impossible".
                        var altskip = IsCached(cache, source[i]) ? 1 : i2 + 1;

                        // skip the maximum of md2 and impossible calculation
                        i += Math.Max(md2, altskip);

                        goto scan_loop;
                    }

                    return i - patternLength + 1; // everything matched
                }

                if(!IsCached(cache, source[i]) )
                    i += patternLengthM1;

                scan_loop: ; //go again
            }
            return -1;
        }

        public static int IndexOf(this StringBuilder source, int pattern, int startIndex = 0)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (startIndex > source.Length)
                throw new ArgumentOutOfRangeException("startIndex");

            if (source.Length == 0)
                return -1;

            for (var i = startIndex; i < source.Length; i++)
                if (source[i] == pattern)
                    return i;

            return -1;
        }

        public static Match Match(this Regex regex, StringBuilder source, int startIndex)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return Match(regex, source, startIndex, source.Length - startIndex);
        }

        public static Match Match(this Regex regex, StringBuilder source, int startIndex, int length)
        {
             if (source == null)
                throw new ArgumentNullException("source");

            if (startIndex > source.Length)
                throw new ArgumentOutOfRangeException("startIndex");

            if (source.Length == 0)
                return null;

            if ((length < 0) || (startIndex > (source.Length - length)))
                throw new ArgumentOutOfRangeException("length");

            //TODO: this is very inefficient but I can't think of an easy way to do this so it will have to do for now.
            return regex.Match(source.ToString(), startIndex, length);
        }

        public static int AsInt32(this byte[] bs)
        {
            return (((bs[3] | (bs[2] << 8)) | (bs[1] << 0x10)) | (bs[0] << 0x18));
        }

        public static byte[] ToByteArray(this string s)
        {
            return s.ToCharArray().Select(n => (byte)n).ToArray();
        }


        private static bool IsCached(long cache, char c)
        {
            return (cache & (1L << (c & 63))) != 0L;
        }

        private static long CalculateCache(string pattern)
        {
            return pattern.Aggregate(0L, (current, c) => current | 1L << (c & 63));
        }
    }
}
