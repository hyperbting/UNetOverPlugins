using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Uniblocks;

//this will be NetworkServer.Spawn() can do [Command] itself
public class UniblocksUNetClient : UniblocksClient 
{
    public static UniblocksUNetClient Instance;
	public virtual void Awake()
	{
		Instance = this;
	}

	// ===== network communication ============
	public void UpdatePlayerPositionUNet (int x, int y, int z)
    {
        UNetChunkLoader.Instance.CmdUpdatePlayerPosition(x, y, z); ////Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("UpdatePlayerPosition", RPCMode.Server, Network.player, x, y, z);
	}

	public void UpdatePlayerPositionUNet (Index index) 
	{
        UpdatePlayerPositionUNet(index.x, index.y, index.z); ////Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("UpdatePlayerPosition", RPCMode.Server, Network.player, index.x, index.y, index.z);
	}

	public void UpdatePlayerRangeUNet (int range) 
	{
        UNetChunkLoader.Instance.CmdUpdatePlayerRange(range); ////Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("UpdatePlayerRange", RPCMode.Server, Network.player, range);
	}

	/* To send info local player is required... */
	public override void SendPlaceBlock ( VoxelInfo info, ushort data ) 
	{	// sends a voxel change to the server, which then redistributes it to other clients
		// convert to ints
		int chunkx = info.chunk.ChunkIndex.x;
		int chunky = info.chunk.ChunkIndex.y;
		int chunkz = info.chunk.ChunkIndex.z;

		//// send to server
		//if (Network.isServer) {
		//	GetComponent<UniblocksServer>().ServerPlaceBlock (Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		//}
		//else {
		//	GetComponent<NetworkView>().RPC ("ServerPlaceBlock", RPCMode.Server, Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		//}
        UNetChunkLoader.Instance.CmdSendPlaceBlock(info.index.x, info.index.y, info.index.z, chunkx, chunky, chunkz, (int)data);
    }

	public override void SendChangeBlock(VoxelInfo info, ushort data)
	{
		// convert to ints
		int chunkx = info.chunk.ChunkIndex.x;
		int chunky = info.chunk.ChunkIndex.y;
		int chunkz = info.chunk.ChunkIndex.z;

		//// send to server
		//if (Network.isServer) {
		//	GetComponent<UniblocksServer>().ServerChangeBlock (Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		//}
		//else {
		//	GetComponent<NetworkView>().RPC ("ServerChangeBlock", RPCMode.Server, Network.player, info.index.x, info.index.y, info.index.z, chunkx,chunky,chunkz, (int)data);
		//}

		//// send to server
        UNetChunkLoader.Instance.CmdSendChangeBlock(info.index.x, info.index.y, info.index.z, chunkx, chunky, chunkz, (int)data);
	}

	public virtual void ReceivePlaceBlock(NetworkInstanceId sender, int x, int y, int z, int chunkx, int chunky, int chunkz, int data, bool isChangeBlock=false)
	{

		GameObject chunkObject = ChunkManager.GetChunk (chunkx,chunky,chunkz);

		if (chunkObject == null)
			return;

		// convert back to VoxelInfo
		Index voxelIndex = new Index (x,y,z);
		VoxelInfo info = new VoxelInfo (voxelIndex, chunkObject.GetComponent<Chunk>());

		if (!isChangeBlock && data == 0) 
		{
            //data == 0 && is Place Block
            this.DestroyBlockMultiplayer(info);	////Voxel.DestroyBlockMultiplayer (info, sender);
			return;
		} 
		else 
		{
			//isChangeBlock && (is Place Block, data != 0 )
            this.PlaceBlockMultiplayer(info, (ushort)data); ////Voxel.PlaceBlockMultiplayer (info, (ushort)data, sender);
		}
	}

	public new void ReceiveVoxelData ( int chunkx, int chunky, int chunkz, byte[] data ) 
	{
		GameObject chunkObject = ChunkManager.GetChunk (chunkx,chunky,chunkz); // find the chunk

		if (chunkObject == null) 	
			return; // abort if chunk isn't spawned anymore

		Chunk chunk = chunkObject.GetComponent<Chunk>();

		ChunkDataFiles.DecompressData (chunk, UniblocksUNetServer.GetString(data)); // decompress data

		//		ChunkManager.DataReceivedCount ++; // let ChunkManager know that we have received the data
		chunk.VoxelsDone = true; // let Chunk know that it can update it's mesh

		Chunk.CurrentChunkDataRequests --;
	}

	#region direct copy from Voxel .PlaceBlockMultiplayer .DestroyBlockMultiplayer .ChangeBlockMultiplayer
	void PlaceBlockMultiplayer ( VoxelInfo voxelInfo, ushort data ) 
	{ // received from server, don't use directly

		voxelInfo.chunk.SetVoxel (voxelInfo.index, data, true);

		GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (data) ) as GameObject;

		VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();

		if (events != null) 
		{
			events.OnBlockPlace(voxelInfo);
		}
		Destroy (voxelObject);
	}

	void DestroyBlockMultiplayer ( VoxelInfo voxelInfo) 
	{
		GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (voxelInfo.GetVoxel()) ) as GameObject;

		VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
		if (events != null) 
		{
			events.OnBlockDestroy(voxelInfo);
		}
		voxelInfo.chunk.SetVoxel (voxelInfo.index, 0, true);
		Destroy(voxelObject);
	}

	void ChangeBlockMultiplayer ( VoxelInfo voxelInfo, ushort data) 
	{ // received from server, don't use directly

		voxelInfo.chunk.SetVoxel (voxelInfo.index, data, true);

		GameObject voxelObject = Instantiate ( Engine.GetVoxelGameObject (data) ) as GameObject;

		VoxelEvents events = voxelObject.GetComponent<VoxelEvents>();
		if (events != null) 
		{
			events.OnBlockChange(voxelInfo);
			//events.OnBlockChangeMultiplayer(voxelInfo, sender);
		}
		Destroy (voxelObject);
	}
	#endregion
}