using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.WCF;
using Albumprinter.CorrelationTracking.Http;
using Albumprinter.CorrelationTracking.Tracing.WCF;
using log4net;
using log4net.Config;
using WebClient.CorrelationService;
using WebClient.CorrelationWebService;
using CorrelationManager = Albumprinter.CorrelationTracking.Correlation.Core.CorrelationManager;

namespace WebClient
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            CorrelationTrackingConfiguration.Initialize();

            Console.WriteLine(@"Press any key to start...");
            Console.ReadKey(true);

            var correlationManager = CorrelationManager.Instance;

            using (correlationManager.UseScope(Guid.NewGuid()))
            {
                Log.Info("HTTP correlationId: " + CorrelationScope.Current.CorrelationId);
                Log.Info("HTTP start job");

                var client = new HttpClient().UseCorrelationTracking();
                client.BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute);
                var response = client.SendAsync(new HttpRequestMessage(HttpMethod.Get, @"/api/correlation")).GetAwaiter().GetResult();
                var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                Log.Info($"HTTP end job, got: {result}");
                Debug.Assert(CorrelationScope.Current.CorrelationId == Guid.Parse(result.Trim('"')));
            }

            using (correlationManager.UseScope(Guid.NewGuid()))
            {
                Log.Info("WCF correlationId: " + CorrelationScope.Current.CorrelationId);
                Log.Info("WCF start job");

                var client = new CorrelationServiceClient();
                client.Endpoint.Behaviors.Add(new CorrelationClientBehavior());
                client.Endpoint.Behaviors.Add(new Log4NetClientBehavior());

                var result = client.GetCorrelationIdAsync().GetAwaiter().GetResult();

                Log.Info($"WCF end job, got: {result}");
                Debug.Assert(CorrelationScope.Current.CorrelationId == result);
            }

            using (correlationManager.UseScope(Guid.NewGuid()))
            {
                Log.Info("ASMX correlationId: " + CorrelationScope.Current.CorrelationId);
                Log.Info("ASMX start job");

                var client = new CorrelationWebServiceSoapClient();
                client.Endpoint.Behaviors.Add(new CorrelationClientBehavior());
                client.Endpoint.Behaviors.Add(new Log4NetClientBehavior());

                var result = client.GetCorrelationIdAsync().GetAwaiter().GetResult();

                Log.Info($"ASMX end job, got: {result}");
                Debug.Assert(CorrelationScope.Current.CorrelationId == result);
            }

            Console.WriteLine(@"Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}