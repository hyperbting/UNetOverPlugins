using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNetOverUniblocks
using Uniblocks;

/// <summary>
/// UNet chunk loader.
/// copy from Uniblocks ChunkLoader.cs with adaption for UNet
/// this script have to be attached to PLAYER!
/// the voxel world have to be load from the LocalPlayer not Server!
/// </summary>
public class UNetChunkLoader : NetworkBehaviour, IUniblockUnetClient
{
    public bool EnableDebugLog;
    public string CreatedBlockLocationName = "UniBlocks";
    public Transform CreatedBlockLocation;
    public GameObject UniblocksNetworkPrefab;

    public List<Behaviour> LocalPlayerItems;

    private Index LastPos;
    private Index currentPos;

    public static UNetChunkLoader Instance;
    public void Awake()
    {
        GameObject targetPlace = GameObject.Find(CreatedBlockLocationName);

        if (!targetPlace)
        {
            targetPlace = new GameObject(CreatedBlockLocationName);
        }

        CreatedBlockLocation = targetPlace.transform;
    }

    public void Update()
    {
        // don't load chunks if engine isn't initialized yet
        if (!Engine.Initialized || !ChunkManager.Initialized)
        {
            return;
        }

        // don't load chunks if multiplayer is enabled but the connection isn't established yet
        if (!isClient && !isServer)//if (Engine.EnableMultiplayer && !Network.isClient && !Network.isServer) 
        {
            return;
        }

        if (!hasAuthority)
        {
            return;
        }

        // track which chunk we're currently in. If it's different from previous frame, spawn chunks at current position.
        currentPos = Engine.PositionToChunkIndex(transform.position);

        if (currentPos.IsEqual(LastPos) == false)
        {
            ChunkManager.SpawnChunks(currentPos.x, currentPos.y, currentPos.z);

            // (Multiplayer) update server position
            //if (Engine.EnableMultiplayer && Engine.MultiplayerTrackPosition && Engine.UniblocksNetwork != null) 
            //{
            //	UniblocksClient.UpdatePlayerPosition (currentPos);
            //}
            if (Engine.MultiplayerTrackPosition && Engine.UniblocksNetwork != null && isClient)
            {
                UniblocksUNetClient.Instance.UpdatePlayerPositionUNet(currentPos);//UniblocksUNetClient.UpdatePlayerPosition (currentPos);
                if (EnableDebugLog) Debug.Log("Updating Player Position UNet");
            }
        }

        LastPos = currentPos;
    }

    // UNet multiplayer
    public override void OnStartServer()
    {
        base.OnStartServer();
        if (EnableDebugLog) Debug.LogWarning(isServer + " " + isClient + " " + isLocalPlayer);//T F F !! for Server!!!
    }

    //only Server and Host(Server+Client) have this
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (EnableDebugLog) Debug.LogWarning(isServer + " " + isClient + " " + isLocalPlayer);//T T F !! for Server!!!
        
    }

    //Only Host and Client have this
    public override void OnStartLocalPlayer()
    {
        //now I know this is my local loader
        Instance = this;

        base.OnStartLocalPlayer();
        if (EnableDebugLog) Debug.LogWarning(isServer + " " + isClient + " " + isLocalPlayer);//T T T !! for Server!!!

        //Specific item for local player to use
        if (LocalPlayerItems != null)
            for (int i = 0; i < LocalPlayerItems.Count; i++)
                LocalPlayerItems[i].enabled = true;

        //Network Instantiate UniblocksNetworkPrefab for communication
        if (Engine.EnableMultiplayer && isServer)
        {
            if (EnableDebugLog) Debug.Log("Initial UniblockUNet Service");
            StartUniblockUNet();
        }

        //track Client player position if required
        if (Engine.MultiplayerTrackPosition)
        {
            StartCoroutine(InitialPositionAndRangeUpdate());
            if (EnableDebugLog) Debug.Log("InitialPositionAndRangeUpdate");
        }

        //I can be LocalPlayer or normal Client
    }

    void StartUniblockUNet()
    {
        if (EnableDebugLog) Debug.Log("Initial UniblocksNetwork");
        var go = Instantiate(UniblocksNetworkPrefab);
        NetworkServer.Spawn(go);
    }

    IEnumerator InitialPositionAndRangeUpdate()
    {
        while (Engine.UniblocksNetwork == null || currentPos == null)
        {
            yield return new WaitForEndOfFrame();
        }

        //UniblocksUNetClient.UpdatePlayerPosition (currentPos);
        //UniblocksUNetClient.UpdatePlayerRange (Engine.ChunkSpawnDistance);
        if (EnableDebugLog) Debug.Log("UpdatePlayerPositionUNet");

        UniblocksUNetClient.Instance.UpdatePlayerPositionUNet(currentPos);

        if (EnableDebugLog) Debug.Log("UpdatePlayerRangeUNet");

        UniblocksUNetClient.Instance.UpdatePlayerRangeUNet(Engine.ChunkSpawnDistance);
    }

    //based on the "command only on local player object" limitation. This script has to be under player object
    #region Command to Server
	[Command]
	public void CmdSendVoxelData (int chunkx, int chunky, int chunkz ) 
	{
		if (!isServer)
			return;
		
		//[Server]sender "target" tells me to SendVoxelData to him
        if (EnableDebugLog) Debug.LogFormat("Asking for {0} {1} {2} Data", chunkx, chunky, chunkz);
        UniblocksUNetServer.Instance.SendVoxelData(connectionToClient, chunkx, chunky, chunkz);
	}
		
	[Command]
	public void CmdSendPlaceBlock(int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
	{
		if (!isServer)
			return;
		
		//[Server]sender tells me to ServerPlaceBlock
        UniblocksUNetServer.Instance.ServerPlaceBlock(netId, x, y, z, chunkx, chunky, chunkz, data);
	}

	[Command]
	public void CmdSendChangeBlock(int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
	{
		if (!isServer)
			return;

		//[Server]sender tells me to ServerChangeBlock
        UniblocksUNetServer.Instance.ServerChangeBlock(netId, x, y, z, chunkx, chunky, chunkz, data);
	}

	[Command]
	public void CmdUpdatePlayerPosition(int _x, int _y, int _z)
	{
		if (!isServer)
			return;
		
		//[Server]sender tells to change specific player position
        UniblocksUNetServer.Instance.UpdatePlayerPosition(netId, _x, _y, _z);
	}

	[Command]
	public void CmdUpdatePlayerRange (int _range ) 
	{ // sent by client
		
        //Server can do this its own
		if (!isServer)
			return;
		
		//[Server]Someone tells to change specific player Range
        UniblocksUNetServer.Instance.UpdatePlayerRange(netId, _range);
    }
    #endregion
}
#endif