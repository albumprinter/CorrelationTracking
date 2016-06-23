using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.Http;
using Albumprinter.CorrelationTracking.Tracing.Http;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public class CorrelationDelegatingHandlerTests : Log4NetTest
    {
        public CorrelationDelegatingHandlerTests(ITestOutputHelper output) : base(output)
        {
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
