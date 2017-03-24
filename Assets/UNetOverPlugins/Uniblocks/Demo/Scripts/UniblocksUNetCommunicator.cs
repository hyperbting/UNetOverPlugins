using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UniblocksUNetCommunicator : NetworkBehaviour, IUniblockUnetServer 
{
	public UniblocksUNetServer myServer;
	public UniblocksUNetClient myClient;

	void Awake ()
	{
		myClient = GetComponent<UniblocksUNetClient> ();
		myServer = GetComponent<UniblocksUNetServer> ();

		//myClient.myUniblockCom = this;
		myServer.myUniblockCom = this;
	}

	public override void OnStartClient()
	{
		base.OnStartClient ();

		Debug.Log ("NetworkUniblockCom: Connected to server.");

		if(!isServer)
		{
			Uniblocks.Engine.SaveVoxelData = false; // disable local saving for pure client(that is not with server)
		}
	}

	#region (Server) to Client RPC
	[TargetRpc]
	public void TargetReceiveVoxelData ( NetworkConnection target, int chunkx, int chunky, int chunkz, byte[] data )
	{
		//Debug.LogError("TargetReceiveVoxelData");
		//only target player do this
		myClient.ReceiveVoxelData( chunkx, chunky, chunkz, data );
	}

	[ClientRpc]
	public void RpcReceivePlaceBlock ( NetworkInstanceId sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock ) 
	{	// receives a change sent by server
		//Debug.LogError("RpcReceivePlaceBlock");
		//every player do this
		myClient.ReceivePlaceBlock (sender, x, y, z, chunkx, chunky, chunkz, data, isChangeBlock);

	}
	#endregion Rpc to Client from Server

	//based on the "command only on local player object" limitation. 
	//Every [Command] here have to move under player object
	#region Command to Server
	/*
	[Command]
	public void CmdSendVoxelData (int chunkx, int chunky, int chunkz ) 
	{
		if (!isServer)
			return;
		
		//[Server]sender "target" tells me to SendVoxelData to him
		//This is only valid for PLAYER OBJECTS on the server.
		myServer.SendVoxelData (connectionToClient, chunkx, chunky, chunkz);
	}
		
	[Command]
	public void CmdSendPlaceBlock(int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
	{
		if (!isServer)
			return;
		
		//[Server]sender tells me to ServerPlaceBlock
		myServer.ServerPlaceBlock(netId, x, y, z, chunkx, chunky, chunkz, data);
	}
	[Command]
	public void CmdSendChangeBlock(int x, int y, int z, int chunkx, int chunky, int chunkz, int data)
	{
		if (!isServer)
			return;
		//[Server]sender tells me to ServerChangeBlock
		myServer.ServerChangeBlock(netId, x, y, z, chunkx, chunky, chunkz, data);
	}
	[Command]
	public void CmdUpdatePlayerPosition(int _x, int _y, int _z)
	{
		if (!isServer)
			return;
		
		//[Server]sender tells to change specific player position
		myServer.UpdatePlayerPosition(netId, _x, _y, _z);
	}
	[Command]
	public void CmdUpdatePlayerRange (int _range ) 
	{ // sent by client
		
		if (!isServer)
			return;
		
		//[Server]Someone tells to change specific player Range
		myServer.UpdatePlayerRange ( netId, _range );
	}
	*/
	#endregion end Commands 

	public NetworkInstanceId GetNetworkInstanceID()
	{
		return netId;
	}
}
