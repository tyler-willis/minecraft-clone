using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Face
{
    Top,
    Bottom,
    North,
    South,
    East,
    West
}

public enum BlockType
{
    Air,
    Grass,
    Dirt,
    Stone,
    Bedrock,
    Water,
    OakTrunk,
    OakLeaves,
    BirchTrunk,
    BirchLeaves,
    Coal,
    Iron,
    Gold,
    Diamond
}

public enum TreeType
{
    Oak,
    Birch
}

public static class TextureMap
{
    public static Vector2[] GetUvs(BlockType blockType, Face face)
    {
        Vector2[] uv = new Vector2[4];

        switch (blockType)
        {
            case BlockType.Air:
                break;
            case BlockType.Grass:
                uv = GetGrass(face);
                break;
            case BlockType.Dirt:
                uv = GetTextureCoords(1, 6);
                break;
            case BlockType.Stone:
                uv = GetTextureCoords(1, 7);
                break;
            case BlockType.Bedrock:
                uv = GetTextureCoords(4, 7);
                break;
            case BlockType.Coal:
                uv = GetTextureCoords(5, 7);
                break;
            case BlockType.Iron:
                uv = GetTextureCoords(6, 7);
                break;
            case BlockType.Gold:
                uv = GetTextureCoords(5, 6);
                break;
            case BlockType.Diamond:
                uv = GetTextureCoords(6, 6);
                break;
            case BlockType.BirchTrunk:
                uv = GetTreeTrunk(face, TreeType.Birch);
                break;
            case BlockType.OakTrunk:
                uv = GetTreeTrunk(face, TreeType.Oak);
                break;
            case BlockType.BirchLeaves:
                uv = GetTextureCoords(3, 5);
                break;
            case BlockType.OakLeaves:
                uv = GetTextureCoords(2, 5);
                break;
            default:
                Debug.LogError("ERROR: Tried to get the UV for an unsupported block type");
                break;
        }

        return uv;
    }

    private static Vector2[] GetAir(Face face)
    {
        Debug.LogError("ERROR: Tried to draw a face on an air block");
        return new Vector2[4];
    }

    private static Vector2[] GetGrass(Face face)
    {
        Vector2[] uv = new Vector2[4];

        switch (face)
        {
            case Face.Top:
                uv = GetTextureCoords(0, 7);
                break;
            case Face.Bottom:
                uv = GetTextureCoords(1, 6);
                break;
            default:   // N, S, E, and W are the same texture
                uv = GetTextureCoords(0, 6);
                break;
        }

        return uv;
    }

    private static Vector2[] GetTreeTrunk(Face face, TreeType treeType)
    {
        Vector2[] uv = new Vector2[4];

        if (treeType == TreeType.Oak)
        {
            if (face == Face.Top || face == Face.Bottom)
                uv = GetTextureCoords(2, 7);
            else
                uv = GetTextureCoords(2, 6);
        }
        else if (treeType == TreeType.Birch)
        {
            if (face == Face.Top || face == Face.Bottom)
                uv = GetTextureCoords(3, 7);
            else
                uv = GetTextureCoords(3, 6);
        }
        else
        {
            Debug.LogError("ERROR: Tried to load the UV for a tree that doesn't exist");
            uv = GetTextureCoords(0, 7);
        }

        return uv;
    }

    private static Vector2[] GetTextureCoords(int x, int y)
    {
        float s = 1f / 8f;

        Vector2[] output = new Vector2[4]{
            new Vector2((x  ) * s, (y  ) * s),
            new Vector2((x  ) * s, (y+1) * s),
            new Vector2((x+1) * s, (y+1) * s),
            new Vector2((x+1) * s, (y  ) * s)
        };

        return output;
    }
}
