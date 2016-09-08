using System;
using System.ComponentModel;
using System.Reflection;
using System.Web.Services;
using Albumprinter.CorrelationTracking.Correlation.Core;
using log4net;

namespace WebApp1
{
    /// <summary>
    ///     Summary description for CorrelationWebService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class CorrelationWebService : WebService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [WebMethod]
        public Guid GetCorrelationId()
        {
            try
            {
                Log.Info("ASMX: on start");
                return CorrelationScope.Current.CorrelationId;
            }
            finally
            {
                Log.Info("ASMX: on end");
            }
        }

        [WebMethod]
        public void ThrowError()
        {
            throw new NotSupportedException("ASMX: TEST_ERROR!");
        }
    }
}