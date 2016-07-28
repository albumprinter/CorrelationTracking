using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Http;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public sealed class HttpClientTests : Log4NetTest
    {
        public HttpClientTests(ITestOutputHelper output) : base(output)
        {
            Client = new HttpClient().UseCorrelationTracking();
            Client.BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute);
        }

        public HttpClient Client { get; }

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
