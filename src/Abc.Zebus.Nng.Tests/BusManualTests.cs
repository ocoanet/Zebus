using System.Threading;
using Abc.Zebus.Core;
using Abc.Zebus.Nng.Transport;
using Abc.Zebus.Persistence;
using Abc.Zebus.Transport;
using NUnit.Framework;

namespace Abc.Zebus.Nng.Tests
{
    [TestFixture]
    [Explicit]
    [Category("ManualOnly")]
    public class BusManualTests
    {
        // this must be a valid directory endpoint
        private const string _directoryEndPoint = "tcp://SrvcsPactSWD017:129";
        private const string _environment = "Dev";

        [Test]
        public void StartBusAndStopItLikeABoyScout()
        {
            using (var bus = CreateBusFactory().CreateAndStartBus())
            {
                Thread.Sleep(3_000);
            }
        }
    
        private static BusFactory CreateBusFactory()
        {
            return new BusFactory()
                   .ConfigureContainer(x =>
                   {
                       x.ForSingletonOf<IPersistentTransport>().Use<PersistentTransport>().Ctor<ITransport>().Is<NngTransport>();
                       x.For<INngTransportConfiguration>().Use(new NngTransportConfiguration());
                   })
                   .WithConfiguration(_directoryEndPoint, _environment)
                   .WithPeerId("Abc.Nng.*");
        }
    }
}
