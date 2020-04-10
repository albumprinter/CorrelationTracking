using System;
using System.Diagnostics;
using System.Linq;
using Albelli.Correlation.Http.Server.Middleware;
using Albelli.CorrelationTracking.Correlation.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Albelli.Correlation.Http.Server
{
    [PublicAPI]
    public static class UseExtensions
    {
        /// <summary>
        /// Adds a replacement for ILoggerT that can decorate log messages
        /// with values from Activity.Current.Baggage and Activity.Current.Tags.
        ///
        /// It will log:
        /// 1) All the tag values
        /// 2) Values from the baggage that start with "X-"
        /// </summary>
        /// <param name="services">The service collection from the IoC container</param>
        public static void AddDefaultActivityBagTagLoggerFactory([NotNull] this IServiceCollection services)
        {
            services.AddActivityBagTagLogger(s => s.StartsWith("X-", StringComparison.OrdinalIgnoreCase), _ => true);
        }

        /// <summary>
        /// Adds a replacement for ILoggerT that can decorate log messages
        /// with values from Activity.Current.Baggage and Activity.Current.Tags.
        ///
        /// At least one predicate must be specified.
        /// </summary>
        /// <param name="services">The service collection from the IoC container</param>
        /// <param name="shouldLogBag">
        /// A predicate which checks whether this baggage should be logged or not.
        /// `null` means it's not going to be logged.
        /// </param>
        /// <param name="shouldLogTag">
        /// A predicate which checks whether this tag should be logged or not.
        /// `null` means it's not going to be logged.
        /// </param>
        public static void AddActivityBagTagLogger([NotNull] this IServiceCollection services, Predicate<string> shouldLogBag = null, Predicate<string> shouldLogTag = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddSingleton(new ActivityBagTagDecoratingLoggerSettings(shouldLogBag, shouldLogTag));
            var existingLifetime = services
                .Where(x => x.ServiceType == typeof(ILogger<>))
                .Select(x => x.Lifetime)
                .FirstOrDefault(); // If the existing lifetime was not found, it defaults to Singleton
            services.Replace(new ServiceDescriptor(typeof(ILogger<>), typeof(ActivityBagTagDecoratingLogger<>), existingLifetime));
        }

        /// <summary>
        /// Adds a DiagnosticListener that intercepts all the incoming requests and tries
        /// To get the X-CorrelationId value for the backward compatibility with the W3C
        /// tracing standard.
        /// </summary>
        public static void UseCorrelationDiagnosticListenerSubscriber([NotNull] this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var listener = app.ApplicationServices.GetRequiredService<DiagnosticListener>();
            listener.Subscribe(new CorrelationDiagnosticListenerSubscriber());
        }

        public static void UseCorrelation([NotNull] this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseMiddleware<CorrelationTrackingMiddleware>();
            app.UseMiddleware<LogRequestMiddleware>();
            app.UseMiddleware<LogResponseMiddleware>();
        }
    }
}
