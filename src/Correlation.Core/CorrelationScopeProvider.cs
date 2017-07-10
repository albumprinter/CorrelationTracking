namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    public abstract class CorrelationScopeProvider
    {
        public abstract CorrelationScope Scope { get; set; }
    }
}