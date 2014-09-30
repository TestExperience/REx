using System.ServiceModel;

namespace REx.Common.ServiceContracts
{
    /// <summary>
    /// This is the Service contract of REx service
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IRemoteExecutorCallback))]
    public interface IRemoteExecutor : IRemoteExecutorMethods
    {
        [OperationContract]
        void ConnectClient();

        [OperationContract]
        void DisconnectClient();
    }
}
