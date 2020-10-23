using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    public GameObject chunk;
    public int chunkSize = 50;
    public int chunkHeight = 100;
    public int drawDistance = 3;
    public int seed;
    Dictionary<Vector2Int, GameObject> chunks;

    private Vector2Int playerCoord;

    // Start is called before the first frame update
    void Start()
    {
        seed = (int)Random.Range(-2000000000f, 2000000000f);
        Random.InitState(seed);
        playerCoord = new Vector2Int(0, 0);

        chunks = new Dictionary<Vector2Int, GameObject>();
        // Add initial chunk to diction

        // Create initial landscape around player
        BuildNewChunks();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCoord != GameObject.Find("Player").GetComponent<Player>().currentChunk)
        {
            playerCoord = GameObject.Find("Player").GetComponent<Player>().currentChunk;
            BuildNewChunks();
            EraseOldChunks();
        }
    }

    void BuildNewChunks()
    {
        for (int x = playerCoord.x - drawDistance; x < playerCoord.x + drawDistance; x++)
        {
            for (int y = playerCoord.y - drawDistance; y < playerCoord.y + drawDistance; y++)
            {
                if (!chunks.ContainsKey(new Vector2Int(x, y)))
                {
                    createChunk(x, y);
                }
            }
        }
    }


    void createChunk(int x, int y)
    {
        chunks.Add(new Vector2Int(x, y), Instantiate(chunk, new Vector3(x * chunkSize, 0, y * chunkSize), Quaternion.identity));
    }

    void EraseOldChunks()
    {
        List<Vector2Int> toRemove = new List<Vector2Int>();

        foreach (var item in chunks)
        {
            if (item.Key.x < playerCoord.x - drawDistance ||
                item.Key.x > playerCoord.x + drawDistance ||
                item.Key.y < playerCoord.y - drawDistance ||
                item.Key.y > playerCoord.y + drawDistance)
            {
                toRemove.Add(new Vector2Int(item.Key.x, item.Key.y));
            }
        }

        foreach (var item in toRemove)
        {
            removeChunk(item.x, item.y);
        }
    }

    void removeChunk(int x, int y)
    {
        Destroy(chunks[new Vector2Int(x, y)]);
        chunks.Remove(new Vector2Int(x, y));
    }

    public Chunk GetChunk(Vector2Int coord)
    {
        return chunks[coord].GetComponent<Chunk>();
    }
}
