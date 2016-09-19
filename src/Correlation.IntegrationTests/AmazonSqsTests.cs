﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Albumprinter.CorrelationTracking.AmazonSqs;
using Albumprinter.CorrelationTracking.Correlation.AmazonSqs;
using Albumprinter.CorrelationTracking.Correlation.Core;
using Amazon.SQS;
using Amazon.SQS.Internal;
using Amazon.SQS.Model;
using log4net;
using Xunit;
using Xunit.Abstractions;

namespace Correlation.IntegrationTests
{
    public sealed class AmazonSqsTests : Log4NetTest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static readonly Dictionary<string, MessageAttributeValue> messageAttributes = new Dictionary<string, MessageAttributeValue>
        {
            {"StringAttribute", new MessageAttributeValue {DataType = "String", StringValue = "StringAttributeValue"}},
            {"NumberAttribute", new MessageAttributeValue {DataType = "Number", StringValue = "1234"}},
            /*{"BinaryAttribute", new MessageAttributeValue {DataType = "Binary", BinaryValue = new MemoryStream(Encoding.UTF8.GetBytes("BinaryAttributeValue"))}},*/
            {"UPPERCASESTRINGATTRIBUTE", new MessageAttributeValue {DataType = "String", StringValue = "UPPERCASESTRINGATTRIBUTE"}},
            {"lowercasestringattribute", new MessageAttributeValue {DataType = "String", StringValue = "lowercasestringattribute"}}
        };

        public AmazonSqsTests(ITestOutputHelper output) : base(output)
        {
        }

        private static AmazonSQSClient GetConfiguredAmazonSqs()
        {
            return new AmazonSQSClient("AKIAJRT3MSHUD3KB5ERQ", "bEjQl/h12nwJCWwlr4x/wGvRe7+8Dsg0Y0qFS96D", new AmazonSQSConfig
            {
                ServiceURL = "https://sqs.eu-west-1.amazonaws.com"
            });
        }

        [Fact, Trait("Category", "Integration")]
        public void SimpleSend()
        {
            var client = GetConfiguredAmazonSqs().UseCorrelationTracking();

            var queue = client.ListQueues(new ListQueuesRequest {QueueNamePrefix = "d-dtap-test-queue"});
            var queueUrl = queue.QueueUrls.First();

            client.PurgeQueue(queueUrl);

            var correlationId1 = Guid.NewGuid();
            Log.Debug($">>> Sending an individual message with correlation: {correlationId1}");
            using (CorrelationManager.Instance.UseScope(correlationId1))
            {
                var messageBody = Guid.NewGuid().ToString("N");
                TestSendMessage(client, queueUrl, messageBody, false);
                TestReceiveMessage(client, queueUrl, correlationId1, false);
            }

            var correlationId2 = Guid.NewGuid();
            Log.Debug($">>> Sending 2 messages batch with correlation: {correlationId2}");
            using (CorrelationManager.Instance.UseScope(correlationId2))
            {
                var messageBody = Guid.NewGuid().ToString("N");
                TestSendMessageBatch(client, queueUrl, messageBody);
                TestReceiveMessage(client, queueUrl, correlationId2, false);
            }

            var correlationId3 = Guid.NewGuid();
            Log.Debug($">>> Sending an individual message with attributes and correlation: {correlationId2}");
            using (CorrelationManager.Instance.UseScope(correlationId3))
            {
                var messageBody = Guid.NewGuid().ToString("N");
                TestSendMessage(client, queueUrl, messageBody, true);
                TestReceiveMessage(client, queueUrl, correlationId3, true);
            }
        }

        private static void TestSendMessage(IAmazonSQS client, string queueURL, string messageBody, bool sendAttributes)
        {
            var request = new SendMessageRequest
            {
                MessageBody = messageBody,
                QueueUrl = queueURL
            };
            if (sendAttributes)
            {
                request.MessageAttributes = messageAttributes;
            }
            var response = client.SendMessage(request);

            ValidateMD5(request.MessageBody, response.MD5OfMessageBody);
        }

        private static void TestSendMessageBatch(IAmazonSQS client, string queueUrl, string messageBody)
        {
            var request = new SendMessageBatchRequest
            {
                QueueUrl = queueUrl,
                Entries = new List<SendMessageBatchRequestEntry>
                {
                    new SendMessageBatchRequestEntry
                    {
                        MessageBody = messageBody,
                        Id = "fooId"
                    }
                }
            };
            var response = client.SendMessageBatch(request);

            ValidateMD5(request.Entries[0].MessageBody, response.Successful[0].MD5OfMessageBody);
        }

