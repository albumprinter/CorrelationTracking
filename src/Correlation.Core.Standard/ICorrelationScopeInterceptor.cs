namespace Correlation.Core.Standard
{
    public interface ICorrelationScopeInterceptor
    {
        void Enter(CorrelationManager manager, CorrelationScope scope);
        void Exit(CorrelationManager manager, CorrelationScope scope);
    }
}