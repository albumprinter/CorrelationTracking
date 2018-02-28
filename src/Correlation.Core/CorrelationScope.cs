using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    [Serializable]
    public sealed class CorrelationScope
    {
        private static readonly string CorrelationScopeSlotName = typeof (CorrelationScope).FullName;
        public static readonly CorrelationScope Initial = new CorrelationScope(Guid.NewGuid(), Guid.Empty, Guid.Empty);

        public CorrelationScope() : this(Guid.Empty, Guid.Empty, Guid.Empty)
        {
        }

        internal CorrelationScope(Guid processId, Guid correlationId, Guid requestId)
        {
            ProcessId = processId;
            CorrelationId = correlationId;
            RequestId = requestId;
        }

        public static CorrelationScope Current
        {
            get { return CallContext.LogicalGetData(CorrelationScopeSlotName) as CorrelationScope ?? Initial; }
            internal set { CallContext.LogicalSetData(CorrelationScopeSlotName, value); }
        }

        /// <summary>
        ///     Gets the process identifier. The process is created once during the application lifecycle.
        /// </summary>
        /// <value>
        ///     The process identifier.
        /// </value>
        public Guid ProcessId { get; private set; }

        /// <summary>
        ///     Gets the correlation identifier. An explicit correlation identifier for the business transaction.
        /// </summary>
        /// <value>
        ///     The correlation identifier.
        /// </value>
        public Guid CorrelationId { get; private set; }

        /// <summary>
        ///     Gets the request identifier. An explicit correlation identifier for the request.
        /// </summary>
        /// <value>
        ///     The request identifier.
        /// </value>
        public Guid RequestId { get; private set; }

    }
}