using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelExporter : MonoBehaviour
{
    [SerializeField] Seed seed;
    public string fileName = "generated_level_data.json";
    private void Start()
    {
        var input = FindAnyObjectByType<PlayerInputSubscription>();
        if (input != null)
            input.OnExportPressed += ExportLevel;
    }
    public List<Vector2Int> solidTiles = new List<Vector2Int>();

    public void ExportLevel()
    {
    Debug.Log("Exporting level...");

    LevelData level = new LevelData();

        Tilemap tilemap = GameObject.Find("Ground").GetComponent<Tilemap>();

        // Player
        Player player = FindAnyObjectByType<Player>();
        Vector3Int playerCell = tilemap.WorldToCell(player.transform.position);
        level.playerStart = new Vector2Int(playerCell.x, playerCell.y -1);
        level.moveSpeed = player.GetSpeed();
        level.grappleRange = player.GetGrappleController().GetGrappleRange();
        float gravity = Physics2D.gravity.y * player.GetComponentInChildren<Rigidbody2D>().gravityScale;
        float jumpVelocity = player.GetJumpForce();
        float maxHeight = (jumpVelocity * jumpVelocity) / (2 * Mathf.Abs(gravity));

        level.maxJumpHeight = Mathf.RoundToInt(maxHeight);
        float timeInAir = (2 * jumpVelocity) / Mathf.Abs(gravity);
        float maxDistance = player.GetSpeed() * timeInAir;

        level.maxJumpDistance = Mathf.RoundToInt(maxDistance);
        
        float flowerJumpVelocity = seed.GetJumpForce();
        float flowerMaxHeight = (flowerJumpVelocity * flowerJumpVelocity) / (2 * Mathf.Abs(gravity));

        level.maxFlowerJumpHeight = Mathf.RoundToInt(flowerMaxHeight);
        float flowerTimeInAir = (2 * flowerJumpVelocity) / Mathf.Abs(gravity);
        float flowerMaxDistance = player.GetSpeed() * flowerTimeInAir;

        level.maxFlowerJumpDistance = Mathf.RoundToInt(maxDistance);

        // Boss
        Boss boss = FindAnyObjectByType<Boss>();
        if (boss != null)
            level.bossArenaPosition = boss.transform.position;

        // Goal (se separato dal boss)
        // oppure usa un Empty GameObject chiamato "Goal"
        GameObject goal = GameObject.Find("BossSpawn");
        if (goal != null)
        {
            Vector3Int goalCell = tilemap.WorldToCell(goal.transform.position);
            level.goalPosition = new Vector2Int(goalCell.x, goalCell.y-2);
        }

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                if (tilemap.HasTile(cellPos))
                {
                    solidTiles.Add(new Vector2Int(x, y));
                }
            }
        }
        level.solidTiles = solidTiles;

        // Enemies
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        level.enemies = enemies.Select(e => new EnemyData
        {
            position = e.transform.position
        }).ToList();

        string json = JsonUtility.ToJson(level, true);
        string path = Path.Combine(
            Application.dataPath,
            "Scripts",
            "AI",
            fileName
        );
        File.WriteAllText(path, json);

        Debug.Log("Level exported to: " + path);
    }
}