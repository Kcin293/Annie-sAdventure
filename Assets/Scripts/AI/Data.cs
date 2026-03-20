using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public Vector2 playerStart;
    public float moveSpeed;
    public float maxJumpHeight;
    public float maxJumpDistance;
    public float grappleRange;
    public Vector2 bossArenaPosition;
    public Vector2 goalPosition;

    public List<Vector2Int> solidTiles;
    public List<EnemyData> enemies;
    public float maxFlowerJumpHeight;
    public float maxFlowerJumpDistance;

}

[System.Serializable]
public class PlatformData
{
    public Vector2 position;
    public Vector2 size;
}

[System.Serializable]
public class EnemyData
{
    public Vector2 position;
}

[System.Serializable]
public class SeedData
{
    public Vector2 position;
}