using System.Web;
using Albumprinter.CorrelationTracking.Correlation.Core;

namespace Albumprinter.CorrelationTracking.Correlation.IIS
{
    public sealed class IisCorrelationScopeProvider : CorrelationScopeProvider
    {
        public static readonly IisCorrelationScopeProvider Instance = new IisCorrelationScopeProvider();
        private static readonly string CorrelationScopeSlotName = typeof(CorrelationScope).FullName;

        public override CorrelationScope Scope
        {
            get => HttpContext.Current?.Items[CorrelationScopeSlotName] as CorrelationScope;
            set
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    context.Items[CorrelationScopeSlotName] = value;
                }
            }
        }
    }
}