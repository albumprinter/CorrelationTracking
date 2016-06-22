using System;
using System.Net.Http;
using System.Reflection;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.Http;
using Albumprinter.CorrelationTracking.Tracing.Http;
using log4net;

namespace WebClient
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            CorrelationTrackingConfiguration.Initialize();

            Console.WriteLine(@"Press any key to start...");
            Console.ReadKey(true);

            var correlationManager = CorrelationManager.Instance;

            using (correlationManager.UseScope(Guid.NewGuid()))
            {
                Log.Info("correlationId: " + CorrelationScope.Current.CorrelationId);
                Log.Info("start job");

                var client = HttpClientFactory.Create(new CorrelationDelegatingHandler(), new Log4NetDelegatingHandler(true));
                client.BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute);
                client.SendAsync(new HttpRequestMessage(HttpMethod.Get, @"/api/correlation")).GetAwaiter().GetResult();

                Log.Info("end job");
            }

            using (correlationManager.UseScope(Guid.NewGuid()))
            {
                Log.Info("correlationId: " + CorrelationScope.Current.CorrelationId);
                Log.Info("start job");

                var client = HttpClientFactory.Create(new CorrelationDelegatingHandler(), new Log4NetDelegatingHandler(true));
                client.BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute);
                client.SendAsync(new HttpRequestMessage(HttpMethod.Get, @"/api/correlation")).GetAwaiter().GetResult();

                Log.Info("end job");
            }
            Console.WriteLine(@"Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}