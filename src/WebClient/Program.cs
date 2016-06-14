using System;
using System.Net.Http;
using System.Reflection;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Http;
using Albumprinter.CorrelationTracking.Correlation.Log4net;
using Albumprinter.CorrelationTracking.Tracing.Http.Log4net;
using log4net;
using log4net.Config;

namespace WebClient
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            var correlationManager = CorrelationManager.Instance;
            correlationManager.ScopeInterceptors.Add(new Log4NetCorrelationScopeInterceptor());

            Console.WriteLine(@"Press any key to start...");
            Console.ReadKey(true);

            using (correlationManager.UseScope(Guid.NewGuid()))
            {
                Log.Info("correlationId: " + CorrelationScope.Current.CorrelationId);
                Log.Info("start job");

                var client = HttpClientFactory.Create(new CorrelationDelegatingHandler(), new Log4NetDelegatingHandler(true));
                client.BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute);
                var request = new HttpRequestMessage(HttpMethod.Get, @"/api/correlation");
                var response = client.SendAsync(request).GetAwaiter().GetResult();

                Log.Info("end job");
            }
            Console.WriteLine(@"Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}