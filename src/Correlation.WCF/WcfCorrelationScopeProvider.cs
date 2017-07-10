using System.ServiceModel;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albumprinter.CorrelationTracking.Correlation.WCF
{
    public sealed class WcfCorrelationScopeProvider : CorrelationScopeProvider
    {
        public static readonly WcfCorrelationScopeProvider Instance = new WcfCorrelationScopeProvider();
        private static readonly string CorrelationScopeSlotName = typeof(CorrelationScope).FullName;

        public override CorrelationScope Scope
        {
            get
            {
                object value = null;
                OperationContext.Current?.RequestContext.RequestMessage.Properties.TryGetValue(CorrelationScopeSlotName, out value);
                return value as CorrelationScope;
            }
            set
            {
                var context = OperationContext.Current;
                if (context != null)
                {
                    context.RequestContext.RequestMessage.Properties[CorrelationScopeSlotName] = value;
                }
            }
        }
    }
}