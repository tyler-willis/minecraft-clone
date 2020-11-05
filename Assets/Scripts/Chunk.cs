using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    public Vector2Int chunkCoord;
    BlockType[,,] blockData;
    Mesh mesh;
    int chunkSize;
    int chunkHeight;
    public int waterLevel;
    Noise mainNoise;
    // public float noiseMult = 0.01f;
    // public float noiseHeightMult = 50f;
    // public float caveNoiseMult;
    public float caveThreshhold;
    public int caveHeight;
    int seed;

    // Start is called before the first frame update
    void Start()
    {
        chunkSize = GameObject.Find("ChunkHandler").GetComponent<ChunkHandler>().chunkSize;
        chunkHeight = GameObject.Find("ChunkHandler").GetComponent<ChunkHandler>().chunkHeight;
        seed = GameObject.Find("ChunkHandler").GetComponent<ChunkHandler>().seed;

        chunkCoord = new Vector2Int((int)this.transform.position.x, (int)this.transform.position.z) / chunkSize;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        GenerateBlockData();
        DrawLand();
    }

    void GenerateBlockData()
    {
        if (blockData == null)
        {
            blockData = new BlockType[chunkSize + 2, chunkHeight + 2, chunkSize + 2];
        }

        mainNoise = new Noise(seed);
        mainNoise.addChannel(new Channel("Height", Algorithm.Simplex2d, 250.0f, NoiseStyle.Linear, 35f, 120f, Edge.Smooth).setFractal(4, 2.4f, 0.9f));
        mainNoise.addChannel(new Channel("Biome", Algorithm.Simplex2d, 300.0f, NoiseStyle.Linear, 1.0f, 1.8f, Edge.Smooth));
        mainNoise.addChannel(new Channel("Mountain", Algorithm.Simplex2d, 30.0f, NoiseStyle.Linear, 1f, 50f, Edge.Smooth));
        mainNoise.addChannel(new Channel("Cave", Algorithm.Simplex3d, 10.0f, NoiseStyle.Linear, 5f, 50f, Edge.Smooth).setFractal(2, 1.5f, 0.25f));
        mainNoise.addChannel(new Channel("Coal", Algorithm.Simplex3d, 10.0f, NoiseStyle.Linear, 0f, 100f, Edge.Smooth));
        mainNoise.addChannel(new Channel("Iron", Algorithm.Simplex3d, 8.0f, NoiseStyle.Linear, 0f, 50f, Edge.Smooth));
        mainNoise.addChannel(new Channel("Gold", Algorithm.Simplex3d, 12.0f, NoiseStyle.Linear, 0f, 30f, Edge.Smooth));
        mainNoise.addChannel(new Channel("Diamond", Algorithm.Simplex3d, 10.0f, NoiseStyle.Linear, 0f, 10f, Edge.Smooth));


        // Basic terrain
        for (int x = 1; x < chunkSize + 1; x++)
        {
            for (int y = 1; y < chunkSize + 1; y++)
            {
                BuildLand(x, y);

                for (int z = 0; z < chunkHeight; z++)
                {
                    if (z < 3)
                    {
                        // Make Bedrock
                        blockData[x, z, y] = BlockType.Bedrock;
                    }
                    else if (z < caveHeight)
                    {
                        CutCaves(x, y, z);
                    }

                    // blockData[x, z, y] = BlockType.Stone;
                    PlaceOres(x, y, z);
                }
            }
        }

        PlantTrees();
    }

    void BuildLand(int x, int y)
    {
        Vector3 chunkOffset = new Vector3(x + (chunkCoord.x * chunkSize), 0, y + (chunkCoord.y * chunkSize));

        //float z = (mainNoise.getNoise(chunkOffset, "Height") + mainNoise.getNoise(chunkOffset, "Mountain")) / 2;
        float z = (mainNoise.getNoise(chunkOffset, "Height")); // * (mainNoise.getNoise(chunkOffset, "Biome"));

        //if (mainNoise.getNoise(chunkOffset, "Biome") > 70f)
        //{
        //    // Make Terrain Mountainy
        //    z = (z + mainNoise.getNoise(chunkOffset, "Mountain")) / 2;
        //}

        // Make the top block grass and the rest dirt
        if (z > chunkHeight)
        {
            z = chunkHeight - 1;
            Debug.Log("Tried to create a block taller than the map");
        }

        blockData[x, (int)z, y] = BlockType.Grass;
        z--;

        int zi = (int)z;
        while (zi >= 0)
        {
            if (z - zi < 4)
                blockData[x, (int)zi, y] = BlockType.Dirt;
            else
                blockData[x, (int)zi, y] = BlockType.Stone;

            zi--;
        }
        
    }

    void CutCaves(int x, int y, int z)
    {
        Vector3 chunkOffset = new Vector3(x + (chunkCoord.x * chunkSize), z, y + (chunkCoord.y * chunkSize));

        if (mainNoise.getNoise(chunkOffset, "Cave") < caveThreshhold)
        {
            blockData[x, z, y] = BlockType.Air;
        }
    }

    void PlaceOres(int x, int y, int z)
    {
        // ignore blocks that aren't stone
        if (blockData[x, z, y] != BlockType.Stone)
            return;

        Vector3 chunkOffset = new Vector3(x + (chunkCoord.x * chunkSize), z, y + (chunkCoord.y * chunkSize));
        float coalThreshhold = 16;
        float ironThreshhold = 8;
        float goldThreshhold = 3.5f;
        float diamondThreshhold = 1f;

        // Coal
        if (mainNoise.getNoise(chunkOffset, "Coal") < coalThreshhold)
        {
            blockData[x, z, y] = BlockType.Coal;
        }

        // Iron
        if (mainNoise.getNoise(chunkOffset, "Iron") < ironThreshhold)
        {
            blockData[x, z, y] = BlockType.Iron;
        }

        // Gold
        if (z < 30 && mainNoise.getNoise(chunkOffset, "Gold") < goldThreshhold)
        {
            blockData[x, z, y] = BlockType.Gold;
        }

        // Diamond
        if (z < 10 && mainNoise.getNoise(chunkOffset, "Diamond") < diamondThreshhold)
        {
            blockData[x, z, y] = BlockType.Diamond;
        }
    }

    void PlantTrees()
    {
        for (int x = 3; x < chunkSize - 2; x++)
        {
            for (int z = 3; z < chunkSize - 2; z++)
            {
                if (Random.Range(0, 100) < 2)
                {
                    int topBlock = chunkHeight;

                    while (blockData[x, topBlock, z] == BlockType.Air)
                        topBlock--;

                    // Make sure trees don't grow under the water level
                    if (topBlock < waterLevel || blockData[x, topBlock, z] != BlockType.Grass)
                        continue;

                    TreeType newTree = (TreeType)Random.Range(0, 2);
                    PlantTree(new Vector3Int(x, topBlock, z), newTree);
                }
            }
        }
    }

    void PlantTree(Vector3Int coord, TreeType type)
    {
        int height = Random.Range(5, 8);

        // Make Trunk
        for (int i = 1; i <= height; i++)
        {
            if (coord.y + i < chunkHeight)
            {
                if (type == TreeType.Oak)
                    blockData[coord.x, coord.y + i, coord.z] = BlockType.OakTrunk;
                else if (type == TreeType.Birch)
                    blockData[coord.x, coord.y + i, coord.z] = BlockType.BirchTrunk;
            }
        }

        // Make leaves
        for (int x = coord.x - 2; x <= coord.x + 2; x++)
        {
            for (int z = coord.z - 2; z <= coord.z + 2; z++)
            {
                for (int y = coord.y + height - 2; y <= coord.y + height + 2; y++)
                {
                    if (x == coord.x - 2 && z == coord.z - 2 ||
                        x == coord.x + 2 && z == coord.z + 2 ||
                        x == coord.x - 2 && z == coord.z + 2 ||
                        x == coord.x + 2 && z == coord.z - 2 ||
                        x == coord.x + 2 && y == coord.y + height - 2 ||
                        x == coord.x + 2 && y == coord.y + height + 2 ||
                        x == coord.x - 2 && y == coord.y + height - 2 ||
                        x == coord.x - 2 && y == coord.y + height + 2 ||
                        z == coord.z + 2 && y == coord.y + height - 2 ||
                        z == coord.z + 2 && y == coord.y + height + 2 ||
                        z == coord.z - 2 && y == coord.y + height - 2 ||
                        z == coord.z - 2 && y == coord.y + height + 2)
                        continue;

                    if (x < 1 || y < 1 || z < 1 || x > chunkSize || y > chunkHeight || z > chunkSize)
                        continue;

                    if (blockData[x, y, z] == BlockType.Air)
                    {
                        if (type == TreeType.Oak)
                            blockData[x, y, z] = BlockType.OakLeaves;
                        if (type == TreeType.Birch)
                            blockData[x, y, z] = BlockType.BirchLeaves;
                    }
                }
            }
        }
    }

    void DrawLand()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int faceCount = 0;

        for (int x = 1; x < chunkSize + 1; x++)
        {
            for (int y = 1; y < chunkHeight + 1; y++)
            {
                for (int z = 1; z < chunkSize + 1; z++)
                {
                    if (blockData[x, y, z] != 0)
                    {
                        Vector3 blockPos = new Vector3(x, y, z);
                        Vector3 chunkPos = new Vector3(chunkCoord.x, 0, chunkCoord.y);
                        Vector3 offset = blockPos;
                    
                        // Top Face
                        if (y < chunkHeight && blockData[x, y + 1, z] == 0)
                        {
                            // Set Vertices
                            vertices.Add(offset + new Vector3(0, 1, 0));
                            vertices.Add(offset + new Vector3(0, 1, 1));
                            vertices.Add(offset + new Vector3(1, 1, 1));
                            vertices.Add(offset + new Vector3(1, 1, 0));

                            // Set UVs
                            uvs.AddRange(TextureMap.GetUvs(blockData[x, y, z], Face.Top));

                            faceCount++;
                        }

                        // Bottom Face
                        if (y > 0 && blockData[x, y - 1, z] == 0)
                        {
                            vertices.Add(offset + new Vector3(0, 0, 0));
                            vertices.Add(offset + new Vector3(1, 0, 0));
                            vertices.Add(offset + new Vector3(1, 0, 1));
                            vertices.Add(offset + new Vector3(0, 0, 1));

                            // Set UVs
                            uvs.AddRange(TextureMap.GetUvs(blockData[x, y, z], Face.Bottom));

                            faceCount++;
                        }

                        // North Face
                        if (x < chunkSize+1 && blockData[x + 1, y, z] == 0)
                        {
                            vertices.Add(offset + new Vector3(1, 0, 0));
                            vertices.Add(offset + new Vector3(1, 1, 0));
                            vertices.Add(offset + new Vector3(1, 1, 1));
                            vertices.Add(offset + new Vector3(1, 0, 1));

                            // Set UVs
                            uvs.AddRange(TextureMap.GetUvs(blockData[x, y, z], Face.North));

                            faceCount++;
                        }

                        // South Face
                        if (x > 0 && blockData[x - 1, y, z] == 0)
                        {
                            vertices.Add(offset + new Vector3(0, 0, 1));
                            vertices.Add(offset + new Vector3(0, 1, 1));
                            vertices.Add(offset + new Vector3(0, 1, 0));
                            vertices.Add(offset + new Vector3(0, 0, 0));

                            // Set UVs
                            uvs.AddRange(TextureMap.GetUvs(blockData[x, y, z], Face.South));

                            faceCount++;
                        }

                        // East Face
                        if (z < chunkSize+1 && blockData[x, y, z + 1] == 0)
                        {
                            vertices.Add(offset + new Vector3(1, 0, 1));
                            vertices.Add(offset + new Vector3(1, 1, 1));
                            vertices.Add(offset + new Vector3(0, 1, 1));
                            vertices.Add(offset + new Vector3(0, 0, 1));

                            // Set UVs
                            uvs.AddRange(TextureMap.GetUvs(blockData[x, y, z], Face.East));

                            faceCount++;
                        }

                        // West Face
                        if (z > 0 && blockData[x, y, z - 1] == 0)
                        {
                            vertices.Add(offset + new Vector3(0, 0, 0));
                            vertices.Add(offset + new Vector3(0, 1, 0));
                            vertices.Add(offset + new Vector3(1, 1, 0));
                            vertices.Add(offset + new Vector3(1, 0, 0));

                            // Set UVs
                            uvs.AddRange(TextureMap.GetUvs(blockData[x, y, z], Face.West));

                            faceCount++;
                        }
                    }
                }
            }
        }

        // Ploygon draw sequence for squares --> 0, 1, 2, 0, 2, 3
        for (int i = 0; i < faceCount; i++)
        {
            triangles.AddRange(new int[] { i*4, i*4 + 1, i*4 + 2, i*4, i*4 + 2, i*4 + 3 });
        }

        if (vertices.Count / 4 != triangles.Count / 6)
        {
            Debug.LogError("ERROR: The vertices and triangles aren't the same size in chunk {chunkCoord}");
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void SetChunkCoord(Vector2Int coord)
    {
        this.chunkCoord = coord;
    }

    public void RemoveBlock(Vector3Int coord)
    {
        if (blockData[coord.x, coord.y, coord.z] == BlockType.Air)
        {
            Debug.Log("ERROR: Tried to delete a block that doesn't exist");
            return;
        }

        blockData[coord.x, coord.y, coord.z] = BlockType.Air;
        DrawLand();
    }

    public void AddBlock(Vector3Int coord, BlockType blockType)
    {
        blockData[coord.x, coord.y, coord.z] = blockType;
        DrawLand();
    }

    public BlockType GetBlock(Vector3Int coord)
    {
        return blockData[coord.x, coord.y, coord.z];
    }

    public void SetBlock(Vector3Int coord, BlockType blockType)
    {
        blockData[coord.x, coord.y, coord.z] = blockType;
    }

    public BlockType[,,] GetBlockData()
    {
        return blockData;
    }
}

