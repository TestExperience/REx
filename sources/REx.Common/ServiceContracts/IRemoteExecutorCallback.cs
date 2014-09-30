using System.ServiceModel;

namespace REx.Common.ServiceContracts
{
    public interface IRemoteExecutorCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnProcessExited(int exitcode);

        [OperationContract(IsOneWay = true)]
        void OnProcessWriteToStdOut(string line);

        [OperationContract(IsOneWay = true)]
        void OnProcessWriteToStdErr(string line);
    }
}
