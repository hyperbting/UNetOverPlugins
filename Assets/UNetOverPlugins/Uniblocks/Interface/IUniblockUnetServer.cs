using UnityEngine.Networking;
public interface IUniblockUnetServer
{
	void RpcReceivePlaceBlock(NetworkInstanceId _sender, int _x, int _y, int _z, int _chunkx, int _chunky, int _chunkz, int _data, bool _isChangeBlock );
	void TargetReceiveVoxelData (NetworkConnection _target, int _chunkx, int _chunky, int _chunkz, byte[] _databyte);
}
