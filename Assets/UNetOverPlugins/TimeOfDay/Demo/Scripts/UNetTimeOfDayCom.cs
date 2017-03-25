using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UNetTimeOfDayCom : NetworkBehaviour, ITimeOfDayUNet
{

    // UNet multiplayer
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    //Only Host and Client have this
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //not server , ask for update
        if (!isServer)
            CmdUpdateTOD();
        else
            TimeDisplay.Instance.ControlTime(true);
    }

    /// <summary>
    /// Networking
    /// To network date and time, synchronize the property TOD_Sky.Cycle.Ticks of type long
    /// To network cloud movement, synchronize the property TOD_Sky.Components.Animation.CloudUV of type Vector3
    /// </summary>
    #region
    [Command]
    public void CmdUpdateTOD()
    {
        TargetSendTODUpdate(connectionToClient, TOD_Sky.Instance.Cycle.Ticks);
    }

    [Command]
    public void CmdUpdateTODCloud()
    {
        TargetSendTODCloudUpdate(connectionToClient, TOD_Sky.Instance.Components.Animation.CloudUV);
    }

    [TargetRpc]
    public void TargetSendTODUpdate(NetworkConnection _requester, long _cycleTicks)
    {
        TOD_Sky.Instance.Cycle.Ticks = _cycleTicks;
        TimeDisplay.Instance.ControlTime(true);
    }

    [ClientRpc]
    public void RpcSendTODUpdate(long _cycleTicks)
    {
        TOD_Sky.Instance.Cycle.Ticks = _cycleTicks;
        TimeDisplay.Instance.ControlTime(true);
    }

    [TargetRpc]
    public void TargetSendTODCloudUpdate(NetworkConnection _requester, Vector3 _cloudMovement)
    {
        TOD_Sky.Instance.Components.Animation.CloudUV = _cloudMovement;
    }

    [ClientRpc]
    public void RpcSendTODCloudUpdate(Vector3 _cloudMovement)
    {
        TOD_Sky.Instance.Components.Animation.CloudUV = _cloudMovement;
    }
    #endregion
}
