using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Services.Protocols;
using System.Xml;
using Albumprinter.CorrelationTracking.Correlation.Core;
using log4net;

namespace Albumprinter.CorrelationTracking.Correlation.Asmx
{
    public class CorrelationSoapExtension : SoapExtension
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        Stream transportStream;
        Stream bufferStream;


        public override Stream ChainStream(Stream stream)
        {
            transportStream = stream;
            bufferStream = new MemoryStream();
            return bufferStream;
        }


        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null;
        }


        public override object GetInitializer(Type WebServiceType)
        {
            return null;
        }


        public override void Initialize(object initializer)
        {
        }


        public override void ProcessMessage(SoapMessage message)
        {
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeSerialize:
                    ProcessResponse(message);
                    break;
                case SoapMessageStage.AfterSerialize:
                    CopyBufferToTransport();
                    break;
                case SoapMessageStage.BeforeDeserialize:
                    CopyTransportToBuffer();
                    break;
                case SoapMessageStage.AfterDeserialize:
                    ProcessRequest(message);
                    break;
                default:
                    throw new Exception("invalid stage");
            }
        }

        private void CopyBufferToTransport()
        {
            bufferStream.Position = 0;
            if (transportStream.CanSeek)
            {
                transportStream.Position = 0;
            }
            bufferStream.CopyTo(transportStream);
            bufferStream.Position = 0;
            if (transportStream.CanSeek)
            {
                transportStream.Position = 0;
            }
        }

        private void CopyTransportToBuffer()
        {
            bufferStream.Position = 0;
            if (transportStream.CanSeek)
            {
                transportStream.Position = 0;
            }
            transportStream.CopyTo(bufferStream);
            bufferStream.Position = 0;
            if (transportStream.CanSeek)
            {
                transportStream.Position = 0;
            }
        }

        public void ProcessResponse(SoapMessage response)
        {
            var scope = CorrelationScope.Current;
            if (scope != CorrelationScope.Initial)
            {
                var doc = new XmlDocument();
                var element = doc.CreateElement(null, CorrelationKeys.CorrelationId, CorrelationKeys.Namespace);
                element.InnerText = scope.CorrelationId.ToString();
                response.Headers.Add(
                    new SoapUnknownHeader
                    {
                        Element = element,
                        MustUnderstand = false
                    });
            }
        }


        public void ProcessRequest(SoapMessage request)
        {
            var correlationHeader = request.Headers.OfType<SoapUnknownHeader>().FirstOrDefault(sh => sh.Element.Name == CorrelationKeys.CorrelationId);
            var correlationId = Guid.Empty;
            if (correlationHeader != null && string.IsNullOrEmpty(correlationHeader.Element.InnerText) == false)
            {
                correlationId = Guid.Parse(correlationHeader.Element.InnerText);
            }
            if (correlationId == Guid.Empty)
            {
                correlationId = Guid.NewGuid();
            }
            CorrelationManager.Instance.UseScope(correlationId);
        }
    }
}