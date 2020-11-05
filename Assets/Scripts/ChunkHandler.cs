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
    public int cacheDelay = 5;

    Dictionary<Vector2Int, GameObject> chunks;
    private Vector2Int playerCoord;
    private List<(Vector2Int, bool)> chunkCache;
    private int cacheTimer;

    // Start is called before the first frame update
    void Start()
    {
        seed = (int)Random.Range(-2000000000f, 2000000000f);
        Random.InitState(seed);
        playerCoord = new Vector2Int(0, 0);
        chunkCache = new List<(Vector2Int, bool)>();
        cacheTimer = cacheDelay;

        chunks = new Dictionary<Vector2Int, GameObject>();
        // Add initial chunk to diction

        // Create initial landscape around player
        BuildNewChunks();
        BuildNewChunks();
        while (chunkCache.Count > 0)
            RunCache();
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

        if (chunkCache.Count > 0)
            RunCache();
    }

    void BuildNewChunks()
    {
        for (int x = playerCoord.x - drawDistance; x < playerCoord.x + drawDistance; x++)
        {
            for (int y = playerCoord.y - drawDistance; y < playerCoord.y + drawDistance; y++)
            {
                if (!chunks.ContainsKey(new Vector2Int(x, y)) && !chunkCache.Contains((new Vector2Int(x, y), false)))
                {
                    chunkCache.Add((new Vector2Int(x, y), false));
                } // Check if the chunk exists, but is inctive
                else if (chunks.ContainsKey(new Vector2Int(x, y)))
                {
                    if (chunks[new Vector2Int(x, y)].activeSelf == false)
                        chunks[new Vector2Int(x, y)].SetActive(true);
                }
            }
        }
    }


    void CreateChunk(Vector2Int coord)
    {
        chunks.Add(coord, Instantiate(chunk, new Vector3(coord.x * chunkSize, 0, coord.y * chunkSize), Quaternion.identity));
    }

    void EraseOldChunks()
    {
        foreach (var item in chunks)
        {
            if (!chunkCache.Contains((new Vector2Int(item.Key.x, item.Key.y), true)))
            {
                if (item.Key.x < playerCoord.x - drawDistance ||
                    item.Key.x > playerCoord.x + drawDistance ||
                    item.Key.y < playerCoord.y - drawDistance ||
                    item.Key.y > playerCoord.y + drawDistance)
                {
                    chunkCache.Add((new Vector2Int(item.Key.x, item.Key.y), true));
                }
            }
        }
    }

    void RemoveChunk(Vector2Int coord)
    {
        if (!chunks.ContainsKey(coord))
        {
            Debug.LogError("ERROR: Tried to delete a chunk that doesn't exist");
            return;
        }

        //Destroy(chunks[coord]);
        //chunks.Remove(coord);

        chunks[coord].SetActive(false);
    }

    public Chunk GetChunk(Vector2Int coord)
    {
        return chunks[coord].GetComponent<Chunk>();
    }

    private void RunCache()
    {
        if (cacheTimer > 0)
        {
            cacheTimer--;
            return;
        }

        cacheTimer = cacheDelay;

        (Vector2Int, bool) chunk = chunkCache[0];

        // Delete chunk
        if (chunk.Item2)
        {
            RemoveChunk(chunk.Item1);
        }
        // Add chunk
        else
        {
            CreateChunk(chunk.Item1);
        }

        chunkCache.RemoveAt(0);
    }

    private void CreateInitialChunks()
    {

    }
}
