namespace SSHSharp
{
    public class RawString
    {
        public string Value { get; private set; }

        public RawString(string value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}