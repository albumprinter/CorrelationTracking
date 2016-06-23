using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace Albumprinter.CorrelationTracking.Tracing.IIS
{
    internal sealed class TrackingHttpModuleConfiguration
    {
        public Regex AllowedUrls { get; private set; }
        public HashSet<string> AllowedHeaders { get; private set; }

        public TrackingHttpModuleConfiguration(string allowedUrls, IEnumerable<string> allowedHeaders)
        {
            AllowedUrls = new Regex(
                string.IsNullOrWhiteSpace(allowedUrls) ? @"/api/|(\.asmx|\.svc)(\?|$)" : allowedUrls,
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            AllowedHeaders = new HashSet<string>(
                allowedHeaders ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase) { "Accept", "Content-Type", "X-CorrelationId", "X-RequestId" };
        }

        public static TrackingHttpModuleConfiguration FromConfig(string moduleName)
        {
            var appSettings = WebConfigurationManager.AppSettings;
            var allowedUrls = appSettings.Get(moduleName + @":AllowedUrls");
            var allowedHeaders = (appSettings.Get(moduleName + @":AllowedHeaders") ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new TrackingHttpModuleConfiguration(allowedUrls, allowedHeaders);
        }
    }
}