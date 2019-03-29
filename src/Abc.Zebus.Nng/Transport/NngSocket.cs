using System;
using System.Runtime.InteropServices;

namespace Abc.Zebus.Nng.Transport
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NngSocket
    {
        public uint id;

        public static NngSocket Pull()
        {
            NngSocket socket;
            NngNative.nng_pull0_open(&socket);

            return socket;
        }

        public string GetOptionString(string option)
        {
            byte* bytes;
            NngNative.nng_getopt_string(this, option, &bytes);

            var value = Marshal.PtrToStringAnsi((IntPtr)bytes);
            NngNative.nng_strfree(bytes);

            return value;
        }

        public void SetOptionInt32(string option, int value)
        {
            NngNative.nng_setopt_int(this, option, value);
        }

        public void SetOptionTimeSpan(string option, TimeSpan value)
        {
            NngNative.nng_setopt_ms(this, option, (int)value.TotalMilliseconds);
        }

        public void Listen(string endpoint, NngListener* listener)
        {
            NngNative.nng_listen(this, endpoint, listener, 0);
        }

        public (NngError error, int size) Receive(ref byte[] buffer)
        {
            var recvBytes = new IntPtr();
            var recvSize = new IntPtr();

            var error = (NngError)NngNative.nng_recv(this, &recvBytes, &recvSize, NngFlags.Alloc);

            if (error != NngError.None)
                return (error, default);

            var size = (int)recvSize;

            if (buffer == null || buffer.Length < size)
                buffer = new byte[size];

            fixed (byte* pBuf = &buffer[0])
            {
                Buffer.MemoryCopy(recvBytes.ToPointer(), pBuf, buffer.Length, size);
            }

            NngNative.nng_free(recvBytes.ToPointer(), recvSize);

            return (NngError.None, size);
        }

        public (NngError error, int size) ReceiveMsg(ref byte[] buffer)
        {
            var recvMsg = new IntPtr();

            var error = (NngError)NngNative.nng_recvmsg(this, &recvMsg, NngFlags.None);

            if (error != NngError.None)
                return (error, default);

            var size = (int)NngNative.nng_msg_len(recvMsg);

            if (buffer == null || buffer.Length < size)
                buffer = new byte[size];

            fixed (byte* pBuf = &buffer[0])
            {
                Buffer.MemoryCopy(NngNative.nng_msg_body(recvMsg), pBuf, buffer.Length, size);
            }

            NngNative.nng_msg_free(recvMsg);

            return (NngError.None, size);
        }

        public NngError Send(byte[] buffer, int size)
        {
            fixed (byte* b = buffer)
            {
                return (NngError)NngNative.nng_send(this, b, (IntPtr)size, 0);
            }
        }
    }
}
