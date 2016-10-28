using System;
using log4net.Layout;

namespace Correlation.IntegrationTests
{
    public sealed class ActionAppender : InterceptAppender
    {

        public ActionAppender(Action<string> output, string pattern)
        {
            Layout = new PatternLayout(pattern);
            OnAppend += (sender, loggingEvent) =>
            {
                output.Invoke(Layout.Format(loggingEvent));
            };
        }

        public PatternLayout Layout { get; set; }
    }
}