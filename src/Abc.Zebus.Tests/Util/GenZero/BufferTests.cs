using System;
using System.Linq;
using Abc.Zebus.Testing.Extensions;
using NUnit.Framework;
using Buffer = Abc.Zebus.Util.Buffer;

namespace Abc.Zebus.Tests.Util.GenZero
{
    [TestFixture]
    public unsafe class BufferTests
    {
        [Test]
        public void should_compare_buffer_equality_1()
        {
            var bufA = new Buffer(new byte[] { 42, 1, 2, 3, 4, 5 });
            var bufB = new Buffer(new byte[] { 42, 1, 2, 3, 4, 5 });

            CompareBuffersAndHashCodes(bufA, bufB).ShouldBeTrue();
        }

        [Test]
        public void should_compare_buffer_equality_2()
        {
            var bufA = new Buffer(new byte[] { 42, 1, 2, 3, 4, 5 });
            var bufB = new Buffer(new byte[] { 42, 1, 2, 30, 4, 5 });

            CompareBuffersAndHashCodes(bufA, bufB).ShouldBeFalse();
        }

        [Test]
        public void should_compare_buffer_equality_3()
        {
            var data = new byte[] { 42, 1, 2, 3, 4, 5 };
            var buf = new Buffer(data, 0, 4);

            CompareBuffersAndHashCodes(buf, new Buffer(data, 0, 4)).ShouldBeTrue();
            CompareBuffersAndHashCodes(buf, new Buffer(data, 0, 3)).ShouldBeFalse();
            CompareBuffersAndHashCodes(buf, new Buffer(data, 1, 4)).ShouldBeFalse();
            CompareBuffersAndHashCodes(buf, new Buffer()).ShouldBeFalse();
            CompareBuffersAndHashCodes(new Buffer(data, 0, 0), new Buffer()).ShouldBeFalse();
            CompareBuffersAndHashCodes(new Buffer(data, 3, 0), new Buffer()).ShouldBeFalse();
        }

        private static bool CompareBuffersAndHashCodes(Buffer a, Buffer b)
            => a.Equals(b) & a.GetHashCode() == b.GetHashCode();

        [Test]
        public void should_copy_buffer()
        {
            var data = new byte[] { 42, 1, 2, 3, 4, 5 };
            var buf = new Buffer(data, 0, 4);
            var bufCopy = buf.Copy();

            CompareBuffersAndHashCodes(buf, bufCopy).ShouldBeTrue();
            CompareBuffersAndHashCodes(bufCopy, new Buffer(new byte[] { 42, 1, 2, 3 })).ShouldBeTrue();

            data[1] = 10;
            
            CompareBuffersAndHashCodes(buf, bufCopy).ShouldBeFalse();
            CompareBuffersAndHashCodes(bufCopy, new Buffer(new byte[] { 42, 1, 2, 3 })).ShouldBeTrue();
        }

        [Test]
        public void should_copy_buffers_for_all_lengths([Range(0, 1024)] int length)
        {
            var buffer1 = new byte[1024];
            var buffer2 = new byte[1024];

            var random = new Random();
            random.NextBytes(buffer1);

            fixed (byte* b1 = buffer1)
            fixed (byte* b2 = buffer2)
            {
                Buffer.Copy(b2, b1, length);
            }

            buffer2.Take(length).SequenceEqual(buffer1.Take(length)).ShouldBeTrue();
            buffer2.Skip(length).Where(x => x != 0).ShouldBeEmpty();
        }

        [Test]
        public void should_compare_buffers_for_all_lengths([Range(0, 1024)] int length)
        {
            var buffer1 = new byte[length];
            var buffer2 = new byte[length];

            var random = new Random();
            random.NextBytes(buffer1);

            for (var i = 0; i < length; ++i)
                buffer2[i] = buffer1[i];

            unchecked
            {
                fixed (byte* b1 = buffer1)
                fixed (byte* b2 = buffer2)
                {
                    Buffer.Equals(b1, b2, length).ShouldBeTrue();

                    if (length == 0)
                        return;

                    for (var i = 0; i < length; ++i)
                    {
                        ++buffer2[i];
                        Buffer.Equals(b1, b2, length).ShouldBeFalse();
                        --buffer2[i];
                    }
                }
            }
        }
    }
}
