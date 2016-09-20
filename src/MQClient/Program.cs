using System;
using System.Linq;
using System.Reflection;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.AmazonSqs;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Albumprinter.CorrelationTracking.MassTransit;
using Amazon.SQS;
using Amazon.SQS.Model;
using log4net;
using log4net.Config;
using MassTransit;
using MassTransit.Log4NetIntegration;

namespace MQClient
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            CorrelationTrackingConfiguration.Initialize();

            TriggerRabbitMq();
            TriggerAmazonSqs();
        }

        private static void TriggerRabbitMq()
        {
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

                    sbc.ReceiveEndpoint(host, "CorrelationTrackingTest", ep =>
                    {
                        ep.AutoDelete = true;
                        ep.Handler<TestMessage>(
                            context => { return Console.Out.WriteLineAsync($"Received: {context.Message.Text}"); });
                    });

                    sbc.UseLog4Net();
                });

            bus.UseCorrelationTracking();
            bus.Start();

            using (CorrelationManager.Instance.UseScope(Guid.NewGuid()))
            {
                bus.Publish(new TestMessage {Text = "Hi"});

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
            }

            bus.Stop();
        }

        private static void TriggerAmazonSqs()
        {
            var client = new AmazonSQSClient("AKIAJRT3MSHUD3KB5ERQ", "bEjQl/h12nwJCWwlr4x/wGvRe7+8Dsg0Y0qFS96D", new AmazonSQSConfig
            {
                ServiceURL = "https://sqs.eu-west-1.amazonaws.com"
            }).UseCorrelationTracking();

            var queue = client.ListQueues(new ListQueuesRequest {QueueNamePrefix = "d-dtap-test-queue"});
            var queueUrl = queue.QueueUrls.First();

            client.PurgeQueue(queueUrl);
            var correlationId1 = Guid.NewGuid();
            //Log.Debug($">>> Sending an individual message with correlation: {correlationId1}");
            using (CorrelationManager.Instance.UseScope(correlationId1))
            {
                var request = new SendMessageRequest
                {
                    MessageBody = "SOME MESSAGE BODY FOR TEST",
                    QueueUrl = queueUrl
                };
                var response = client.SendMessage(request);
            }
            CorrelationManager.Instance.UseScope(Guid.Empty);
            Log.Debug("After send");
            var receiveResponse = client.ReceiveMessage(queueUrl);
            foreach (var message in receiveResponse.Messages)
            {
                using (message.SetCorrelationScopeAndLog())
                {
                    // process the message here
                    client.DeleteMessage(queueUrl, message.ReceiptHandle);
                }
            }
        }
    }

    public class TestMessage
    {
        public string Text { get; set; }
    }
}