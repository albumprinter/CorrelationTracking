namespace Albelli.Correlation.Http
{
    public interface IHttpClientLogger
    {
        void Log(HttpClientCommunicationRequest request);
        void Log(HttpClientCommunicationResponse request);
    }
}
