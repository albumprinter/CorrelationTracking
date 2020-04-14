namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    public interface ICorrelationScopeInterceptor
    {
        void Enter(CorrelationManager manager, CorrelationScope scope);
    }
}
