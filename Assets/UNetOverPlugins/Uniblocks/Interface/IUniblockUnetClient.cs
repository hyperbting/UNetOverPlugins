public interface IUniblockUnetClient  : IUniblockUNet
{
	void CmdUpdatePlayerPosition (int _x, int _y, int _z);
	void CmdUpdatePlayerRange (int _range);
	void CmdSendPlaceBlock(int _indexX, int _indexY, int _indexZ,int _chunkIndexX, int _chunkIndexY, int _chunkIndexZ, int _data);
	void CmdSendChangeBlock(int _indexX, int _indexY, int _indexZ,int _chunkIndexX, int _chunkIndexY, int _chunkIndexZ, int _data);

}
