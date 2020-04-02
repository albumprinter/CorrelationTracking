using System;
using System.Collections.Generic;

namespace Albumprinter.CorrelationTracking.Correlation.Core
{
    public sealed class CorrelationManager
    {
        public static readonly CorrelationManager Instance = new CorrelationManager();

        internal CorrelationManager()
        {
            ScopeInterceptors = new List<ICorrelationScopeInterceptor>();
        }

        public List<ICorrelationScopeInterceptor> ScopeInterceptors { get; }

        public IDisposable UseScope(Guid correlationId)
        {
            return UseScope(new CorrelationScope(CorrelationScope.Initial.ProcessId, correlationId, Guid.NewGuid()));
        }

        public IDisposable UseScope(Guid correlationId, Guid requestId)
        {
            return UseScope(new CorrelationScope(CorrelationScope.Initial.ProcessId, correlationId, requestId));
        }

        public IDisposable UseScope(CorrelationScope newScope)
        {
            if (newScope == null)
            {
                throw new ArgumentNullException(nameof(newScope));
            }
            var oldScope = CorrelationScope.Current;

            ScopeInterceptors.ForEach(x => x.Enter(this, newScope));
            CorrelationScope.Current = newScope;

            return new Disposable(
                delegate
                {
                    CorrelationScope.Current = oldScope;
                    ScopeInterceptors.ForEach(x => x.Exit(this, oldScope));
                });
        }

        private sealed class Disposable : IDisposable
        {
            private readonly Action _dispose;

            public Disposable(Action dispose)
            {
                _dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
            }

            public void Dispose()
            {
                _dispose.Invoke();
            }
        }
    }
}