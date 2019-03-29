using System;
using System.Runtime.InteropServices;

namespace Abc.Zebus.Nng.Transport
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NngListener
    {
        public uint id;

        public string GetOptionString(string option)
        {
            byte* bytes;
            NngNative.nng_listener_getopt_string(this, option, &bytes);

            var value = Marshal.PtrToStringAnsi((IntPtr)bytes);
            NngNative.nng_strfree(bytes);

            return value;
        }

        public void Close()
        {
            NngNative.nng_listener_close(this);
        }
    }
}
