using System;
using System.Collections.Generic;

namespace Correlation.Core.Standard
{
    public sealed class CorrelationManager
    {
        public static readonly CorrelationManager Instance = new CorrelationManager();

        internal CorrelationManager()
        {
            ScopeInterceptors = new List<ICorrelationScopeInterceptor>();
        }

        public List<ICorrelationScopeInterceptor> ScopeInterceptors { get; private set; }

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
                throw new ArgumentNullException("newScope");
            }
            var oldScope = CorrelationScope.Current;

            ScopeInterceptors.ForEach(x => x.Enter(this, newScope));
            CorrelationScope.Current = newScope;

            return new Disposable(
                delegate
                {
                    CorrelationScope.Current = oldScope;
                    ScopeInterceptors.ForEach(x => x.Exit(this, newScope));
                });
        }

        private sealed class Disposable : IDisposable
        {
            private readonly Action dispose;

            public Disposable(Action dispose)
            {
                if (dispose == null)
                {
                    throw new ArgumentNullException("dispose");
                }
                this.dispose = dispose;
            }

            public void Dispose()
            {
                dispose.Invoke();
            }
        }
    }
}