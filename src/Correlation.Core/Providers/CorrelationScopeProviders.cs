namespace Albumprinter.CorrelationTracking.Correlation.Core.Providers
{
    public static class CorrelationScopeProviders
    {
        public static readonly CallContextCorrelationScopeProvider CallContext;
        public static readonly CompositeCorrelationScopeProvider Default;
        public static volatile CorrelationScopeProvider Current;

        static CorrelationScopeProviders()
        {
            CallContext = new CallContextCorrelationScopeProvider();
            Default = new CompositeCorrelationScopeProvider { Providers = { CallContext } };
            Current = Default;
        }
    }
}