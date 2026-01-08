using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton manager class that oversees the overall game state, including seed management, boss fights, UI screens, and game flow.
/// Handles events like game over, game win, and transitions between different game phases.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// List of all active seed game objects in the scene.
    /// </summary>
    [SerializeField] private List<GameObject> Seeds;

    /// <summary>
    /// Prefab used to instantiate new seeds.
    /// </summary>
    [SerializeField] private GameObject seedPrefab;

    /// <summary>
    /// Cinemachine camera used during boss fights.
    /// </summary>
    [SerializeField] private CinemachineCamera BossCamera;

    /// <summary>
    /// Cinemachine camera used for following the player.
    /// </summary>
    [SerializeField] private CinemachineCamera PlayerCamera;

    /// <summary>
    /// Reference to the heart health bar UI component.
    /// </summary>
    [SerializeField] private HeartHealthBar heartHealthBar;

    /// <summary>
    /// Prefab for the boss enemy.
    /// </summary>
    [SerializeField] private GameObject boss;

    /// <summary>
    /// Reference to the player game object.
    /// </summary>
    [SerializeField] private GameObject player;

    /// <summary>
    /// Spawn point for the boss.
    /// </summary>
    [SerializeField] private Transform bossSpawnPoint;

    /// <summary>
    /// Prefab for the rock object used in boss fight cutscene.
    /// </summary>
    [SerializeField] private GameObject rockPrefab;

    /// <summary>
    /// Spawn point for the rock in the boss fight.
    /// </summary>
    [SerializeField] private Transform rockSpawnPoint;

    /// <summary>
    /// Position for spawning the first seed in cutscenes.
    /// </summary>
    [SerializeField] private Vector3 seed1Position;

    /// <summary>
    /// Position for spawning the second seed in cutscenes.
    /// </summary>
    [SerializeField] private Vector3 seed2Position;

    /// <summary>
    /// Right boundary constraint for boss movement.
    /// </summary>
    [SerializeField] private GameObject RightConstraint;

    /// <summary>
    /// Left boundary constraint for boss movement.
    /// </summary>
    [SerializeField] private GameObject LeftConstraint;

    /// <summary>
    /// Main menu UI screen.
    /// </summary>
    [SerializeField] private GameObject mainMenuScreen;

    /// <summary>
    /// Game over UI screen.
    /// </summary>
    [SerializeField] private GameObject gameOverScreen;

    /// <summary>
    /// Game win UI screen.
    /// </summary>
    [SerializeField] private GameObject gameWinScreen;

    /// <summary>
    /// Health bar UI element (note: likely a typo in the original, should be 'health').
    /// </summary>
    [SerializeField] private GameObject healtHealthBar;

    /// <summary>
    /// Tutorial canvas shown at the start of the game.
    /// </summary>
    [SerializeField] private GameObject TutorialCanvas;

    /// <summary>
    /// Flag indicating if the boss fight has started.
    /// </summary>
    private bool bossFightStarted = false;

    /// <summary>
    /// Ensures only one instance of GameManager exists (singleton pattern).
    /// Destroys duplicate instances.
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    /// <summary>
    /// Initializes the game manager by subscribing to player and boss events.
    /// </summary>
    private void Start()
    {
        player.GetComponent<Player>().OnPlayerDead += GameOver;
        player.GetComponent<Player>().OnHealthChanged += heartHealthBar.UpdateHearts;
    }

    /// <summary>
    /// Registers a seed game object in the seeds list.
    /// </summary>
    /// <param name="seed">The seed game object to register.</param>
    public void RegisterSeed(GameObject seed)
    {
        if (!Seeds.Contains(seed))
        {
            Seeds.Add(seed);
        }
    }

    /// <summary>
    /// Unregisters a seed game object from the seeds list.
    /// </summary>
    /// <param name="seed">The seed game object to unregister.</param>
    public void UnregisterSeed(GameObject seed)
    {
        if (Seeds.Contains(seed))
        {
            Seeds.Remove(seed);
        }
    }

    /// <summary>
    /// Returns the list of all registered seeds.
    /// </summary>
    /// <returns>List of seed game objects.</returns>
    public List<GameObject> GetSeeds()
    {
        return Seeds;
    }

    /// <summary>
    /// Spawns a new seed at the specified position and registers it.
    /// </summary>
    /// <param name="position">The position to spawn the seed.</param>
    public void SpawnSeedAtPosition(Vector3 position)
    {
        GameObject seed = Instantiate(seedPrefab, position, Quaternion.identity);
        RegisterSeed(seed);
    }

    /// <summary>
    /// Spawns two seeds at predefined positions for cutscenes and registers them.
    /// </summary>
    public void SpawnSeedCutscene()
    {
        GameObject seed1 = Instantiate(seedPrefab, seed1Position, Quaternion.identity);
        GameObject seed2 = Instantiate(seedPrefab, seed2Position, Quaternion.identity);
        RegisterSeed(seed1);
        RegisterSeed(seed2);
    }


    /// <summary>
    /// Triggered when entering a collider, starts the boss fight sequence if not already started.
    /// Switches cameras and initiates boss spawning.
    /// </summary>
    /// <param name="collision">The collider that triggered the event.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (bossFightStarted)
            return;
        Seeds.Clear();
        BossCamera.Priority = 10;
        PlayerCamera.Priority = 5;
        bossFightStarted = true;
        StartCoroutine(SpawnBoss());
    }

    /// <summary>
    /// Coroutine that handles the boss fight initialization sequence.
    /// Spawns the boss, rock, and manages the progression of the fight phases.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public IEnumerator SpawnBoss()
    {
        bool rockDone = false;


        Player playerCtrl = player.GetComponent<Player>();
        yield return new WaitUntil(() => playerCtrl.IsGrounded());
        playerCtrl.EnableGameplayInput(false);

        GameObject bossInstance =
            Instantiate(boss, bossSpawnPoint.position, Quaternion.identity);
        Boss bossCtrl = bossInstance.GetComponent<Boss>();
        bossCtrl.OnBossDefeated += GameWin;

        bossCtrl.SetPlayer(player.transform);
        bossCtrl.SetupConstraints(RightConstraint, LeftConstraint);
        yield return new WaitForSeconds(0.5f);

        GameObject rockGO =
        Instantiate(rockPrefab, rockSpawnPoint.position, Quaternion.identity);

        Rock rock = rockGO.GetComponent<Rock>();

        rock.OnRockFinished += () => rockDone = true;

        yield return new WaitUntil(() => rockDone);
        Debug.Log("Rock finished falling");
        StartCoroutine(bossCtrl.PerformStompAttack());
        yield return new WaitUntil(() => Seeds.Count >= 2);
        StartCoroutine(bossCtrl.PerformFlameAttack());
        yield return new WaitUntil(() => Seeds.Count >= 1);
        playerCtrl.EnableGameplayInput(true);
        bossCtrl.StartBossFight();
    }

    /// <summary>
    /// Handles the game over event by pausing the game and showing the game over screen.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">Event arguments.</param>
    private void GameOver(object sender, EventArgs e)
    {
        Time.timeScale = 0f;
        gameOverScreen.SetActive(true);
    }

    /// <summary>
    /// Handles the game win event by pausing the game and showing the win screen.
    /// </summary>
    /// <param name="sender">The object that triggered the event.</param>
    /// <param name="e">Event arguments.</param>
    private void GameWin(object sender, EventArgs e)
    {
        Time.timeScale = 0f;
        gameWinScreen.SetActive(true);
    }

    /// <summary>
    /// Starts the game by hiding menus, enabling input, and setting up initial state.
    /// </summary>
    public void StartGame()
    {
        mainMenuScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        gameWinScreen.SetActive(false);
        healtHealthBar.SetActive(true);
        player.GetComponent<Player>().EnableGameplayInput(true);
        PlayerCamera.Priority = 10;
        TutorialCanvas.SetActive(true);
    }

    /// <summary>
    /// Restarts the game by reloading the current scene.
    /// </summary>
    public void RestartGame()
    {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
