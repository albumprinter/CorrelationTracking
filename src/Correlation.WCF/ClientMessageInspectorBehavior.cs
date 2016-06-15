using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Albumprinter.CorrelationTracking.Correlation.WCF
{
    public static class MessageX
    {
        public static Message Clone(Message message)
        {
            var bytes = Encoding.UTF8.GetBytes(message.ToString());
            var xdr = XmlDictionaryReader.CreateTextReader(bytes, XmlDictionaryReaderQuotas.Max);
            return Message.CreateMessage(xdr, int.MaxValue, message.Version);
        }
    }
}