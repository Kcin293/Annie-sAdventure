using System.Collections;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Represents a seed object in the game that can be interacted with and grown.
/// The seed has different states (Idle, Growing, Ready, Recoil)
/// Inherits from Interactable to allow player interaction.
/// </summary>
public class Seed : Interactable
{
    /// <summary>
    /// Animator component for handling seed animations.
    /// </summary>
    [SerializeField] private Animator seedAnimator;

    /// <summary>
    /// Rigidbody2D for physics interactions of the seed.
    /// </summary>
    [SerializeField] private Rigidbody2D seedRigidbody;

    /// <summary>
    /// Reference to the game manager for registering the seed.
    /// </summary>
     private GameManager gameManager;

    /// <summary>
    /// Force applied when the seed jumps or recoils.
    /// </summary>
    [SerializeField] private float jumpForce = 10f;

    /// <summary>
    /// Distance to check for ground below the seed.
    /// </summary>
    [SerializeField] private float groundCheckDistance = 0.55f;

    /// <summary>
    /// Current state of the seed (Idle, Growing, Ready, Recoil).
    /// </summary>
    private SeedState currentState = SeedState.Idle;

    /// <summary>
    /// Reference to the player object for interaction.
    /// </summary>
    private GameObject playerObject;

    /// <summary>
    /// Rigidbody of another object, possibly for collision handling.
    /// </summary>
    private Rigidbody2D otherRigidbody;

    /// <summary>
    /// Current coroutine running for state transitions.
    /// </summary>
    private Coroutine currentCoroutine;

    /// <summary>
    /// Flag indicating if the seed is grounded.
    /// </summary>
    [SerializeField] private bool isGrounded = false;

    /// <summary>
    /// Enumeration of possible seed states.
    /// </summary>
    private enum SeedState
    {
        Idle,
        Growing,
        Ready,
        Recoil
    }

    /// <summary>
    /// Initializes the seed component.
    /// Finds and registers with the game manager.
    /// </summary>
    private void Awake()
    {
        isGrounded = false;
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        if (gameManager != null)
        {
            gameManager.RegisterSeed(this.gameObject);
        }
    }

    /// <summary>
    /// Updates the seed every frame.
    /// Checks for ground contact if not grounded.
    /// </summary>
    private void Update()
    {
        if(isGrounded == false)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground"));
            UnityEngine.Debug.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
            if (hit.collider != null)
            {
                isGrounded = true;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                      rb.angularVelocity = 0f;
                    rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
        }
    }
    private void StartSafeCoroutine(IEnumerator routine)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);

        currentCoroutine = StartCoroutine(routine);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Pet"))
        {
          return;  
        }
        if (currentState != SeedState.Idle)
            return;
        UnityEngine.Debug.Log("isGrounded: " + isGrounded);
        if(isGrounded == false)
            return;

        UnityEngine.Debug.Log("Seed Interacted");
        Destroy(collision.gameObject);
        currentState = SeedState.Growing;
        seedAnimator.SetTrigger("Grow");

        StartSafeCoroutine(SetReady());
    }

    private IEnumerator SetReady()
    {
        seedAnimator.SetTrigger("Scende");
        yield return new WaitForSeconds(0.6f);
        currentState = SeedState.Ready;

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player"))
            return;

        Vector2 normal = collision.contacts[0].normal;

        if (normal.y > 0.5f)
            return;

        if (currentState == SeedState.Ready)
        {
            playerObject = collision.collider.gameObject;
            otherRigidbody = playerObject.GetComponent<Rigidbody2D>();
            playerObject.transform.SetParent(transform);

            StartSafeCoroutine(RecoilCoroutine());
        }
    }

    private IEnumerator RecoilCoroutine()
    {
        currentState = SeedState.Recoil;

        seedAnimator.SetTrigger("Sale");
        yield return new WaitForSeconds(0.4f);

        playerObject.transform.SetParent(null);

        otherRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(1.5f);

        StartSafeCoroutine(SetReady());
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.transform.SetParent(null);
        }
    }

    public void TakeDamage()
    {
        StackTrace stackTrace = new StackTrace();
        UnityEngine.Debug.Log(stackTrace);
        gameManager.UnregisterSeed(this.gameObject);
        Destroy(this.gameObject);
    }

    public float GetJumpForce()
    {
        return jumpForce;
    }
}
