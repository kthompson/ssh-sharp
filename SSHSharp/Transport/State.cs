using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSHSharp.Transport
{
    public class State
    {
        public State(PacketStream stream, StateMode mode)
        {
            throw new NotImplementedException();
        }
    }

    public enum StateMode
    {
        Client,
        Server,
    }
}
