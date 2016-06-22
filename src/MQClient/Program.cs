using System;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.Correlation.MassTransit;
using Albumprinter.CorrelationTracking.Tracing.MassTransit;
using log4net.Config;
using MassTransit;
using MassTransit.Log4NetIntegration;

namespace MQClient
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            CorrelationTrackingConfiguration.Initialize();

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

                    sbc.ReceiveEndpoint(host, "FulfillmentTestTracing", ep =>
                        {
                            ep.UseRetry(Retry.Incremental(3, TimeSpan.FromSeconds(2), TimeSpan.Zero));
                            ep.Handler<TestMessage>(
                                context =>
                                {
                                    //throw new NotSupportedException("TEST_EXCEPTION");
                                    return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
                                });
                        });

                    sbc.UseLog4Net();
                });

            var correlationObserver = new CorrelationObserver();
            var tracingObserver = new Log4NetObserver();
            bus.ConnectPublishObserver(correlationObserver);
            bus.ConnectPublishObserver(tracingObserver);
            bus.ConnectConsumeObserver(correlationObserver);
            bus.ConnectConsumeObserver(tracingObserver);
            bus.Start();

            using (CorrelationManager.Instance.UseScope(Guid.NewGuid()))
            {
                bus.Publish(new TestMessage { Text = "Hi" });

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }

            bus.Stop();
        }
    }

    public class TestMessage
    {
        public string Text { get; set; }
    }
}