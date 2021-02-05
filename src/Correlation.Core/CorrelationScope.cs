using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    [Serializable]
    public sealed class CorrelationScope
    {
        internal static Guid AutoProcessId => Guid.NewGuid();

        internal CorrelationScope(Guid correlationId, string requestId)
        {
            CorrelationId = correlationId;
            RequestId = requestId;
        }

        [CanBeNull]
        public static CorrelationScope Current
        {
            get
            {
                var currentActivity = Activity.Current;
                if (currentActivity == null) return null;
                var correlationIdX = currentActivity.GetTagItem(CorrelationKeys.CorrelationId);
                var requestIdX = currentActivity.GetTagItem(CorrelationKeys.RequestId);
                if (correlationIdX == null || !Guid.TryParse(correlationIdX, out var correlationIdGuid))
                    return null;
                return new CorrelationScope(correlationIdGuid, requestIdX);
            }
            internal set
            {
                var currentActivity = Activity.Current;
                if (currentActivity == null) return;
                if (value == null) return;
                currentActivity.AddTag(CorrelationKeys.CorrelationId, value.CorrelationId.ToString());
                currentActivity.AddTag(CorrelationKeys.RequestId, value.RequestId);
            }
        }

        /// <summary>
        ///     Gets the process identifier. The process is created once during the application lifecycle.
        /// </summary>
        /// <value>
        ///     The process identifier.
        /// </value>
        public Guid ProcessId => AutoProcessId;

        /// <summary>
        ///     Gets the correlation identifier. An explicit correlation identifier for the business transaction.
        /// </summary>
        /// <value>
        ///     The correlation identifier.
        /// </value>
        public Guid CorrelationId { get; }

        /// <summary>
        ///     Gets the request identifier. An explicit correlation identifier for the request.
        /// </summary>
        /// <value>
        ///     The request identifier.
        /// </value>
        public String RequestId { get; }
    }
}