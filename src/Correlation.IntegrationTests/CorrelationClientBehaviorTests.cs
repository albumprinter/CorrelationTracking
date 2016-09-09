using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.WCF;
using Albumprinter.CorrelationTracking.Tracing.WCF;
using Correlation.IntegrationTests.CorrelationServices;
using Correlation.IntegrationTests.CorrelationWebServices;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public class CorrelationClientBehaviorTests : Log4NetTest
    {
        public CorrelationClientBehaviorTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_WCF()
        {
            var client =
                new CorrelationServiceClient(
                    new BasicHttpBinding(BasicHttpSecurityMode.None),
                    new EndpointAddress("http://localhost:60695/CorrelationService.svc"));
            await
                TestClient<CorrelationServiceClient, ICorrelationService>(client,
                    cl => cl.GetCorrelationIdAsync().ConfigureAwait(false).GetAwaiter().GetResult());
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_ASMX()
        {
            var client =
                new CorrelationWebServiceSoapClient(
                    new BasicHttpBinding(BasicHttpSecurityMode.None),
                    new EndpointAddress("http://localhost:60695/CorrelationWebService.asmx"));
            await
                TestClient<CorrelationWebServiceSoapClient, CorrelationWebServiceSoap>(client,
                    cl => cl.GetCorrelationIdAsync().ConfigureAwait(false).GetAwaiter().GetResult());
        }

        public async Task TestClient<T, TChannel>(T client, Func<T, Guid> act)
            where T : ClientBase<TChannel>
            where TChannel : class
        {
            // arrange
            var expect = Guid.NewGuid();
            var spy = new MessageInspectorClientBehavior();

            spy.BeforeSend += delegate(object sender, Message message)
            {
                Assert.NotEqual(-1, message.Headers.FindHeader(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace));
                Assert.Equal(expect, message.Headers.GetHeader<Guid>(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace));
            };

            spy.AfterReceive += delegate(object sender, Message message)
            {
                Assert.NotEqual(-1, message.Headers.FindHeader(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace));
                Assert.Equal(expect, message.Headers.GetHeader<Guid>(CorrelationKeys.CorrelationId, CorrelationKeys.Namespace));
            };

            // act
            using (CorrelationManager.Instance.UseScope(expect))
            {
                client.Endpoint.Behaviors.Add(new CorrelationClientBehavior());
                client.Endpoint.Behaviors.Add(new Log4NetClientBehavior());
                client.Endpoint.Behaviors.Add(spy);

                var actual = act(client);

                // assert
                Assert.Equal(expect, actual);
            }
        }
    }
}