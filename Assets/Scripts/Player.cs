using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Controls the player character 'Annie' in the game.
/// Handles movement, jumping, attacking, grappling, health management, and special abilities like Grootino.
/// Inherits from HealthSystem to manage player health.
/// This class integrates input handling, physics, animations, and game logic for the player.
/// </summary>
public class Player : HealthSystem
{
    /// <summary>
    /// Reference to the player's input subscription component for handling input events.
    /// </summary>
    [SerializeField] PlayerInputSubscription playerInputSubscription;

    /// <summary>
    /// The player's Rigidbody2D component for physics-based movement.
    /// </summary>
    Rigidbody2D rb;

    /// <summary>
    /// Movement speed of the player.
    /// </summary>
    [SerializeField] float speed = 0f;

    /// <summary>
    /// Force applied when the player jumps.
    /// </summary>
    [SerializeField] float jumpForce = 5f;

    /// <summary>
    /// Distance to check for ground below the player to determine if grounded.
    /// </summary>
    [SerializeField] float groundCheckDistance = 0.1f;

    /// <summary>
    /// SpriteRenderer for the main Annie character sprite.
    /// </summary>
    [SerializeField] SpriteRenderer AnnieSpriteRenderer;

    /// <summary>
    /// SpriteRenderer for the bottom part of the character (possibly for layering).
    /// </summary>
    [SerializeField] SpriteRenderer BottomSpriteRenderer;

    /// <summary>
    /// Animator for the body animations of the player.
    /// </summary>
    [SerializeField] Animator BodyAnimator;

    /// <summary>
    /// Animator for the bottom animations of the player.
    /// </summary>
    [SerializeField] Animator BottomAnimator;

    /// <summary>
    /// Reference to the grapple controller for handling grappling mechanics.
    /// </summary>
    [SerializeField] GrappleController grappleController;

    /// <summary>
    /// Layer mask used to identify ground for collision checks.
    /// </summary>
    [SerializeField] LayerMask groundLayer;

    /// <summary>
    /// Prefab for instantiating Grootino, the player's special companion.
    /// </summary>
    [SerializeField] GameObject grootinoPrefab;

    /// <summary>
    /// Event triggered when the player dies.
    /// </summary>
    public event EventHandler OnPlayerDead;

    /// <summary>
    /// Event triggered when the player's health changes, passing current and max health.
    /// </summary>
    public event Action<int,int> OnHealthChanged;

    /// <summary>
    /// Instance of the Grootino object when spawned.
    /// </summary>
    private GameObject grootinoInstance;

    /// <summary>
    /// Flag indicating if the player can spawn Grootino.
    /// </summary>
    private bool CanSpawnGrootino = false;

    /// <summary>
    /// Flag indicating if the player can jump.
    /// </summary>
    private bool canJump = true;

    /// <summary>
    /// Cooldown time between attacks.
    /// </summary>
    float attackCooldown = 0.1f;

    /// <summary>
    /// Timestamp of the last attack for cooldown management.
    /// </summary>
    float lastAttackTime = 0f;

    /// <summary>
    /// Current movement input vector.
    /// </summary>
    Vector2 moveInput = Vector2.zero;

    /// <summary>
    /// Flag indicating if the player is grounded.
    /// </summary>
    bool isGrounded = false;

    /// <summary>
    /// Flag indicating if the player is currently grappling.
    /// </summary>
    bool isGrappling = false;

    /// <summary>
    /// Flag indicating if the player is in a knockback state.
    /// </summary>
    private bool isKnockedBack = false;

    /// <summary>
    /// Property indicating if the player is in range of an interactable object.
    /// </summary>
    public bool IsInRange { get; set; }

    /// <summary>
    /// Flag to track the last attack input state.
    /// </summary>
    private bool lastAttackInput = false;

    /// <summary>
    /// Damage value dealt by the player's attacks.
    /// </summary>
    public int damage = 1;

    /// <summary>
    /// Timestamp of the last time the player took damage.
    /// </summary>
    private float lastDamageTime = 0f;

    /// <summary>
    /// Cooldown period after taking damage to prevent rapid damage.
    /// </summary>
    private float damageCooldown = 0.2f;


