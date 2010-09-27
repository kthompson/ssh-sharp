using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SSHSharp
{
    internal class StringScanner
    {
        public int Position { get; private set; }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                this.Position = 0;
                _text = value;
            }
        }

        public int Skip(string regex)
        {
            return Skip(new Regex(regex));
        }

        public int Skip(Regex regex)
        {
            var match = regex.Match(this._text, this.Position);
            if (match == Match.Empty)
                return 0;

            this.Position += match.Length;
            return match.Length;
        }

        public string Scan(string regex)
        {
            return Scan(new Regex(regex));
        }

        public string Scan(Regex regex)
        {
            var match = regex.Match(this._text, this.Position);
            if (match == Match.Empty)
                return null;

            this.Position += match.Length;
            return match.ToString();
        }

        public bool IsMatch(string regex)
        {
            return IsMatch(new Regex(regex));
        }

        public bool IsMatch(Regex regex)
        {
            return regex.IsMatch(this._text, this.Position);
        }

        public string Rest
        {
            get { return _text.Substring(this.Position); }
        }
    }
}
