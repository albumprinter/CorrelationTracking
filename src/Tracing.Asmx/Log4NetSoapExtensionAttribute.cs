using System;
using System.Web.Services.Protocols;

namespace Albumprinter.CorrelationTracking.Tracing.Asmx
{
    [AttributeUsage(AttributeTargets.Method)]
    public class Log4NetSoapExtensionAttribute : SoapExtensionAttribute
    {
        public override Type ExtensionType => typeof(Log4NetSoapExtension);


        public override int Priority { get; set; }
    }
}