using System.Net.Http;
using Albumprinter.CorrelationTracking.Correlation.Http;
using Albumprinter.CorrelationTracking.Tracing.Http;
using Xunit;

namespace Albumprinter.CorrelationTracking.Http.Tests
{
    public static class HttpClientExtensionsTests
    {
        public sealed class UseCorrelationTracking
        {
            [Fact]
            public void Should_register_CorrelationDelegatingHandler()
            {
                // arrange
                var client = new HttpClient();

                // act
                client.UseCorrelationTracking();

                // assert
                Assert.NotNull(client.GetHttpMessageHandler<CorrelationDelegatingHandler>());
            }

            [Fact]
            public void Should_register_Log4NetDelegatingHandler()
            {
                // arrange
                var client = new HttpClient();

                // act
                client.UseCorrelationTracking();

                // assert
                Assert.NotNull(client.GetHttpMessageHandler<Log4NetDelegatingHandler>());
            }

            [Fact]
            public void Should_register_Log4NetDelegatingHandler_after_CorrelationDelegatingHandler()
            {
                // arrange
                var client = new HttpClient();

                // act
                client.UseCorrelationTracking();
                var correlationDelegatingHandler = client.GetHttpMessageHandler<CorrelationDelegatingHandler>();
                var log4NetDelegatingHandler = client.GetHttpMessageHandler<Log4NetDelegatingHandler>();

                // assert
                Assert.Same(log4NetDelegatingHandler, correlationDelegatingHandler.InnerHandler);
            }

            [Fact]
            public void Should_reuse_default_HttpClientHandler()
            {
                // arrange
                var client = new HttpClient();

                // act
                client.UseCorrelationTracking();
                var actual = client.GetHttpMessageHandler<HttpClientHandler>();
                var expected = client.GetHttpMessageHandler<Log4NetDelegatingHandler>().InnerHandler;

                // assert
                Assert.NotNull(actual);
                Assert.Same(expected, actual);
            }
        }

        public sealed class GetHttpMessageHandler
        {
            [Fact]
            public void Should_find_HttpClientHandler()
            {
                // arrange
                var client = new HttpClient();

                // act
                client.UseCorrelationTracking();

                // assert
                Assert.NotNull(client.GetHttpMessageHandler<HttpClientHandler>());
            }
        }
    }
}