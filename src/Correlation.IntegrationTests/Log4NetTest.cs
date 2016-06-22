using System;
using Albumprinter.CorrelationTracking;
using log4net;
using log4net.Appender;
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
            CorrelationTrackingConfiguration.Initialize();
        }

        protected Log4NetTest(ITestOutputHelper output)
        {
            Output = output;
            xUnitAppender = new ActionAppender(Output.WriteLine);
            hierarchy = (log4net.Repository.Hierarchy.Hierarchy) LogManager.GetRepository();
            hierarchy.Root.AddAppender(xUnitAppender);
        }

        public void Dispose()
        {
            hierarchy.Root.RemoveAppender(xUnitAppender);
        }
    }
}