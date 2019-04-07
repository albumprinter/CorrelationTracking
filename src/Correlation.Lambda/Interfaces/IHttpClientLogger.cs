namespace Albumprinter.CorrelationTracking.Correlation.Interfaces
{
    public interface IHttpClientLogger
    {
        void Log(HttpClientCommunicationRequest request);
        void Log(HttpClientCommunicationResponse request);
    }
}
