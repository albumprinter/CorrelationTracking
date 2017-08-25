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
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_Mvc()
        {
            var client = new HttpClient { BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute) }
                .UseCorrelationTracking();
            // arrange
            var expect = Guid.NewGuid();

            // act
            using (CorrelationManager.Instance.UseScope(expect))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, @"/Home/Index");
                var response = await client.SendAsync(request).ConfigureAwait(false);

                // assert
                Assert.True(response.Headers.Contains(CorrelationKeys.CorrelationId));
                Assert.Equal(expect.ToString(), response.Headers.GetValues(CorrelationKeys.CorrelationId).First());
            }
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_WebAPI()
        {
            var client = new HttpClient { BaseAddress = new Uri("http://localhost:60695/", UriKind.Absolute) }
                .UseCorrelationTracking();

            // arrange
            var expect = Guid.NewGuid();

            // act
            using (CorrelationManager.Instance.UseScope(expect))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, @"/api/correlation");
                var response = await client.SendAsync(request).ConfigureAwait(false);

                // assert
                Assert.True(response.Headers.Contains(CorrelationKeys.CorrelationId));
                Assert.Equal(expect.ToString(), response.Headers.GetValues(CorrelationKeys.CorrelationId).First());
            }
        }
    }
}
