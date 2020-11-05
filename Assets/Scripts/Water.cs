using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour
{
    Chunk chunk;
    Mesh mesh;
    int chunkSize;
    int waterLevel;

    void Start()
    {
        chunk = transform.parent.gameObject.GetComponent<Chunk>();
        chunkSize = GameObject.Find("ChunkHandler").GetComponent<ChunkHandler>().chunkSize;
        waterLevel = chunk.waterLevel;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        DrawWater();
    }

    void DrawWater()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int faceCount = 0;

        for (int x = 1; x < chunkSize + 1; x++)
        {
            for (int z = 1; z < chunkSize + 1; z++)
            {
                if (chunk.GetBlock(new Vector3Int(x, waterLevel, z)) == BlockType.Air) {
                    // Draw Water
                    Vector3 offset = new Vector3(x, waterLevel, z);

                    vertices.Add(offset + new Vector3(0, 0.9f, 0));
                    vertices.Add(offset + new Vector3(0, 0.9f, 1));
                    vertices.Add(offset + new Vector3(1, 0.9f, 1));
                    vertices.Add(offset + new Vector3(1, 0.9f, 0));

                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 1));
                    uvs.Add(new Vector2(1, 1));
                    uvs.Add(new Vector2(1, 0));

                    faceCount++;
                }
            }
        }

        for (int i = 0; i < faceCount; i++)
        {
            triangles.AddRange(new int[] { i * 4, i * 4 + 1, i * 4 + 2, i * 4, i * 4 + 2, i * 4 + 3 });
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
    }
}
