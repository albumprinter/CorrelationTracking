using System;
using System.Runtime.CompilerServices;
using MassTransit;

namespace Albumprinter.CorrelationTracking.Correlation.MassTransit
{
    internal sealed class ReceiveContextManager
    {
        private readonly ConditionalWeakTable<ReceiveContext, IDisposable> weakTable;

        public ReceiveContextManager()
        {
            weakTable = new ConditionalWeakTable<ReceiveContext, IDisposable>();
        }

        public void Capture(ReceiveContext context, IDisposable disposable)
        {
            weakTable.Add(context, disposable);
        }

        public void Release(ReceiveContext context)
        {
            IDisposable disposable;
            if (weakTable.TryGetValue(context, out disposable))
            {
                weakTable.Remove(context);
                disposable?.Dispose();
            }
        }
    }
}