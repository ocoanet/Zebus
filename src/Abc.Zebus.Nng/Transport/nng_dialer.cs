using System.Runtime.InteropServices;

namespace Abc.Zebus.Nng.Transport
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct nng_dialer
    {
        public uint id;
    }
}