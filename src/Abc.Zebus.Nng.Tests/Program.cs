using System;
using System.Text;
using Abc.Zebus.Nng.Transport;

namespace Abc.Zebus.Nng.Tests
{
    public static class Program
    {
        public static unsafe void Main()
        {
            Console.WriteLine(NngNative.Version());

            NngSocket pull;
            NngNative.nng_pull0_open(&pull);

            NngSocket push;
            NngNative.nng_push0_open(&push);

            //Console.WriteLine("(1) Press enter to continue...");
            //Console.ReadLine();
            Console.WriteLine("(1)");

            NngListener listener;
            //NngNative.nng_listen(pull, "tcp://127.0.0.1:0", &listener, 0);
            NngNative.nng_listen(pull, "tcp://:0", &listener, 0);

            var endpointString = listener.GetOptionString(NngOptions.NNG_OPT_URL);
            Console.WriteLine($"[{endpointString}]");

            var endpoint = new NngEndPoint(endpointString);
            NngNative.nng_dial(push, endpoint.ValueForConnect(), null, 0);

            //NngNative.nng_listen(pull, "tcp://127.0.0.1:123", null, 0);
            //NngNative.nng_dial(push, "tcp://127.0.0.1:123", null, 0);

            Console.WriteLine("(2) Press enter to continue...");
            Console.ReadLine();

            var sendBytes = Encoding.ASCII.GetBytes("Hello!!");
            fixed (byte* b = sendBytes)
            {
                NngNative.nng_send(push, b, (IntPtr)sendBytes.Length, 0);
            }

            //var recvBytes = new byte[1024];
            //var recvSize = new IntPtr(recvBytes.Length);
            //fixed (byte* b = recvBytes)
            //{
            //    NngNative.nng_recv(pull, b, &recvSize, 0);
            //}

            // Console.WriteLine(Encoding.ASCII.GetString(recvBytes, 0, (int)recvSize));

            var recvBytes = new IntPtr();
            var recvSize = new IntPtr();

            NngNative.nng_recv(pull, &recvBytes, &recvSize, NngFlags.Alloc);

            Console.WriteLine((int)recvSize);
            Console.WriteLine(Encoding.ASCII.GetString((byte*)recvBytes.ToPointer(), (int)recvSize));

            NngNative.nng_free(recvBytes.ToPointer(), recvSize);

            Console.WriteLine("(3) Press enter to continue...");
            Console.ReadLine();

            NngNative.nng_close(pull);
            NngNative.nng_close(push);
        }
    }
}
