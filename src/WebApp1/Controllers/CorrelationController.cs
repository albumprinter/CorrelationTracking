using System;
using System.Reflection;
using System.Web.Http;
using Albumprinter.CorrelationTracking;
using log4net;

namespace WebApp1.Controllers
{
    public class CorrelationController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Guid Get()
        {
            try
            {
                Log.Info("WEBAPI: on start");
                return CorrelationScope.Current.CorrelationId;
            }
            finally
            {
                Log.Info("WEBAPI: on end");
            }
        }
    }
}