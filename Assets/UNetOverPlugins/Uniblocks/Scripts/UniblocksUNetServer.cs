using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Uniblocks;

[RequireComponent(typeof(UniblocksUNetClient))]
public class UniblocksUNetServer : UniblocksServer 
{
	#region extra storage
	//override the private one in base..
	private float autoSaveTimerUNet;

	private Dictionary<NetworkInstanceId, Index> PlayerPositions; // stores the index of each player's origin chunk. Changes will only be sent if the change is within their radius
	private Dictionary<NetworkInstanceId, int> PlayerChunkSpawnDistances; // chunk spawn distance for each player
	#endregion
    public UniBlocksUNetCom myServerCom;
	public UniblocksUNetClient myClient;

	public static UniblocksUNetServer Instance;
	void Awake () 
	{
		Instance = this;

		if (Engine.EnableMultiplayer == false) 	
			Debug.LogWarning ("Uniblocks: Multiplayer is disabled. Unexpected behavior may occur.");

		Engine.UniblocksNetwork = this.gameObject;

		ResetPlayerData();
	}

	void ResetPlayerData () 
	{
		PlayerPositions = new Dictionary<NetworkInstanceId,Index>();// reset/initialize player origins
		PlayerChunkSpawnDistances = new Dictionary<NetworkInstanceId,int>(); // reset/initialize player chunk spawn distances
	}

	public void UpdatePlayerPosition ( NetworkInstanceId player, int chunkx, int chunky, int chunkz ) 
	{
		PlayerPositions [player] = new Index (chunkx, chunky, chunkz);
		if (EnableDebugLog) Debug.Log 
			("UniblocksServer: Updated player position. Player "+player.ToString() + ", position: " + new Vector3(chunkx,chunky,chunkz).ToString());
	}

	public void UpdatePlayerRange ( NetworkInstanceId player, int range ) 
	{
		PlayerChunkSpawnDistances [player] = range;
		if (EnableDebugLog) 
			Debug.Log ("UniblocksServer: Updated player range. Player: "+player.ToString() + ", range: " + range.ToString());
	}

	public void ServerChangeBlock ( NetworkInstanceId sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data ) 
	{
		if (EnableDebugLog) 
			Debug.Log ("UniblocksServer: Received ChangeBlock from player " + sender.ToString());

		DistributeChange (sender, x,y,z, chunkx, chunky,chunkz, data, true);
	}

	public void ServerPlaceBlock ( NetworkInstanceId sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data ) 
	{
		if (!EnableDebugLog)
			Debug.Log ("UniblocksServer: Received PlaceBlock from player " + sender.ToString ());

		DistributeChange (sender, x,y,z, chunkx, chunky,chunkz, data, false);
	}

	//tell everyone that things changed
	void DistributeChange ( NetworkInstanceId sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock ) 
	{ // sends a change in the voxel data to all clients

		// update server
		ApplyOnServer (x,y,z, chunkx, chunky, chunkz, data, isChangeBlock);

		//// send to every client with IN RANGE
        if (Engine.MultiplayerTrackPosition == false || IsWithinRange(sender, new Index(chunkx, chunky, chunkz)))//if (Engine.MultiplayerTrackPosition == false || IsWithinRange(myClient.UNetCom.GetNetId(), new Index(chunkx, chunky, chunkz)))
        { // check if the change is within range of each player

			if (EnableDebugLog)  
				Debug.Log ("UniblocksServer: Sending voxel change to player " + sender.ToString());

			//RpcReceivePlaceBlock with isChangeBlock true == RpcReceiveChangeBlock
			myServerCom.RpcReceivePlaceBlock (sender, x,y,z, chunkx, chunky,chunkz, data, isChangeBlock);
		}
	}

	void ApplyOnServer ( int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock ) 
	{ // updates the voxel data stored on the server with the change sent by client

		Chunk chunk = ChunkManager.SpawnChunkFromServer (chunkx,chunky,chunkz).GetComponent<Chunk>();// if chunk is not loaded, load it

		chunk.Lifetime = 0f; // refresh the chunk's lifetime

		//if (isChangeBlock) {
		//	GetComponent<UniblocksClient>().ReceiveChangeBlock (Network.player, x,y,z, chunkx,chunky,chunkz, data);
		//}
		//else {
		//	GetComponent<UniblocksClient>().ReceivePlaceBlock (Network.player, x,y,z, chunkx,chunky,chunkz, data);
		//}
        myClient.ReceivePlaceBlock(UniBlocksUNetCom.Instance.netId, x, y, z, chunkx, chunky, chunkz, data, isChangeBlock);
	}

	// ===== send chunk data

	/// <summary>
	/// Sends the voxel data to specific player.
	/// Cannot do Rpc/Command here
	/// tell the one with NetworkBehaviour to do thing for me...
	/// </summary>
	public void SendVoxelData ( NetworkConnection target, int chunkx, int chunky, int chunkz ) 
	{
		// >> You can check whether the request for voxel data is valid here <<
		Chunk chunk = ChunkManager.SpawnChunkFromServer (chunkx,chunky,chunkz).GetComponent<Chunk>(); // get the chunk (spawn it if it's not spawned already)

		chunk.Lifetime = 0f; // refresh the chunk's lifetime

		string data = ChunkDataFiles.CompressData (chunk); // get data from the chunk and compress it

		byte[] dataBytes = GetBytes (data); // convert to byte array (sending strings over RPC doesn't work too well)

		////GetComponent<NetworkView>().RPC ("ReceiveVoxelData", player, chunkx, chunky, chunkz, dataBytes); // send compressed data to the player who requested it

		// send compressed data to the target player who requested it
		myServerCom.TargetReceiveVoxelData( target, chunkx, chunky, chunkz, dataBytes );
	}

	// ===== save data

	void Update () 
	{

		if (AutosaveTime > 0.0001f) 
		{
			if (autoSaveTimerUNet < 0) 
			{
				autoSaveTimerUNet = AutosaveTime;
				Engine.SaveWorld();
			}
			else 
			{
				autoSaveTimerUNet -= Time.deltaTime;
			}
		}
	}

	#region utility
	bool IsWithinRange ( NetworkInstanceId player, Index chunkIndex ) 
	{ // checks if the player is within the range of the chunk

		if ( Mathf.Abs ( PlayerPositions [player].x - chunkIndex.x ) > PlayerChunkSpawnDistances[player] ||
			Mathf.Abs ( PlayerPositions [player].y - chunkIndex.y ) > PlayerChunkSpawnDistances[player] ||
			Mathf.Abs ( PlayerPositions [player].z - chunkIndex.z ) > PlayerChunkSpawnDistances[player] )
		{
			return false;
		}

		return true;
	}

	// convert string to byte array
	public static byte[] GetBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}

	// convert back to string
	public static string GetString(byte[] bytes)
	{
		char[] chars = new char[bytes.Length / sizeof(char)];
		System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		return new string(chars);
	}
	#endregion
}