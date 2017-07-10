using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albumprinter.CorrelationTracking
{
    public static class CorrelationTrackingConfiguration
    {
        private static volatile bool initialized;

        public static void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                Launch();

                CorrelationManager.Instance.UseScope(Guid.Empty, Guid.Empty);
            }
        }

        internal static void Launch()
        {
            var appDomain = AppDomain.CurrentDomain;
            foreach (var libraryPath in Directory.EnumerateFiles(appDomain.RelativeSearchPath ?? appDomain.BaseDirectory, "*CorrelationTracking*.dll"))
            {
                Assembly.LoadFile(Path.GetFullPath(libraryPath));
            }
            Configure(appDomain.GetAssemblies().ToArray());
        }

        internal static void Configure(params Assembly[] assemblies)
        {
            var configuratorTypes = assemblies.SelectMany(assembly => assembly.ExportedTypes)
                .Where(type => !type.IsAbstract && typeof(ICorrelationTrackingConfiguration).IsAssignableFrom(type));
            foreach (var configuratorType in configuratorTypes)
            {
                try
                {
                    var initializer = Activator.CreateInstance(configuratorType) as ICorrelationTrackingConfiguration;
                    initializer?.Configure();
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Trace.WriteLine(exception);
                }
            }
        }
    }
}