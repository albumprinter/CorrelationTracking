using System;
using System.Diagnostics;
using System.IO;
using log4net.Core;
using log4net.Layout.Pattern;

namespace Albumprinter.CorrelationTracking.Tracing.Log4net.Pattern
{
    /// <summary>
    ///     <see cref="PatternLayoutConverter" /> for injecting Trace.CorrelationManger.ActivityId into log messages.
    ///     <example>
    ///         <layout type="log4net.Layout.PatternLayout">
    ///             <param name="ActivityIdPattern" value="[AI:%AI] %-5p %d{yyyy-MM-dd hh:mm:ss} :: %m%n" />
    ///             <converter>
    ///                 <name value="AI" />
    ///                 <type value="Tracing.Log4net.Pattern.CorrelationPatternConverter" />
    ///             </converter>
    ///         </layout>
    ///     </example>
    /// </summary>
    public class ActivityIdPatternConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            var activityId = Trace.CorrelationManager.ActivityId;
            if (activityId != Guid.Empty)
            {
                writer.Write(activityId.ToString());
            }
        }
    }
}