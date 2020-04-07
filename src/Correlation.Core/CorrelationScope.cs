using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    [Serializable]
    public sealed class CorrelationScope
    {
        internal static Guid AutoProcessId => Guid.NewGuid();

        public CorrelationScope() : this(Guid.Empty, Guid.Empty, Guid.Empty)
        {
        }

        internal CorrelationScope(Guid processId, Guid correlationId, Guid requestId)
        {
            ProcessId = processId;
            CorrelationId = correlationId;
            RequestId = requestId;
        }

        [CanBeNull]
        public static CorrelationScope Current
        {
            get
            {
                if (Activity.Current == null) return null;
                var correlationIdX = GetBaggageItemByKnownName(CorrelationKeys.CorrelationId);
                var processIdX = GetBaggageItemByKnownName(CorrelationKeys.ProcessId);
                var requestIdX = GetBaggageItemByKnownName(CorrelationKeys.RequestId);
                if (correlationIdX == null || !Guid.TryParse(correlationIdX, out var correlationIdGuid))
                    return null;
                if (processIdX == null || !Guid.TryParse(correlationIdX, out var processIdGuid))
                    return null;
                if (requestIdX == null || !Guid.TryParse(correlationIdX, out var requestIdGuid))
                    return null;
                return new CorrelationScope(processIdGuid, correlationIdGuid, requestIdGuid);
            }
            internal set
            {
                if (Activity.Current == null) return;
                if (value == null) return;
                SetBaggageItemByKnownName(CorrelationKeys.CorrelationId, value.CorrelationId.ToString());
                SetBaggageItemByKnownName(CorrelationKeys.ProcessId, value.ProcessId.ToString());
                SetBaggageItemByKnownName(CorrelationKeys.RequestId, value.RequestId.ToString());
            }
        }

        private static string GetBaggageItemByKnownName(string name)
        {
            return Activity.Current?.GetBaggageItem(CorrelationKeys.ActivityPrefix + name);
        }

        private static void SetBaggageItemByKnownName(string name, string value)
        {
            Activity.Current?.AddBaggage(CorrelationKeys.ActivityPrefix + name, value);
        }

        /// <summary>
        ///     Gets the process identifier. The process is created once during the application lifecycle.
        /// </summary>
        /// <value>
        ///     The process identifier.
        /// </value>
        public Guid ProcessId { get; }

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
        public Guid RequestId { get; }
    }
}