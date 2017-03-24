public interface IUNet 
{
    bool IsClient();
    bool IsServer();
    bool IsLocalPlayer();
    UnityEngine.Networking.NetworkInstanceId GetNetId();
}
