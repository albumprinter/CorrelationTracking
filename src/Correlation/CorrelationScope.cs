using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Correlation
{
    public sealed class CorrelationScope
    {
        private static readonly string CorrelationScopeSlotName = typeof(CorrelationScope).FullName;
        public static readonly CorrelationScope Zero = new CorrelationScope(Guid.Empty, Guid.Empty);

        internal CorrelationScope(Guid correlationId, Guid requestId)
        {
            CorrelationId = correlationId;
            RequestId = requestId;
            Properties = new Dictionary<string, object>();
        }

        public static CorrelationScope Current
        {
            get { return CallContext.LogicalGetData(CorrelationScopeSlotName) as CorrelationScope ?? Zero; }
            internal set { CallContext.LogicalSetData(CorrelationScopeSlotName, value); }
        }

        public Guid CorrelationId { get; private set; }
        public Guid RequestId { get; private set; }
        public Dictionary<string, object> Properties { get; private set; }
    }
}