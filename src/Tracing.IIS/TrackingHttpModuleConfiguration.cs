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
        public List<string> AllowedHeaders { get; private set; }

        public TrackingHttpModuleConfiguration(string allowedUrls, IEnumerable<string> allowedHeaders)
        {
            AllowedUrls = new Regex(allowedUrls ?? @"/api/|(\.asmx|\.svc)(\?|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            AllowedHeaders = new List<string> { "Accept", "Content-Type", "X-CorrelationId", "X-RequestId" };
            AllowedHeaders.AddRange(allowedHeaders ?? Enumerable.Empty<string>());
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