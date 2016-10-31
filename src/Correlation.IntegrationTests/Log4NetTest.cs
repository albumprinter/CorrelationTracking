using System;
using Albumprinter.CorrelationTracking;
using log4net;
using log4net.Appender;
using log4net.Config;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public abstract class Log4NetTest : IDisposable
    {
        private readonly log4net.Repository.Hierarchy.Hierarchy hierarchy;
        private readonly IAppender xUnitAppender;
        protected readonly ITestOutputHelper Output;

        static Log4NetTest()
        {
            XmlConfigurator.Configure();
            CorrelationTrackingConfiguration.Initialize();
        }

        protected Log4NetTest(ITestOutputHelper output)
        {
            Output = output;
            xUnitAppender = new ActionAppender(
                x => Output.WriteLine(x),
                "[PI:%property{X-ProcessId}]%n[CI:%property{X-CorrelationId}]%n[RI:%property{X-RequestId}]%n%date %-5level %m%n%n");
            hierarchy = (log4net.Repository.Hierarchy.Hierarchy) LogManager.GetRepository();
            hierarchy.Root.AddAppender(xUnitAppender);
        }

        public void Dispose()
        {
            hierarchy.Root.RemoveAppender(xUnitAppender);
        }
    }
}