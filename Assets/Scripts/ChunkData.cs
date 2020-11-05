using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    Vector2Int chunkCoord;
    BlockType[,,] blockData;

    public ChunkData(Chunk chunk)
    {
        this.blockData = chunk.GetBlockData();
        this.chunkCoord = chunk.chunkCoord;
    }
}
