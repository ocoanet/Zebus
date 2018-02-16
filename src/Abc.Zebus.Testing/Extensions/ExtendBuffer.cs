using Abc.Zebus.Util;
using NUnit.Framework;

namespace Abc.Zebus.Testing.Extensions
{
    internal static class ExtendBuffer
    {
        public static void ShouldEqual(this Buffer actual, ref Buffer expected, string message = null)
        {
            Assert.AreEqual(expected, actual, message);
        }
    }
}
