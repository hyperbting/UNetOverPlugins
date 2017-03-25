public interface ITimeOfDayUNet 
{
    void CmdUpdateTOD();
    void TargetSendTODUpdate(UnityEngine.Networking.NetworkConnection _requester, long _cycleTicks);
    void RpcSendTODUpdate(long _cycleTicks);
}
