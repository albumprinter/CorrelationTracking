using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace Albumprinter.CorrelationTracking.Tracing.IIS
{
    internal sealed class TrackingHttpModuleConfiguration
    {
        private static readonly RegexOptions DefaultRegexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline;
        private static readonly List<string> DefaultAllowedHeaders = new List<string>
        {
            @"Accept",
            @"Content-Type",
            @"X-CorrelationId",
            @"X-RequestId"
        };

        public TrackingHttpModuleConfiguration(string allowedUrls, IEnumerable<string> allowedHeaders, string deniedUrls)
        {
            // NOTE: set to zero
            allowedUrls = string.IsNullOrWhiteSpace(allowedUrls) ? null : allowedUrls;
            deniedUrls = string.IsNullOrWhiteSpace(deniedUrls) ? null : deniedUrls;

            // NOTE: extend with defaults
            allowedUrls = allowedUrls ?? @"/api/|(\.asmx|\.svc)(\?|$)";
            allowedHeaders = allowedHeaders ?? Enumerable.Empty<string>();
            deniedUrls = deniedUrls ?? @"^[^:]+$";

            // NOTE: apply configurations
            AllowedUrls = new Regex(allowedUrls, DefaultRegexOptions);
            AllowedHeaders = new HashSet<string>(allowedHeaders, StringComparer.OrdinalIgnoreCase);
            DeniedUrls = new Regex(deniedUrls, DefaultRegexOptions);

            DefaultAllowedHeaders.ForEach(allowedHeader => AllowedHeaders.Add(allowedHeader));
        }

        public Regex AllowedUrls { get; private set; }
        public HashSet<string> AllowedHeaders { get; private set; }
        public Regex DeniedUrls { get; private set; }

        public static TrackingHttpModuleConfiguration FromConfig(string moduleName)
        {
            var appSettings = WebConfigurationManager.AppSettings;
            var allowedUrls = appSettings.Get(moduleName + @":AllowedUrls");
            var allowedHeaders = (appSettings.Get(moduleName + @":AllowedHeaders") ?? string.Empty).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var deniedUrls = appSettings.Get(moduleName + @":DeniedUrls");
            return new TrackingHttpModuleConfiguration(allowedUrls, allowedHeaders, deniedUrls);
        }
    }
}