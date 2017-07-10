using System.Collections.Generic;
using System.Linq;

namespace Albumprinter.CorrelationTracking.Correlation.Core.Providers
{
    public sealed class CompositeCorrelationScopeProvider : CorrelationScopeProvider
    {
        public List<CorrelationScopeProvider> Providers { get; } = new List<CorrelationScopeProvider>();

        public override CorrelationScope Scope
        {
            get { return Providers.Select(provider => provider.Scope).FirstOrDefault(scope => scope != null); }
            set { Providers.ForEach(provider => provider.Scope = value); }
        }
    }
}