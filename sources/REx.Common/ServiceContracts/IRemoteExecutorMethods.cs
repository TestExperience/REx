using System.ServiceModel;

namespace REx.Common.ServiceContracts
{
    /// <summary>
    /// This interface should only contain the methods used by client and service
    /// No Service specific methods like connect and disconnect
    /// </summary>
    [ServiceContract]
    public interface IRemoteExecutorMethods
    {
        [OperationContract]
        void StartProcessAsync(string application, string arguments);

        [OperationContract]
        void KillProcess();

        [OperationContract]
        void WriteToProcess(string value);

        [OperationContract]
        void WriteLineToProcess(string value);
    }
}
