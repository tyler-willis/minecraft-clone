using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject sphere;

    public float moveSpeed = 6f;
    public float rotateSpeed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float mouseSensitivity = 2.0f;

    public Vector2Int currentChunk;

    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    private int jumps;
    private ChunkHandler chunkHandler;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        chunkHandler = GameObject.Find("ChunkHandler").GetComponent<ChunkHandler>();

        // Lock capsule rotation
        
    }

    // Update is called once per frame
    void Update()
    {
        FixRotation();
        Move();
        UpdateChunk();
        HandleInput();
    }

    void FixRotation()
    {
        
    }

    void Move()
    {
        // Moving and Jumping
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= moveSpeed;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        // Gravity
        moveDirection.y -= gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }

    void UpdateChunk()
    {
        currentChunk = new Vector2Int((int)transform.position.x, (int)transform.position.z) / chunkHandler.chunkSize;
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            BreakBlock();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            CreateBlock();
        }
    }
    void BreakBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Chunk chunk = hit.transform.gameObject.GetComponent<Chunk>();

            // Move the position slightly further into the block and round down
            Vector3Int targetBlock = Vector3Int.FloorToInt(hit.point - (hit.normal / 2));
            targetBlock.x %= chunkHandler.chunkSize;
            targetBlock.z %= chunkHandler.chunkSize;

            if (targetBlock.x <= 0)
                targetBlock.x += chunkHandler.chunkSize;

            if (targetBlock.z <= 0)
                targetBlock.z += chunkHandler.chunkSize;

            // Debug.Log("Chunk coordinate: " + chunk.chunkCoord.x + ", " + chunk.chunkCoord.y);
            Debug.Log("Hit coordinate: " + Mathf.FloorToInt(targetBlock.x) + ", " + Mathf.FloorToInt(targetBlock.y) + ", " + Mathf.FloorToInt(targetBlock.z));

            if (targetBlock.x == 0)
            {
                Debug.Log("Border on the x axis");
            }

            if (targetBlock.z == 0)
            {
                Debug.Log("Border on the z axis");
            }

            if (chunk.GetBlock(targetBlock) != BlockType.Bedrock)
                chunk.RemoveBlock(targetBlock);
        }
    }
    void CreateBlock()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            Chunk chunk = hit.transform.gameObject.GetComponent<Chunk>();

            Vector3Int targetBlock = Vector3Int.FloorToInt(hit.point + (hit.normal / 5));
            targetBlock.x %= chunkHandler.chunkSize;
            targetBlock.z %= chunkHandler.chunkSize;

            if (targetBlock.x < 0)
                targetBlock.x += chunkHandler.chunkSize;

            if (targetBlock.z < 0)
                targetBlock.z += chunkHandler.chunkSize;

            chunk.AddBlock(targetBlock, BlockType.Dirt);
        }
    }
}
