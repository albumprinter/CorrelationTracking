using System;
using System.IO;
using System.Reflection;
using System.Web.Services.Protocols;
using log4net;

namespace Albumprinter.CorrelationTracking.Tracing.Asmx
{
    public class Log4NetSoapExtension : SoapExtension
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
                    break;
                case SoapMessageStage.AfterSerialize:
                    LogResponse(message);
                    CopyBufferToTransport();
                    break;
                case SoapMessageStage.BeforeDeserialize:
                    CopyTransportToBuffer();
                    break;
                case SoapMessageStage.AfterDeserialize:
                    LogRequest(message);
                    break;
                default:
                    throw new Exception("invalid stage");
            }
        }

        public void LogResponse(SoapMessage response)
        {
            bufferStream.Position = 0;
            var reader = new StreamReader(bufferStream);
            var messageString = reader.ReadToEnd();
            Log.Debug(messageString);
        }


        public void LogRequest(SoapMessage request)
        {
            bufferStream.Position = 0;
            var reader = new StreamReader(bufferStream);
            var messageString = reader.ReadToEnd();
            Log.Debug(messageString);
            bufferStream.Position = 0;
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
    }
}