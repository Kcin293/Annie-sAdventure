using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelImporter : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase solidTile;
    public string fileName = "generated_level_data.json";
    void Start()
    {
        ImportLevel();
    }

    private void ImportLevel()
    {
        string path = Path.Combine(Application.dataPath, "Scripts", "AI", fileName);
        string json = File.ReadAllText(path);
        LevelData level = JsonUtility.FromJson<LevelData>(json);
        foreach (Vector2Int tile in level.solidTiles)
        {
            tilemap.SetTile(new Vector3Int(tile.x,tile.y, 0), solidTile);
        }
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
            player.transform.position = new Vector3(level.playerStart.x, level.playerStart.y, 0);
            player.EnableGameplayInput(true);
    }
}
