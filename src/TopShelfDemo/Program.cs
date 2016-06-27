using System;
using System.Reflection;
using System.Timers;
using Albumprinter.CorrelationTracking;
using Albumprinter.CorrelationTracking.Correlation.Core;
using log4net;
using log4net.Config;
using Topshelf;

namespace TopShelfDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            XmlConfigurator.Configure();
            CorrelationTrackingConfiguration.Initialize();

            HostFactory.Run(x =>
            {
                x.Service<TimeService>(s =>
                {
                    s.ConstructUsing(name => new TimeService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.UseLog4Net();

                x.SetDescription("Sample Topshelf Host");
                x.SetDisplayName("Sample Topshelf Host");
                x.SetServiceName("Sample Topshelf Host");
            });
        }
    }

    internal sealed class TimeService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        readonly Timer timer;
        public TimeService()
        {
            timer = new Timer(1000) { AutoReset = true };
            timer.Elapsed += OnTimerOnElapsed;
        }

        private void OnTimerOnElapsed(object sender, ElapsedEventArgs eventArgs)
        {
            using (CorrelationManager.Instance.UseScope(Guid.NewGuid()))
            {
                Log.InfoFormat("It is {0} and all is well", DateTime.UtcNow);
            }
        }

        public void Start()
        {
            using (CorrelationManager.Instance.UseScope(Guid.NewGuid()))
            {
                Log.Info("exec TopShelfService.Start()");
                timer.Start();
            }
        }

        public void Stop()
        {
            using (CorrelationManager.Instance.UseScope(Guid.NewGuid()))
            {
                Log.Info("exec TopShelfService.Stop()");
                timer.Stop();
            }
        }
    }
}
