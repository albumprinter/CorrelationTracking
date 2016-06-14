using System;
using System.Reflection;
using System.Web.Http;
using log4net;

namespace WebApp1.Controllers
{
    public class ErrorController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Guid Get()
        {
            try
            {
                Log.Info("WEBAPI: on start");
                throw new NotSupportedException("TEST_EXCEPTION!");
            }
            finally
            {
                Log.Info("WEBAPI: on end");
            }
        }
    }
}