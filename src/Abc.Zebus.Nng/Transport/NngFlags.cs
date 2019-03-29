using System;

namespace Abc.Zebus.Nng.Transport
{
    [Flags]
    public enum NngFlags
    {
        None = 0,
        Alloc = 1,
        Nonblock = 2,
    }
}
