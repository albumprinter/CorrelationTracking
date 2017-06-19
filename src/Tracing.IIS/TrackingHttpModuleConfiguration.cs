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

        public TrackingHttpModuleConfiguration(string allowedUrls, IEnumerable<string> allowedHeaders, string deniedUrls, int maxMessageSize)
        {
            // NOTE: set to zero
            allowedUrls = string.IsNullOrWhiteSpace(allowedUrls) ? null : allowedUrls;
            deniedUrls = string.IsNullOrWhiteSpace(deniedUrls) ? null : deniedUrls;

            // NOTE: extend with defaults
            allowedUrls = allowedUrls ?? @"/(api|v\d+)/|\.(asmx|svc)(\?|$)";
            allowedHeaders = allowedHeaders ?? Enumerable.Empty<string>();
            deniedUrls = deniedUrls ?? @"^[^:]+$";

            // NOTE: apply configurations
            AllowedUrls = new Regex(allowedUrls, DefaultRegexOptions);
            AllowedHeaders = new HashSet<string>(allowedHeaders, StringComparer.OrdinalIgnoreCase);
            DeniedUrls = new Regex(deniedUrls, DefaultRegexOptions);
            MaxMessageSize = maxMessageSize;

            DefaultAllowedHeaders.ForEach(allowedHeader => AllowedHeaders.Add(allowedHeader));
        }

        public Regex AllowedUrls { get; private set; }
        public HashSet<string> AllowedHeaders { get; private set; }
        public Regex DeniedUrls { get; private set; }

        /// <summary>
        /// 0 is no limit, positive value is amount of characters logged in the message (exception and other objects are not truncated)
        /// </summary>
        public int MaxMessageSize { get; }

        public static TrackingHttpModuleConfiguration FromConfig(string moduleName)
        {
            var appSettings = WebConfigurationManager.AppSettings;
            var allowedUrls = appSettings.Get(moduleName + @":AllowedUrls");
            var allowedHeaders = (appSettings.Get(moduleName + @":AllowedHeaders") ?? string.Empty).Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var deniedUrls = appSettings.Get(moduleName + @":DeniedUrls");
            var maxMessageSizeText = appSettings.Get(moduleName + @":MaxMessageSize");
            var maxMessageSize = maxMessageSizeText == null ? 0 : int.Parse(maxMessageSizeText);
            return new TrackingHttpModuleConfiguration(allowedUrls, allowedHeaders, deniedUrls, maxMessageSize);
        }
    }
}