        private static void TestReceiveMessage(IAmazonSQS client, string queueUrl, Guid expectedCorrelationId, bool receiveAttributes)
        {
            var receiveMessageRequest = new ReceiveMessageRequest {QueueUrl = queueUrl};
            if (receiveAttributes)
            {
                receiveMessageRequest.MessageAttributeNames = new List<string> {"All"};
            }
            var receiveResponse = client.ReceiveMessage(receiveMessageRequest);

            var messages = receiveResponse.Messages;
            foreach (var message in messages)
            {
                using (message.SetCorrelationScopeAndLog())
                {
                    ValidateMD5(message.Body, message.MD5OfBody);
                    Assert.Equal(expectedCorrelationId, message.ExtractCorrelationId());
                    if (receiveAttributes)
                    {
                        foreach (var messageAttributeValue in messageAttributes)
                        {
                            Assert.True(message.MessageAttributes.ContainsKey(messageAttributeValue.Key));
                            Assert.Equal(messageAttributeValue.Value.StringValue, message.MessageAttributes[messageAttributeValue.Key].StringValue);
                        }
                    }
                    client.DeleteMessage(new DeleteMessageRequest {QueueUrl = queueUrl, ReceiptHandle = message.ReceiptHandle});
                }
            }
        }

        private static void TestReceiveMessageWithOtherAttributes(IAmazonSQS client, string queueURL, Guid expectedCorrelationId)
        {
            var receiveResponse = client.ReceiveMessage(new ReceiveMessageRequest {QueueUrl = queueURL, MessageAttributeNames = new List<string> {"All"}});
            var messages = receiveResponse.Messages;
            foreach (var message in messages)
            {
                ValidateMD5(message.Body, message.MD5OfBody);
                Assert.Equal(expectedCorrelationId, message.ExtractCorrelationId());
                foreach (var messageAttributeValue in messageAttributes)
                {
                    Assert.True(message.MessageAttributes.ContainsKey(messageAttributeValue.Key));
                    Assert.Equal(messageAttributeValue.Value.StringValue, message.MessageAttributes[messageAttributeValue.Key].StringValue);
                }
                client.DeleteMessage(new DeleteMessageRequest {QueueUrl = queueURL, ReceiptHandle = message.ReceiptHandle});
            }
        }

        private static void ValidateMD5(string message, string md5)
        {
            ValidationResponseHandler.ValidateMD5(message, md5);
        }

        //public bool QueueExists(string queueName)
        //{
        //    string queueUrl;
        //    return this.TryGetQueueUrl(queueName, out queueUrl);
        //}

        //private bool TryGetQueueUrl(string queueName, out string queueUrl)
        //{
        //    this.client.ListQueues(new ListQueuesRequest()
        //    {
        //        QueueNamePrefix = queueName
        //    });
        //}


        [Fact, Trait("Category", "Integration")]
        public async Task Should_propagate_the_correlation_id_to_AmazonSqs_consumer()
        {
            //client = GetConfiguredAmazonSqs(this.settings);


            // arrange
            //var expect = Guid.NewGuid();

            //var tsc = new TaskCompletionSource<bool>();
            //var bus = Bus.Factory.CreateUsingRabbitMq(
            //    sbc =>
            //    {
            //        var host = sbc.Host(
            //            new Uri("rabbitmq://devrabbit.dtap.dcinfra.it/vhost"),
            //            h =>
            //            {
            //                h.Username("D_DEV_Rabbit");
            //                h.Password("d3vr4bb1t");
            //            });

            //        sbc.ReceiveEndpoint(host, @"CorrelationTrackingTest" + Guid.NewGuid().ToString("N"), ep =>
            //        {
            //            ep.AutoDelete = true;
            //            ep.Handler<TestMessage>(
            //                context =>
            //                {
            //                    try
            //                    {
            //                        Assert.Equal(expect, context.Headers.Get(CorrelationKeys.CorrelationId, (Guid?)Guid.Empty));
            //                        tsc.SetResult(true);
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        tsc.SetException(ex);
            //                    }
            //                    return Task.FromResult(true);
            //                });
            //        });
            //        // NOTE: FYI sbc.UseLog4Net();
            //    });

            //bus.UseCorrelationTracking().Start();

            //try
            //{
            //    using (CorrelationManager.Instance.UseScope(expect))
            //    {
            //        await bus.Publish(new TestMessage { Text = "Hi" }).ConfigureAwait(false);
            //    }

            //    await tsc.Task.ConfigureAwait(false);
            //}
            //finally
            //{
            //    // NOTE: wait untill the consumer will send the ank request to rabbitmq
            //    await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            //    bus.Stop();
            //}
        }

        public sealed class TestMessage
        {
            public string Text { get; set; }
        }
    }
}