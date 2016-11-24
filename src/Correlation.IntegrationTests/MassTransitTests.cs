using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.MassTransit;
using Albumprinter.CorrelationTracking.MassTransit;
using Albumprinter.CorrelationTracking.Tracing.MassTransit;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using MassTransit;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public sealed class MassTransitTests
    {
        private readonly ActionAppender TestAppender;
        private readonly RootLogger TestLogger;
        private readonly ILog TestLog;
        private readonly ITestOutputHelper Output;

        static MassTransitTests()
        {
            XmlConfigurator.Configure();
            CorrelationTrackingConfiguration.Initialize();
        }

        public MassTransitTests(ITestOutputHelper output)
        {
            Output = output;
            TestAppender = new ActionAppender(Output.WriteLine, @"[PI:%property{X-ProcessId}]%n[CI:%property{X-CorrelationId}]%n[RI:%property{X-RequestId}]%n%date %-5level %m%n%n");
            TestLogger = new RootLogger(Level.All) { Hierarchy = new Hierarchy { Configured = true, Threshold = Level.All } };
            TestLogger.AddAppender(TestAppender);
            TestLog = new LogImpl(TestLogger);

            CorrelationIds = new List<string>();
            TestLogger.AddAppender(new ActionAppender(text => CorrelationIds.Add(text), "%property{X-CorrelationId}"));
        }

        private List<string> CorrelationIds { get; set; }

        [Fact, Trait("Category", "Integration")]
        public async Task Publish_should_propagate_the_correlation_id_to_RabbitMQ_consumer()
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
                                    Assert.Equal(expect, context.ReceiveContext.TransportHeaders.Get(CorrelationKeys.CorrelationId, (Guid?)Guid.Empty));
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

            bus.UseCorrelationTracking().UseLog4NetObserver(new Log4NetObserver { Log = TestLog }).Start();

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

            Assert.All(CorrelationIds, actual => Assert.Equal(expect.ToString(), actual));
        }

        [Fact, Trait("Category", "Integration")]
        public async Task Send_should_propagate_the_correlation_id_to_RabbitMQ_consumer()
        {
            // arrange
            var expect = Guid.NewGuid();

            var queueName = @"CorrelationTrackingTest" + Guid.NewGuid().ToString("N");

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

                    sbc.ReceiveEndpoint(host, queueName, ep =>
                    {
                        ep.AutoDelete = true;
                        ep.Handler<TestMessage>(
                            context =>
                            {
                                try
                                {
                                    Assert.Equal(expect, context.ReceiveContext.TransportHeaders.Get(CorrelationKeys.CorrelationId, (Guid?)Guid.Empty));
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

            bus.UseCorrelationTracking().UseLog4NetObserver(new Log4NetObserver { Log = TestLog }).Start();

            try
            {
                using (CorrelationManager.Instance.UseScope(expect))
                {
                    var address = new Uri(bus.Address, queueName + "?autodelete=true");
                    var endpoint = await bus.GetSendEndpoint(address).ConfigureAwait(false);
                    await endpoint.Send(new TestMessage { Text = "Hi" }).ConfigureAwait(false);
                }

                await tsc.Task.ConfigureAwait(false);
            }
            finally
            {
                // NOTE: wait untill the consumer will send the ank request to rabbitmq
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                bus.Stop();
            }

            Assert.All(CorrelationIds, actual => Assert.Equal(expect.ToString(), actual));
        }

        [Fact, Trait("Category", "Integration")]
        public async Task PreConsume_should_try_to_restore_the_correlation_if_PreReceive_failed()
        {
            // arrange
            var expect = Guid.NewGuid();

            var queueName = @"CorrelationTrackingTest" + Guid.NewGuid().ToString("N");

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

                    sbc.ReceiveEndpoint(host, queueName, ep =>
                    {
                        ep.AutoDelete = true;
                        ep.Handler<TestMessage>(
                            context =>
                            {
                                try
                                {
                                    // NOTE: Masstransit.Rabbitmq v3.3.5 copies context.headers to transport.headers before calling PreSend method :(
                                    Assert.Equal(Guid.Empty, context.ReceiveContext.TransportHeaders.Get(CorrelationKeys.CorrelationId, (Guid?)Guid.Empty));
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

            var correlationObserver = new CorrelationObserver { InjectOnPreSendToRabbitMqProperties = false };
            bus.ConnectSendObserver(correlationObserver);
            bus.ConnectConsumeObserver(correlationObserver);
            bus.UseLog4NetObserver(new Log4NetObserver { Log = TestLog }).Start();

            try
            {
                using (CorrelationManager.Instance.UseScope(expect))
                {
                    var address = new Uri(bus.Address, queueName + "?autodelete=true");
                    var endpoint = await bus.GetSendEndpoint(address).ConfigureAwait(false);
                    await endpoint.Send(new TestMessage { Text = "Hi" }).ConfigureAwait(false);
                }

                await tsc.Task.ConfigureAwait(false);
            }
            finally
            {
                // NOTE: wait untill the consumer will send the ank request to rabbitmq
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

                bus.Stop();
            }

            CorrelationIds.RemoveAll(x => string.Equals(x, "(null)", StringComparison.Ordinal));
            Assert.All(CorrelationIds, actual => Assert.Equal(expect.ToString(), actual));
        }

        public sealed class TestMessage
        {
            public string Text { get; set; }
        }
    }
}