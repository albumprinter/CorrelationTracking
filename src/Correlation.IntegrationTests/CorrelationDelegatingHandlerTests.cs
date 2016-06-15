using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Http;
using Albumprinter.CorrelationTracking.Correlation.Log4net;
using Albumprinter.CorrelationTracking.Tracing.Http.Log4net;
using log4net;
using log4net.Config;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public class CorrelationDelegatingHandlerTests
    {
        static CorrelationDelegatingHandlerTests()
        {
            XmlConfigurator.Configure();
            CorrelationManager.Instance.ScopeInterceptors.Add(new Log4NetCorrelationScopeInterceptor());
        }

        public CorrelationDelegatingHandlerTests(ITestOutputHelper output)
        {
            var h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            h.Root.AddAppender(new ActionAppender(output.WriteLine));

            Client = HttpClientFactory.Create(new CorrelationDelegatingHandler(), new Log4NetDelegatingHandler(true));
            Client.BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute);
        }

        public HttpClient Client { get; set; }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_Mvc()
        {
            // arrange
            var expect = Guid.NewGuid();

            // act
            using (CorrelationManager.Instance.UseScope(expect))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, @"/Home/Index");
                var response = await Client.SendAsync(request).ConfigureAwait(false);

                // assert
                Assert.True(response.Headers.Contains(CorrelationKeys.CorrelationId));
                Assert.Equal(expect.ToString(), response.Headers.GetValues(CorrelationKeys.CorrelationId).First());
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_WebAPI()
        {
            // arrange
            var expect = Guid.NewGuid();

            // act
            using (CorrelationManager.Instance.UseScope(expect))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, @"/api/correlation");
                var response = await Client.SendAsync(request).ConfigureAwait(false);

                // assert
                Assert.True(response.Headers.Contains(CorrelationKeys.CorrelationId));
                Assert.Equal(expect.ToString(), response.Headers.GetValues(CorrelationKeys.CorrelationId).First());
            }
        }
    }
}
