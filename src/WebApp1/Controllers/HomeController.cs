using System;
using System.Reflection;
using System.Web.Mvc;
using Albumprinter.CorrelationTracking.Correlation.Core;
using log4net;

namespace WebApp1.Controllers
{
    public class HomeController : Controller
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index()
        {
            try
            {
                Log.Info("MVC: on start");
                return View(CorrelationScope.Current);
            }
            finally
            {
                Log.Info("MVC: on end");
            }
        }
        public ActionResult Error()
        {
            throw new NotSupportedException("TEST_ERROR!");
        }
    }
}