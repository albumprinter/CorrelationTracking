using System;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.MassTransit;
using Albumprinter.CorrelationTracking.Tracing.MassTransit;
using MassTransit;
using MassTransit.Log4NetIntegration;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public sealed class MassTransitTests : Log4NetTest
    {
        public MassTransitTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_RabbitMQ_consumer()
        {
            // arrange
            var expect = Guid.NewGuid();

            var tsc = new TaskCompletionSource<bool>();
            var bus = Bus.Factory.CreateUsingRabbitMq(
                sbc =>
                {
                    var host = sbc.Host(
                        new Uri("rabbitmq://devrabbit.dtap.dcinfra.it/vhost"),
                        h =>
                        {
                            h.Username("D_DEV_Rabbit");
                            h.Password("d3vr4bb1t");
                        });

                    sbc.ReceiveEndpoint(host, @"CorrelationTrackingTest" + Guid.NewGuid().ToString("N"), ep =>
                    {
                        ep.AutoDelete = true;
                        ep.Handler<TestMessage>(
                            context =>
                            {
                                try
                                {
                                    Assert.Equal(expect, context.Headers.Get(CorrelationKeys.CorrelationId, (Guid?)Guid.Empty));
                                    tsc.SetResult(true);
                                }
                                catch (Exception ex)
                                {
                                    tsc.SetException(ex);
                                }
                                return Task.FromResult(true);
                            });
                    });

                    // NOTE: FYI sbc.UseLog4Net();
                });

            bus.ConnectPublishObserver(new CorrelationObserver());
            bus.ConnectPublishObserver(new Log4NetObserver());
            bus.ConnectConsumeObserver(new CorrelationObserver());
            bus.ConnectConsumeObserver(new Log4NetObserver());

            bus.Start();

            try
            {
                using (CorrelationManager.Instance.UseScope(expect))
                {
                    await bus.Publish(new TestMessage { Text = "Hi" }).ConfigureAwait(false);
                }

                await tsc.Task.ConfigureAwait(false);
            }
            finally
            {
                // NOTE: wait untill the consumer will send the ank request to rabbitmq
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                bus.Stop();
            }
        }

        public class TestMessage
        {
            public string Text { get; set; }
        }
    }
}