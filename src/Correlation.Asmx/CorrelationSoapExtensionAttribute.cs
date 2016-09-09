using System;
using System.Web.Services.Protocols;

namespace Albumprinter.CorrelationTracking.Correlation.Asmx
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CorrelationSoapExtensionAttribute : SoapExtensionAttribute
    {
        public override Type ExtensionType => typeof(CorrelationSoapExtension);


        public override int Priority { get; set; }
    }
}