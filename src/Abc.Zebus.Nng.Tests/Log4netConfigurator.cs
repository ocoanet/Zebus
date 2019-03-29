using System;
using System.IO;
using log4net.Config;
using log4net.Core;
using NUnit.Framework;

namespace Abc.Zebus.Nng.Tests
{
    [SetUpFixture]
    public class Log4netConfigurator
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var configurationFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            XmlConfigurator.Configure(LoggerManager.GetRepository(typeof(Log4netConfigurator).Assembly), new FileInfo(configurationFile));
        }
    }
}
