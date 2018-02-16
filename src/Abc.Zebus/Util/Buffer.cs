using System;
using System.Runtime.CompilerServices;

namespace Abc.Zebus.Util
{
    internal readonly unsafe struct Buffer : IEquatable<Buffer>
    {
        public readonly byte[] Data;
        public readonly int Offset;
        public readonly int Length;

        public Buffer(byte[] data)
            : this(data, 0, data.Length)
        {
        }

        public Buffer(byte[] data, int offset, int length)
        {
            Data = data;
            Offset = offset;
            Length = length;
        }

        public Buffer Copy()
        {
            var newBuffer = new byte[Length];

            fixed (byte* buf = Data, newBuf = newBuffer)
            {
                Copy(newBuf, buf + Offset, Length);
            }

            return new Buffer(newBuffer, 0, newBuffer.Length);
        }

        public bool Equals(Buffer other)
        {
            if (Length != other.Length)
                return false;

            if (Length == 0)
                return true;

            fixed (byte* buf = Data, otherBuf = other.Data)
            {
                return Equals(buf + Offset, otherBuf + other.Offset, Length);
            }
        }

        public override bool Equals(object obj)
            => obj is Buffer other && Equals(other);

        public override int GetHashCode()
        {
            if (Data == null)
                return 0;

            unchecked
            {
                // FNV-1 hash
                var hash = 2166136261u;

                fixed (byte* buf = Data)
                {
                    var data = buf + Offset;

                    for (var i = 0; i < Length; ++i)
                    {
                        hash ^= data[i];
                        hash *= 16777619u;
                    }
                }

                return (int)hash;
            }
        }

        public static bool Equals(byte* a, byte* b, int length)
        {
            SMALLTABLE:
            switch (length)
            {
                case 16:
                    if (*(long*)a != *(long*)b)
                        goto FALSE;
                    if (*(long*)(a + 8) != *(long*)(b + 8))
                        goto FALSE;
                    goto TRUE;
                case 15:
                    if (*(short*)(a + 12) != *(short*)(b + 12))
                        goto FALSE;
                    if (*(a + 14) != *(b + 14))
                        goto FALSE;
                    goto case 12;
                case 14:
                    if (*(short*)(a + 12) != *(short*)(b + 12))
                        goto FALSE;
                    goto case 12;
                case 13:
                    if (*(a + 12) != *(b + 12))
                        goto FALSE;
                    goto case 12;
                case 12:
                    if (*(long*)a != *(long*)b)
                        goto FALSE;
                    if (*(int*)(a + 8) != *(int*)(b + 8))
                        goto FALSE;
                    goto TRUE;
                case 11:
                    if (*(short*)(a + 8) != *(short*)(b + 8))
                        goto FALSE;
                    if (*(a + 10) != *(b + 10))
                        goto FALSE;
                    goto case 8;
                case 10:
                    if (*(short*)(a + 8) != *(short*)(b + 8))
                        goto FALSE;
                    goto case 8;
                case 9:
                    if (*(a + 8) != *(b + 8))
                        goto FALSE;
                    goto case 8;
                case 8:
                    if (*(long*)a != *(long*)b)
                        goto FALSE;
                    goto TRUE;
                case 7:
                    if (*(short*)(a + 4) != *(short*)(b + 4))
                        goto FALSE;
                    if (*(a + 6) != *(b + 6))
                        goto FALSE;
                    goto case 4;
                case 6:
                    if (*(short*)(a + 4) != *(short*)(b + 4))
                        goto FALSE;
                    goto case 4;
                case 5:
                    if (*(a + 4) != *(b + 4))
                        goto FALSE;
                    goto case 4;
                case 4:
                    if (*(int*)a != *(int*)b)
                        goto FALSE;
                    goto TRUE;
                case 3:
                    if (*(a + 2) != *(b + 2))
                        goto FALSE;
                    goto case 2;
                case 2:
                    if (*(short*)a != *(short*)b)
                        goto FALSE;
                    goto TRUE;
                case 1:
                    if (*a != *b)
                        goto FALSE;
                    goto TRUE;
                case 0:
                    goto TRUE;

                default:
                    if (length > 8)
                    {
                        if (*(long*)a != *(long*)b)
                            goto FALSE;

                        var alignA = (int)a & 7;
                        var alignB = (int)b & 7;

                        var align = alignA > alignB ? alignB : alignA;
                        align = 8 - align;

                        a += align;
                        b += align;
                        length -= align;
                    }

                    while (length > 16)
                    {
                        if (*(long*)a != *(long*)b)
                            goto FALSE;
                        if (*(long*)(a + 8) != *(long*)(b + 8))
                            goto FALSE;

                        a += 16;
                        b += 16;
                        length -= 16;
                    }

                    if (length < 0)
                        ThrowArgOutOfRange();

                    goto SMALLTABLE;
            }

            FALSE:
            return false;

            TRUE:
            return true;
        }

        public static void Copy(byte* dest, byte* src, int n)
        {
            SMALLTABLE:
            switch (n)
            {
                case 16:
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
                    goto END;
                case 15:
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    *(dest + 14) = *(src + 14);
                    goto case 12;
                case 14:
                    *(short*)(dest + 12) = *(short*)(src + 12);
                    goto case 12;
                case 13:
                    *(dest + 12) = *(src + 12);
                    goto case 12;
                case 12:
                    *(long*)dest = *(long*)src;
                    *(int*)(dest + 8) = *(int*)(src + 8);
                    goto END;
                case 11:
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    *(dest + 10) = *(src + 10);
                    goto case 8;
                case 10:
                    *(short*)(dest + 8) = *(short*)(src + 8);
                    goto case 8;
                case 9:
                    *(dest + 8) = *(src + 8);
                    goto case 8;
                case 8:
                    *(long*)dest = *(long*)src;
                    goto END;
                case 7:
                    *(short*)(dest + 4) = *(short*)(src + 4);
                    *(dest + 6) = *(src + 6);
                    goto case 4;
                case 6:
                    *(short*)(dest + 4) = *(short*)(src + 4);
                    goto case 4;
                case 5:
                    *(dest + 4) = *(src + 4);
                    goto case 4;
                case 4:
                    *(int*)dest = *(int*)src;
                    goto END;
                case 3:
                    *(dest + 2) = *(src + 2);
                    goto case 2;
                case 2:
                    *(short*)dest = *(short*)src;
                    goto END;
                case 1:
                    *dest = *src;
                    goto END;
                case 0:
                    goto END;
            }

            var count = n / 32;
            n -= count * 32;

            while (count > 0)
            {
                ((long*)dest)[0] = ((long*)src)[0];
                ((long*)dest)[1] = ((long*)src)[1];
                ((long*)dest)[2] = ((long*)src)[2];
                ((long*)dest)[3] = ((long*)src)[3];

                dest += 32;
                src += 32;
                --count;
            }

            if (n > 16)
            {
                ((long*)dest)[0] = ((long*)src)[0];
                ((long*)dest)[1] = ((long*)src)[1];

                src += 16;
                dest += 16;
                n -= 16;
            }

            goto SMALLTABLE;

            END: ;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowArgOutOfRange()
            => throw new ArgumentOutOfRangeException();
    }
}
