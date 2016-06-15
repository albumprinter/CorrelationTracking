using System;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;

namespace Correlation.IntegrationTests
{
    internal sealed class ActionAppender : IAppender
    {
        private readonly Action<string> output;

        public ActionAppender(Action<string> output)
        {
            this.output = output;
        }

        public void Close()
        {
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            var layout = new PatternLayout("[PI:%property{X-ProcessId}]%n[CI:%property{X-CorrelationId}]%n[RI:%property{X-RequestId}]%n%date %-5level %m%n%n");
            output.Invoke(layout.Format(loggingEvent));
        }

        public string Name { get; set; }
    }
}