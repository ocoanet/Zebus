﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;

namespace Abc.Zebus.Nng.Transport
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    unsafe partial class NngNative
    {
        public static int nng_push0_open(NngSocket* s)
            => _impl.nng_push0_open(s);

        public static int nng_pull0_open(NngSocket* s)
            => _impl.nng_pull0_open(s);

        public static int nng_close(NngSocket s)
            => _impl.nng_close(s);

        public static int nng_listen(NngSocket s, string url, NngListener* lp, int flags)
            => _impl.nng_listen(s, url, lp, flags);

        public static int nng_dial(NngSocket s, string url, nng_dialer* dp, int flags)
            => _impl.nng_dial(s, url, dp, flags);

        public static int nng_setopt_int(NngSocket s, string opt, int value)
            => _impl.nng_setopt_int(s, opt, value);

        public static int nng_setopt_ms(NngSocket s, string opt, int ms)
            => _impl.nng_setopt_ms(s, opt, ms);

        public static int nng_getopt_string(NngSocket s, string opt, byte** str)
            => _impl.nng_getopt_string(s, opt, str);

        public static int nng_listener_getopt_string(NngListener l, string opt, byte** str)
            => _impl.nng_listener_getopt_string(l, opt, str);

        public static int nng_listener_close(NngListener l)
            => _impl.nng_listener_close(l);

        public static int nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags)
            => _impl.nng_send(s, buf, len, flags);

        public static int nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags)
            => _impl.nng_sendmsg(s, msg, flags);

        public static int nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags)
            => _impl.nng_recv(s, data, size, flags);

        public static int nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags)
            => _impl.nng_recvmsg(s, msgp, flags);

        public static int nng_msg_alloc(IntPtr* msgp, IntPtr size)
            => _impl.nng_msg_alloc(msgp, size);

        public static void* nng_msg_body(IntPtr msg)
            => _impl.nng_msg_body(msg);

        public static void nng_msg_free(IntPtr msg)
            => _impl.nng_msg_free(msg);

        public static IntPtr nng_msg_len(IntPtr msg)
            => _impl.nng_msg_len(msg);

        public static void nng_msg_clear(IntPtr msg)
            => _impl.nng_msg_clear(msg);

        public static int nng_msg_append(IntPtr msg, void* val, IntPtr size)
            => _impl.nng_msg_append(msg, val, size);

        public static void nng_strfree(byte* str)
            => _impl.nng_strfree(str);

        public static void nng_free(void* ptr, IntPtr size)
            => _impl.nng_free(ptr, size);

        public static IntPtr nng_version()
            => _impl.nng_version();


        private abstract class LibImpl
        {
            public abstract int nng_push0_open(NngSocket* s);
            public abstract int nng_pull0_open(NngSocket* s);
            public abstract int nng_close(NngSocket s);
            public abstract int nng_listen(NngSocket s, string url, NngListener* lp, int flags);
            public abstract int nng_dial(NngSocket s, string url, nng_dialer* dp, int flags);
            public abstract int nng_setopt_int(NngSocket s, string opt, int value);
            public abstract int nng_setopt_ms(NngSocket s, string opt, int ms);
            public abstract int nng_getopt_string(NngSocket s, string opt, byte** str);
            public abstract int nng_listener_getopt_string(NngListener l, string opt, byte** str);
            public abstract int nng_listener_close(NngListener l);
            public abstract int nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags);
            public abstract int nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags);
            public abstract int nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags);
            public abstract int nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags);
            public abstract int nng_msg_alloc(IntPtr* msgp, IntPtr size);
            public abstract void* nng_msg_body(IntPtr msg);
            public abstract void nng_msg_free(IntPtr msg);
            public abstract IntPtr nng_msg_len(IntPtr msg);
            public abstract void nng_msg_clear(IntPtr msg);
            public abstract int nng_msg_append(IntPtr msg, void* val, IntPtr size);
            public abstract void nng_strfree(byte* str);
            public abstract void nng_free(void* ptr, IntPtr size);
            public abstract IntPtr nng_version();
        }

        [SuppressUnmanagedCodeSecurity]
        private class WinImpl : LibImpl
        {
            public override int nng_push0_open(NngSocket* s)
                => extern_nng_push0_open(s);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_push0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_push0_open(NngSocket* s);

            public override int nng_pull0_open(NngSocket* s)
                => extern_nng_pull0_open(s);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_pull0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_pull0_open(NngSocket* s);

            public override int nng_close(NngSocket s)
                => extern_nng_close(s);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_close(NngSocket s);

            public override int nng_listen(NngSocket s, string url, NngListener* lp, int flags)
                => extern_nng_listen(s, url, lp, flags);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_listen", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listen(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, NngListener* lp, int flags);

            public override int nng_dial(NngSocket s, string url, nng_dialer* dp, int flags)
                => extern_nng_dial(s, url, dp, flags);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_dial", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_dial(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, nng_dialer* dp, int flags);

            public override int nng_setopt_int(NngSocket s, string opt, int value)
                => extern_nng_setopt_int(s, opt, value);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_setopt_int", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_int(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int value);

            public override int nng_setopt_ms(NngSocket s, string opt, int ms)
                => extern_nng_setopt_ms(s, opt, ms);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_setopt_ms", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_ms(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int ms);

            public override int nng_getopt_string(NngSocket s, string opt, byte** str)
                => extern_nng_getopt_string(s, opt, str);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_getopt_string(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_getopt_string(NngListener l, string opt, byte** str)
                => extern_nng_listener_getopt_string(l, opt, str);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_listener_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_getopt_string(NngListener l, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_close(NngListener l)
                => extern_nng_listener_close(l);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_listener_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_close(NngListener l);

            public override int nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags)
                => extern_nng_send(s, buf, len, flags);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_send", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags);

            public override int nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags)
                => extern_nng_sendmsg(s, msg, flags);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_sendmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags);

            public override int nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags)
                => extern_nng_recv(s, data, size, flags);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_recv", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags);

            public override int nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags)
                => extern_nng_recvmsg(s, msgp, flags);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_recvmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags);

            public override int nng_msg_alloc(IntPtr* msgp, IntPtr size)
                => extern_nng_msg_alloc(msgp, size);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_msg_alloc", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_alloc(IntPtr* msgp, IntPtr size);

            public override void* nng_msg_body(IntPtr msg)
                => extern_nng_msg_body(msg);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_msg_body", CallingConvention = CallingConvention.Cdecl)]
            private static extern void* extern_nng_msg_body(IntPtr msg);

            public override void nng_msg_free(IntPtr msg)
                => extern_nng_msg_free(msg);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_msg_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_free(IntPtr msg);

            public override IntPtr nng_msg_len(IntPtr msg)
                => extern_nng_msg_len(msg);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_msg_len", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_msg_len(IntPtr msg);

            public override void nng_msg_clear(IntPtr msg)
                => extern_nng_msg_clear(msg);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_msg_clear", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_clear(IntPtr msg);

            public override int nng_msg_append(IntPtr msg, void* val, IntPtr size)
                => extern_nng_msg_append(msg, val, size);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_msg_append", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_append(IntPtr msg, void* val, IntPtr size);

            public override void nng_strfree(byte* str)
                => extern_nng_strfree(str);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_strfree", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_strfree(byte* str);

            public override void nng_free(void* ptr, IntPtr size)
                => extern_nng_free(ptr, size);

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_free(void* ptr, IntPtr size);

            public override IntPtr nng_version()
                => extern_nng_version();

            [DllImport("Abc.Zebus.libnng.dll", EntryPoint = "nng_version", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_version();

        }

        [SuppressUnmanagedCodeSecurity]
        private class Win32Impl : LibImpl
        {
            public override int nng_push0_open(NngSocket* s)
                => extern_nng_push0_open(s);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_push0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_push0_open(NngSocket* s);

            public override int nng_pull0_open(NngSocket* s)
                => extern_nng_pull0_open(s);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_pull0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_pull0_open(NngSocket* s);

            public override int nng_close(NngSocket s)
                => extern_nng_close(s);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_close(NngSocket s);

            public override int nng_listen(NngSocket s, string url, NngListener* lp, int flags)
                => extern_nng_listen(s, url, lp, flags);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_listen", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listen(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, NngListener* lp, int flags);

            public override int nng_dial(NngSocket s, string url, nng_dialer* dp, int flags)
                => extern_nng_dial(s, url, dp, flags);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_dial", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_dial(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, nng_dialer* dp, int flags);

            public override int nng_setopt_int(NngSocket s, string opt, int value)
                => extern_nng_setopt_int(s, opt, value);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_setopt_int", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_int(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int value);

            public override int nng_setopt_ms(NngSocket s, string opt, int ms)
                => extern_nng_setopt_ms(s, opt, ms);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_setopt_ms", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_ms(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int ms);

            public override int nng_getopt_string(NngSocket s, string opt, byte** str)
                => extern_nng_getopt_string(s, opt, str);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_getopt_string(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_getopt_string(NngListener l, string opt, byte** str)
                => extern_nng_listener_getopt_string(l, opt, str);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_listener_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_getopt_string(NngListener l, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_close(NngListener l)
                => extern_nng_listener_close(l);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_listener_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_close(NngListener l);

            public override int nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags)
                => extern_nng_send(s, buf, len, flags);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_send", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags);

            public override int nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags)
                => extern_nng_sendmsg(s, msg, flags);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_sendmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags);

            public override int nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags)
                => extern_nng_recv(s, data, size, flags);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_recv", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags);

            public override int nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags)
                => extern_nng_recvmsg(s, msgp, flags);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_recvmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags);

            public override int nng_msg_alloc(IntPtr* msgp, IntPtr size)
                => extern_nng_msg_alloc(msgp, size);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_msg_alloc", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_alloc(IntPtr* msgp, IntPtr size);

            public override void* nng_msg_body(IntPtr msg)
                => extern_nng_msg_body(msg);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_msg_body", CallingConvention = CallingConvention.Cdecl)]
            private static extern void* extern_nng_msg_body(IntPtr msg);

            public override void nng_msg_free(IntPtr msg)
                => extern_nng_msg_free(msg);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_msg_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_free(IntPtr msg);

            public override IntPtr nng_msg_len(IntPtr msg)
                => extern_nng_msg_len(msg);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_msg_len", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_msg_len(IntPtr msg);

            public override void nng_msg_clear(IntPtr msg)
                => extern_nng_msg_clear(msg);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_msg_clear", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_clear(IntPtr msg);

            public override int nng_msg_append(IntPtr msg, void* val, IntPtr size)
                => extern_nng_msg_append(msg, val, size);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_msg_append", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_append(IntPtr msg, void* val, IntPtr size);

            public override void nng_strfree(byte* str)
                => extern_nng_strfree(str);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_strfree", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_strfree(byte* str);

            public override void nng_free(void* ptr, IntPtr size)
                => extern_nng_free(ptr, size);

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_free(void* ptr, IntPtr size);

            public override IntPtr nng_version()
                => extern_nng_version();

            [DllImport("Abc.Zebus.libnng.x86.dll", EntryPoint = "nng_version", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_version();

        }

        [SuppressUnmanagedCodeSecurity]
        private class Win64Impl : LibImpl
        {
            public override int nng_push0_open(NngSocket* s)
                => extern_nng_push0_open(s);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_push0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_push0_open(NngSocket* s);

            public override int nng_pull0_open(NngSocket* s)
                => extern_nng_pull0_open(s);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_pull0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_pull0_open(NngSocket* s);

            public override int nng_close(NngSocket s)
                => extern_nng_close(s);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_close(NngSocket s);

            public override int nng_listen(NngSocket s, string url, NngListener* lp, int flags)
                => extern_nng_listen(s, url, lp, flags);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_listen", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listen(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, NngListener* lp, int flags);

            public override int nng_dial(NngSocket s, string url, nng_dialer* dp, int flags)
                => extern_nng_dial(s, url, dp, flags);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_dial", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_dial(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, nng_dialer* dp, int flags);

            public override int nng_setopt_int(NngSocket s, string opt, int value)
                => extern_nng_setopt_int(s, opt, value);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_setopt_int", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_int(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int value);

            public override int nng_setopt_ms(NngSocket s, string opt, int ms)
                => extern_nng_setopt_ms(s, opt, ms);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_setopt_ms", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_ms(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int ms);

            public override int nng_getopt_string(NngSocket s, string opt, byte** str)
                => extern_nng_getopt_string(s, opt, str);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_getopt_string(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_getopt_string(NngListener l, string opt, byte** str)
                => extern_nng_listener_getopt_string(l, opt, str);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_listener_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_getopt_string(NngListener l, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_close(NngListener l)
                => extern_nng_listener_close(l);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_listener_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_close(NngListener l);

            public override int nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags)
                => extern_nng_send(s, buf, len, flags);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_send", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags);

            public override int nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags)
                => extern_nng_sendmsg(s, msg, flags);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_sendmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags);

            public override int nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags)
                => extern_nng_recv(s, data, size, flags);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_recv", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags);

            public override int nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags)
                => extern_nng_recvmsg(s, msgp, flags);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_recvmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags);

            public override int nng_msg_alloc(IntPtr* msgp, IntPtr size)
                => extern_nng_msg_alloc(msgp, size);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_msg_alloc", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_alloc(IntPtr* msgp, IntPtr size);

            public override void* nng_msg_body(IntPtr msg)
                => extern_nng_msg_body(msg);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_msg_body", CallingConvention = CallingConvention.Cdecl)]
            private static extern void* extern_nng_msg_body(IntPtr msg);

            public override void nng_msg_free(IntPtr msg)
                => extern_nng_msg_free(msg);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_msg_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_free(IntPtr msg);

            public override IntPtr nng_msg_len(IntPtr msg)
                => extern_nng_msg_len(msg);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_msg_len", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_msg_len(IntPtr msg);

            public override void nng_msg_clear(IntPtr msg)
                => extern_nng_msg_clear(msg);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_msg_clear", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_clear(IntPtr msg);

            public override int nng_msg_append(IntPtr msg, void* val, IntPtr size)
                => extern_nng_msg_append(msg, val, size);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_msg_append", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_append(IntPtr msg, void* val, IntPtr size);

            public override void nng_strfree(byte* str)
                => extern_nng_strfree(str);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_strfree", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_strfree(byte* str);

            public override void nng_free(void* ptr, IntPtr size)
                => extern_nng_free(ptr, size);

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_free(void* ptr, IntPtr size);

            public override IntPtr nng_version()
                => extern_nng_version();

            [DllImport("Abc.Zebus.libnng.x64.dll", EntryPoint = "nng_version", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_version();

        }

        [SuppressUnmanagedCodeSecurity]
        private class LinuxImpl : LibImpl
        {
            public override int nng_push0_open(NngSocket* s)
                => extern_nng_push0_open(s);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_push0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_push0_open(NngSocket* s);

            public override int nng_pull0_open(NngSocket* s)
                => extern_nng_pull0_open(s);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_pull0_open", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_pull0_open(NngSocket* s);

            public override int nng_close(NngSocket s)
                => extern_nng_close(s);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_close(NngSocket s);

            public override int nng_listen(NngSocket s, string url, NngListener* lp, int flags)
                => extern_nng_listen(s, url, lp, flags);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_listen", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listen(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, NngListener* lp, int flags);

            public override int nng_dial(NngSocket s, string url, nng_dialer* dp, int flags)
                => extern_nng_dial(s, url, dp, flags);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_dial", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_dial(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string url, nng_dialer* dp, int flags);

            public override int nng_setopt_int(NngSocket s, string opt, int value)
                => extern_nng_setopt_int(s, opt, value);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_setopt_int", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_int(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int value);

            public override int nng_setopt_ms(NngSocket s, string opt, int ms)
                => extern_nng_setopt_ms(s, opt, ms);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_setopt_ms", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_setopt_ms(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, int ms);

            public override int nng_getopt_string(NngSocket s, string opt, byte** str)
                => extern_nng_getopt_string(s, opt, str);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_getopt_string(NngSocket s, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_getopt_string(NngListener l, string opt, byte** str)
                => extern_nng_listener_getopt_string(l, opt, str);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_listener_getopt_string", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_getopt_string(NngListener l, [In, MarshalAs(UnmanagedType.LPStr)] string opt, byte** str);

            public override int nng_listener_close(NngListener l)
                => extern_nng_listener_close(l);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_listener_close", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_listener_close(NngListener l);

            public override int nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags)
                => extern_nng_send(s, buf, len, flags);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_send", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_send(NngSocket s, void* buf, IntPtr len, NngFlags flags);

            public override int nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags)
                => extern_nng_sendmsg(s, msg, flags);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_sendmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_sendmsg(NngSocket s, IntPtr msg, NngFlags flags);

            public override int nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags)
                => extern_nng_recv(s, data, size, flags);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_recv", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recv(NngSocket s, void* data, IntPtr* size, NngFlags flags);

            public override int nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags)
                => extern_nng_recvmsg(s, msgp, flags);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_recvmsg", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_recvmsg(NngSocket s, IntPtr* msgp, NngFlags flags);

            public override int nng_msg_alloc(IntPtr* msgp, IntPtr size)
                => extern_nng_msg_alloc(msgp, size);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_msg_alloc", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_alloc(IntPtr* msgp, IntPtr size);

            public override void* nng_msg_body(IntPtr msg)
                => extern_nng_msg_body(msg);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_msg_body", CallingConvention = CallingConvention.Cdecl)]
            private static extern void* extern_nng_msg_body(IntPtr msg);

            public override void nng_msg_free(IntPtr msg)
                => extern_nng_msg_free(msg);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_msg_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_free(IntPtr msg);

            public override IntPtr nng_msg_len(IntPtr msg)
                => extern_nng_msg_len(msg);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_msg_len", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_msg_len(IntPtr msg);

            public override void nng_msg_clear(IntPtr msg)
                => extern_nng_msg_clear(msg);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_msg_clear", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_msg_clear(IntPtr msg);

            public override int nng_msg_append(IntPtr msg, void* val, IntPtr size)
                => extern_nng_msg_append(msg, val, size);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_msg_append", CallingConvention = CallingConvention.Cdecl)]
            private static extern int extern_nng_msg_append(IntPtr msg, void* val, IntPtr size);

            public override void nng_strfree(byte* str)
                => extern_nng_strfree(str);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_strfree", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_strfree(byte* str);

            public override void nng_free(void* ptr, IntPtr size)
                => extern_nng_free(ptr, size);

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_free", CallingConvention = CallingConvention.Cdecl)]
            private static extern void extern_nng_free(void* ptr, IntPtr size);

            public override IntPtr nng_version()
                => extern_nng_version();

            [DllImport("Abc.Zebus.libnng.so", EntryPoint = "nng_version", CallingConvention = CallingConvention.Cdecl)]
            private static extern IntPtr extern_nng_version();

        }

    }
}