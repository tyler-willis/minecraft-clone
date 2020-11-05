using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class ChunkSaverSystem
{
    public static void SaveChunk(Chunk chunk)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = "";
    }
}
