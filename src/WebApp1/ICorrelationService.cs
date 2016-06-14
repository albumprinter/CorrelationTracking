using System;
using System.ServiceModel;

namespace WebApp1
{
    [ServiceContract]
    public interface ICorrelationService
    {
        [OperationContract]
        Guid GetCorrelationId();

        [OperationContract]
        void ThrowError();
    }
}
