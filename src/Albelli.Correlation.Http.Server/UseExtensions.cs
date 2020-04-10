using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Albelli.Correlation.Http.Server.Middleware;
using Albelli.CorrelationTracking.Correlation.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Albelli.Correlation.Http.Server
{
    [PublicAPI]
    public static class UseExtensions
    {
        /// <summary>
        /// Adds a decorator for ILoggerFactory that can decorate log messages
        /// with values from Activity.Current.Baggage and Activity.Current.Tags.
        ///
        /// It will log:
        /// 1) All the tag values
        /// 2) Values from the baggage that start with "X-"
        /// </summary>
        /// <param name="services">The service collection from the IoC container</param>
        public static void AddDefaultActivityBagTagLoggerFactory([NotNull] this IServiceCollection services)
        {
            services.AddActivityBagTagLoggerFactory(s => s.StartsWith("X-", StringComparison.OrdinalIgnoreCase), _ => true);
        }

        /// <summary>
        /// Adds a decorator for ILoggerFactory that can decorate log messages
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
        public static void AddActivityBagTagLoggerFactory([NotNull] this IServiceCollection services, Predicate<string> shouldLogBag = null, Predicate<string> shouldLogTag = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (shouldLogBag == null && shouldLogTag == null)
                throw new ArgumentException("At least one predicate should be specified");

            services.Decorate<ILoggerFactory, ActivityBagTagLoggerFactory>(shouldLogBag, shouldLogTag);
        }

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

    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services, params object[] additionalParameters)
            where TDecorator : TService
        {
            return services.DecorateDescriptors(typeof(TService), x => x.Decorate(typeof(TDecorator), additionalParameters));
        }

        public static IServiceCollection Decorate<TService, TDecorator>(this IServiceCollection services)
            where TDecorator : TService
        {
            return services.DecorateDescriptors(typeof(TService), x => x.Decorate(typeof(TDecorator)));
        }

        private static IServiceCollection DecorateDescriptors(this IServiceCollection services, Type serviceType, Func<ServiceDescriptor, ServiceDescriptor> decorator)
        {
            var descriptors = services.Where(service => service.ServiceType == serviceType).ToArray();
            if (descriptors.Length == 0)
            {
                throw new InvalidOperationException("Can't find the type to decorate");
            }

            foreach (var descriptor in descriptors)
            {
                var index = services.IndexOf(descriptor);
                services[index] = decorator(descriptor);
            }

            return services;

        }

        private static ServiceDescriptor Decorate(this ServiceDescriptor descriptor, Type decoratorType, object[] additionalParameters)
        {
            return descriptor.WithFactory(provider => provider.CreateInstance(decoratorType, provider.GetInstance(descriptor), additionalParameters));
        }

        private static ServiceDescriptor Decorate(this ServiceDescriptor descriptor, Type decoratorType)
        {
            return descriptor.WithFactory(provider => provider.CreateInstance(decoratorType, provider.GetInstance(descriptor)));
        }

        private static ServiceDescriptor WithFactory(this ServiceDescriptor descriptor, Func<IServiceProvider, object> factory)
        {
            return ServiceDescriptor.Describe(descriptor.ServiceType, factory, descriptor.Lifetime);
        }

        private static object GetInstance(this IServiceProvider provider, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationType != null)
            {
                return provider.GetServiceOrCreateInstance(descriptor.ImplementationType);
            }

            return descriptor.ImplementationFactory(provider);
        }

        private static object GetServiceOrCreateInstance(this IServiceProvider provider, Type type)
        {
            return ActivatorUtilities.GetServiceOrCreateInstance(provider, type);
        }

        private static object CreateInstance(this IServiceProvider provider, Type type, params object[] arguments)
        {
            return ActivatorUtilities.CreateInstance(provider, type, arguments);
        }
    }
}
