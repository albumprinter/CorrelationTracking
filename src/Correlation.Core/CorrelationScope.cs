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
                if (Activity.Current == null) return null;
                var correlationIdX = Activity.Current?.GetBaggageItem(CorrelationKeys.CorrelationId);
                var requestIdX = Activity.Current?.GetBaggageItem(CorrelationKeys.RequestId);
                if (correlationIdX == null || !Guid.TryParse(correlationIdX, out var correlationIdGuid))
                    return null;
                return new CorrelationScope(correlationIdGuid, requestIdX);
            }
            internal set
            {
                if (Activity.Current == null) return;
                if (value == null) return;
                Activity.Current?.AddBaggage(CorrelationKeys.CorrelationId, value.CorrelationId.ToString());
                Activity.Current?.AddBaggage(CorrelationKeys.RequestId, value.RequestId);
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