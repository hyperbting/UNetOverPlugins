using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uniblocks;

public class UNetChunk : Chunk 
{

	public new void Awake () 
	{
        if (UNetChunkLoader.Instance.CreatedBlockLocation != null)
            transform.parent = UNetChunkLoader.Instance.CreatedBlockLocation;

        // chunk initialization (load/generate data, set position, etc.)
		// Set variables
		ChunkIndex = new Index (transform.position);
		SideLength = Engine.ChunkSideLength;
		SquaredSideLength = SideLength * SideLength;
		NeighborChunks = new Chunk [6]; // 0 = up, 1 = down, 2 = right, 3 = left, 4 = forward, 5 = back
		MeshCreator = GetComponent<ChunkMeshCreator>();
		Fresh = true;

		// Register chunk
		ChunkManager.RegisterChunk (this);

		// Clear the voxel data
		VoxelData = new ushort[SideLength*SideLength*SideLength];

		// Set actual position
		transform.position = ChunkIndex.ToVector3() * SideLength;

		// multiply by scale
		transform.position = new Vector3 (transform.position.x * transform.localScale.x, transform.position.y * transform.localScale.y, transform.position.z * transform.localScale.z);

		// Grab voxel data 
        if (Engine.EnableMultiplayer && !UniBlocksUNetCom.Instance.isServer)//if (Engine.EnableMultiplayer && !Network.isServer) {
        {
            //Debug.LogError("Multiplayer Request");
            StartCoroutine(RequestVoxelDataUNet());//	StartCoroutine (RequestVoxelData());	// if multiplayer, get data from server
		}
        else if (Engine.SaveVoxelData && TryLoadVoxelData() == true ) 
        {
			// data is loaded through TryLoadVoxelData()
		}
		else 
		{
			GenerateVoxelData();
		}

	}

	IEnumerator RequestVoxelDataUNet () 
	{ // waits until we're connected to a server and then sends a request for voxel data for this chunk to the server
        
        while (!UniBlocksUNetCom.Instance.isClient)//while (!Network.isClient) 
        {
			Debug.LogError("Not a Client");
			Chunk.CurrentChunkDataRequests = 0; // reset the counter if we're not connected
			yield return new WaitForEndOfFrame();
        }
        
        while (Engine.MaxChunkDataRequests != 0 && Chunk.CurrentChunkDataRequests >= Engine.MaxChunkDataRequests) 
		{
            Debug.LogError("Too Many Chunk Requests");
            yield return new WaitForEndOfFrame();
		}

		Chunk.CurrentChunkDataRequests ++;
		////Engine.UniblocksNetwork.GetComponent<NetworkView>().RPC ("SendVoxelData", RPCMode.Server, Network.player, ChunkIndex.x, ChunkIndex.y, ChunkIndex.z);

		/// client ask for SendVoxelData
        UNetChunkLoader.Instance.CmdSendVoxelData(ChunkIndex.x, ChunkIndex.y, ChunkIndex.z);

	}
}