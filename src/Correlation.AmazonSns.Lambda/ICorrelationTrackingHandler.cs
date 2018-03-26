﻿using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;

namespace Albumprinter.CorrelationTracking.Correlation.AmazonSns.Lambda
{
    public interface ICorrelationTrackingHandler
    {
        Task Handle(SNSEvent snsEvent);
    }
}