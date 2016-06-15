using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Albumprinter.CorrelationTracking
{
    public sealed class CorrelationScope
    {
        private static readonly string CorrelationScopeSlotName = typeof(CorrelationScope).FullName;
        public static readonly CorrelationScope Zero = new CorrelationScope(Guid.Empty, Guid.Empty, Guid.Empty);

        internal CorrelationScope(Guid processId, Guid correlationId, Guid requestId)
        {
            ProcessId = processId;
            CorrelationId = correlationId;
            RequestId = requestId;
            Properties = new Dictionary<string, object>();
        }

        public static CorrelationScope Current
        {
            get { return CallContext.LogicalGetData(CorrelationScopeSlotName) as CorrelationScope ?? Zero; }
            internal set { CallContext.LogicalSetData(CorrelationScopeSlotName, value); }
        }

        /// <summary>
        /// Gets the process identifier. The process is created once during the application lifecycle.
        /// </summary>
        /// <value>
        /// The process identifier.
        /// </value>
        public Guid ProcessId { get; private set; }

        /// <summary>
        /// Gets the correlation identifier. An explicit correlation identifier for the business transaction.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public Guid CorrelationId { get; private set; }


        /// <summary>
        /// Gets the request identifier. An explicit correlation identifier for the request.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public Guid RequestId { get; private set; }

        public Dictionary<string, object> Properties { get; private set; }
    }
}