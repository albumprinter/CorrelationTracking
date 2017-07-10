using System.Runtime.Remoting.Messaging;

namespace Albumprinter.CorrelationTracking.Correlation.Core.Providers
{
    public sealed class CallContextCorrelationScopeProvider : CorrelationScopeProvider
    {
        private static readonly string CorrelationScopeSlotName = typeof(CorrelationScope).FullName;

        public override CorrelationScope Scope
        {
            get => CallContext.LogicalGetData(CorrelationScopeSlotName) as CorrelationScope;
            set => CallContext.LogicalSetData(CorrelationScopeSlotName, value);
        }
    }
}