    /// <summary>
    /// Initializes the player component.
    /// Sets up references, initializes flags, and disables input initially.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        IsInRange = false;
        CanSpawnGrootino = false;
        playerInputSubscription.SetInputEnabled(false);
    }

    /// <summary>
    /// Updates the player state every frame.
    /// Handles movement input, ground checking, attacking, and jumping if not dead.
    /// </summary>
    void Update()
    {
        if (isDead) return;
        moveInput = playerInputSubscription.MoveInput;
        CheckGroundContact();
        HandleAttack();
        HandleJump();
        Move();
        HandleSpawnGrootino();
        HandleLaunch();
    }

    void Move()
    {
        if (isKnockedBack) return;

        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
        rb.angularVelocity = 0f;
        if (moveInput.x < 0)
        {
            AnnieSpriteRenderer.flipX = true;
            BottomSpriteRenderer.flipX = true;
        }
        else if (moveInput.x > 0)
        {
            AnnieSpriteRenderer.flipX = false;
            BottomSpriteRenderer.flipX = false;
        }
        if (rb.linearVelocity != Vector2.zero)
        {
            BodyAnimator.SetBool("Walking", true);
            BottomAnimator.SetBool("Walking", true);
        }
        else
        {
            BodyAnimator.SetBool("Walking", false);
            BottomAnimator.SetBool("Walking", false);
        }
    }

    void CheckGroundContact()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
        if (isGrounded) canJump = true;
        Debug.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);

    }

    void HandleJump()
    {
        if (playerInputSubscription.JumpInput)
        {
            if (canJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                canJump = false;
            }
            else if (isGrappling)
            {
                isGrappling = false;
                grappleController.StopGrapple();
                Vector2 direction = moveInput.x != 0 ? new Vector2(moveInput.x, 1).normalized : Vector2.up;
                rb.linearVelocity = new Vector2(direction.x * jumpForce * 1.8f, jumpForce * 1.5f);
            }
        }
    }

    public int GetDirection()
    {
        if (AnnieSpriteRenderer.flipX)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    void HandleLaunch()
    {
        if (playerInputSubscription.InteractInput && !IsInRange)
        {
            if (grootinoInstance != null)
            {
                grootinoInstance.GetComponent<Grootino>().TryLaunch();
            }
        }
    }
    void HandleAttack()
    {
        bool currentAttackInput = playerInputSubscription.AttackInput;
        if (currentAttackInput && !lastAttackInput && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            playerInputSubscription.AttackInput = false;
            if (!isGrappling)
            {
                grappleController.StartGrapple();
            }
        }
        if (playerInputSubscription.DeattackInput)
        {
            if (isGrappling)
            {
                isGrappling = false;
                grappleController.StopGrapple();
            }
        }

        lastAttackInput = currentAttackInput;
    }
    

    private void HandleSpawnGrootino()
    {
        if (CanSpawnGrootino && grootinoInstance == null)
        {
            StartCoroutine(SpawnGrootino());
            CanSpawnGrootino = false;
        }
    }

    private IEnumerator SpawnGrootino()
    {


        if (grootinoInstance == null)
        {
            yield return new WaitForSeconds(0.2f);
            IsInRange = true;
            grootinoInstance = Instantiate(grootinoPrefab, transform.position, Quaternion.identity, transform);
            yield return new WaitForSeconds(0.8f);
            IsInRange = false;
            CanSpawnGrootino = true;
        }
    }


    public override void TakeDamage(int damage)
    {
        if (Time.time < lastDamageTime + damageCooldown)
            return;
        lastDamageTime = Time.time;
        base.TakeDamage(damage);
        BodyAnimator.SetTrigger("Damage");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    protected override void FlashRed(float duration)
    {
        StartCoroutine(FlashCoroutine(duration));
    }

    private IEnumerator FlashCoroutine(float duration)
    {
        float endTime = Time.time + duration;
        Color originalAnnie = AnnieSpriteRenderer.color;
        Color originalBottom = BottomSpriteRenderer.color;
        while (Time.time < endTime)
        {
            AnnieSpriteRenderer.color = Color.red;
            BottomSpriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            AnnieSpriteRenderer.color = originalAnnie;
            BottomSpriteRenderer.color = originalBottom;
            yield return new WaitForSeconds(0.2f);
        }
        AnnieSpriteRenderer.color = originalAnnie;
            BottomSpriteRenderer.color = originalBottom;
    }

    public void TakeKnockback(Vector2 direction, float force)
    {
        isKnockedBack = true;
        rb.linearVelocity = new Vector2(direction.x * force, rb.linearVelocity.y);
        StartCoroutine(KnockbackCoroutine(0.2f));
    }

    protected override void Die()
    {
        base.Die();
        BodyAnimator.SetTrigger("Death");
        OnPlayerDead?.Invoke(this, EventArgs.Empty);
    }

    public void SetGrootino()
    {
        CanSpawnGrootino = true;
    }

    public void EnableGameplayInput(bool enable)
    {
            playerInputSubscription.SetInputEnabled(enable); 
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public void SetGrappleState(bool state)
    {
        isGrappling = state;
    }

    private IEnumerator KnockbackCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isKnockedBack = false;
    }
}
