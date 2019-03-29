using System;

namespace Abc.Zebus.Nng.Transport
{
    internal class NngEndPoint
    {
        private readonly string _value;

        public NngEndPoint(string value)
        {
            _value = value;
        }

        public bool HasRandomPort => string.IsNullOrEmpty(_value) || _value.EndsWith(":0") || _value.EndsWith(":*");

        public string ValueForListen()
        {
            if (string.IsNullOrEmpty(_value))
                return "tcp4://:0";

            return _value.Replace(":*", ":0")
                         .Replace("//*:", "//:")
                         .Replace("tcp:", "tcp4:"); // To avoid IPV6 socket creating
        }

        public string ValueForConnect()
        {
            if (string.IsNullOrEmpty(_value))
                throw new InvalidOperationException("Unable to connect to random endpoint");

            return _value.Replace("//:", $"//{Environment.MachineName}:")
                         .Replace("tcp4:", "tcp:") // For ZMQ compatibility
                         .ToLower();
        }
    }
}
