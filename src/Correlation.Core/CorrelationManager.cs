using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            return UseScope(correlationId, Guid.NewGuid().ToString());
        }

        public IDisposable UseScope(Guid correlationId, string requestId)
        {
            return UseScope(new CorrelationScope( correlationId, requestId));
        }

        public IDisposable UseScope(CorrelationScope newScope)
        {
            if (newScope == null)
            {
                throw new ArgumentNullException(nameof(newScope));
            }

            var correlationActivity = new Activity(nameof(CorrelationManager))
                .Start();
            ScopeInterceptors.ForEach(x => x.Enter(this, newScope));
            CorrelationScope.Current = newScope;
            return new Disposable(() => correlationActivity.Stop());
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