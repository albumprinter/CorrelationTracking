using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Log4net;
using Albumprinter.CorrelationTracking.Correlation.WCF;
using Albumprinter.CorrelationTracking.Tracing.WCF.Log4net;
using log4net;
using log4net.Config;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public class CorrelationClientBehaviorTests
    {
        static CorrelationClientBehaviorTests()
        {
            XmlConfigurator.Configure();
            CorrelationManager.Instance.ScopeInterceptors.Add(new Log4NetCorrelationScopeInterceptor());
        }

        public CorrelationClientBehaviorTests(ITestOutputHelper output)
        {
            var h = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            h.Root.AddAppender(new ActionAppender(output.WriteLine));
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_WCF()
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
                var client =
                    new CorrelationServices.CorrelationServiceClient(
                        new BasicHttpBinding(BasicHttpSecurityMode.None),
                        new EndpointAddress("http://localhost:60695/CorrelationService.svc"));
                client.Endpoint.Behaviors.Add(new CorrelationClientBehavior());
                client.Endpoint.Behaviors.Add(new Log4NetClientBehavior());
                client.Endpoint.Behaviors.Add(spy);

                var actual = await client.GetCorrelationIdAsync().ConfigureAwait(false);

                // assert
                Assert.Equal(expect, actual);
            }
        }
    }
}