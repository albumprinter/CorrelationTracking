using System;
using log4net.Appender;
using log4net.Core;

namespace Correlation.IntegrationTests
{
    public class InterceptAppender : IAppender
    {
        public event EventHandler<LoggingEvent> OnAppend = delegate { };

        public InterceptAppender()
        {
            Name = DateTime.UtcNow.Ticks.ToString("D");
        }

        public string Name { get; set; }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            OnAppend.Invoke(this, loggingEvent);
        }

        public void Close()
        {
        }
    }
}