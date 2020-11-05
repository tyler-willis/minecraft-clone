using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChunkText : MonoBehaviour
{
    private Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2Int coords = this.transform.parent.transform.parent.transform.parent.GetComponent<Player>().currentChunk;
        text.text = "Current Coordinates: " + coords.x + ", " + coords.y;
    }
}
