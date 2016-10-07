﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.MassTransit;
using MassTransit;
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

            bus.UseCorrelationTracking().Start();

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

            bus.UseCorrelationTracking().Start();

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
        }

        public sealed class TestMessage
        {
            public string Text { get; set; }
        }
    }
}