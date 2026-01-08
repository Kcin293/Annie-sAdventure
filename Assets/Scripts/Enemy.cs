using System.Collections;
using UnityEngine;

/// <summary>
/// Represents a basic enemy in the game that patrols between points and chases the player when in range.
/// Inherits from HealthSystem to manage health, damage, and death.
/// The enemy can patrol predefined points, detect the player via triggers, and perform attacks.
/// </summary>
public class Enemy : HealthSystem
{
    /// <summary>
    /// Movement speed of the enemy.
    /// </summary>
    [SerializeField] float speed = 2f;

    /// <summary>
    /// Array of patrol points that the enemy moves between when not chasing.
    /// </summary>
    [SerializeField] Transform[] patrolPoints;

    /// <summary>
    /// The current target transform the enemy is moving towards (patrol point or player).
    /// </summary>
    [SerializeField] private Transform currentTarget;

    /// <summary>
    /// Transform used to check for ground ahead to prevent falling off edges.
    /// </summary>
    [SerializeField] Transform groundCheck;

    /// <summary>
    /// Distance to check for ground below the groundCheck transform.
    /// </summary>
    [SerializeField] float groundCheckDistance = 0.5f;

    /// <summary>
    /// Layer mask defining what is considered ground for raycasting.
    /// </summary>
    [SerializeField] LayerMask groundLayer;

    /// <summary>
    /// Index of the current patrol point the enemy is heading towards.
    /// </summary>
    private int currentPointIndex = 0;

    /// <summary>
    /// Time to wait at each patrol point before moving to the next.
    /// </summary>
    private float waitTimeAtPoint = 1f;

    /// <summary>
    /// Timer tracking how long the enemy has been waiting at the current point.
    /// </summary>
    private float waitTimer = 0f;

    /// <summary>
    /// Flag indicating whether the enemy is currently chasing the player.
    /// </summary>
    private bool chase = false;

    /// <summary>
    /// Flag indicating the direction the enemy is facing (true for right, false for left).
    /// </summary>
    private bool facingRight = false;

    /// <summary>
    /// Animator component for controlling enemy animations.
    /// </summary>
    [SerializeField] Animator EnemyAnimator;

    /// <summary>
    /// Overrides the base TakeDamage method to trigger the damage animation.
    /// </summary>
    /// <param name="damage">The amount of damage to take.</param>
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        EnemyAnimator.SetTrigger("Damage");
    }

    /// <summary>
    /// Overrides the base FlashRed method to start the flashing coroutine.
    /// </summary>
    /// <param name="duration">The duration of the flashing effect.</param>
    protected override void FlashRed(float duration)
    {
        StartCoroutine(FlashCoroutine(duration));
    }

    /// <summary>
    /// Coroutine that handles the flashing red effect when the enemy takes damage.
    /// Alternates between red and original color for the specified duration.
    /// </summary>
    /// <param name="duration">The total duration of the flashing effect.</param>
    private IEnumerator FlashCoroutine(float duration)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            float endTime = Time.time + duration;
            Color original = sr.color;
            while (Time.time < endTime)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(0.2f);
                sr.color = original;
                yield return new WaitForSeconds(0.2f);
            }
        }
    }


    /// <summary>
    /// Overrides the base Die method to trigger death animation and spawn a seed.
    /// Destroys the enemy object after a delay.
    /// </summary>
    protected override void Die()
    {
        base.Die();
        EnemyAnimator.SetTrigger("Die");
        GameManager.Instance.SpawnSeedAtPosition(transform.position);
        Destroy(gameObject.transform.parent.gameObject, 1.3f);
    }

    /// <summary>
    /// Updates the enemy behavior every frame if not dead.
    /// Calls the Move method to handle movement logic.
    /// </summary>
    private void Update()
    {
        if (!isDead)
        {
            Move();
        }
    }

    /// <summary>
    /// Handles the movement logic of the enemy, deciding between patrolling and chasing.
    /// Checks for ground ahead and switches to appropriate behavior.
    /// </summary>
    private void Move()
    {
        if (!IsGroundAhead())
        {
            Flip();
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            return;
        }
        if (chase)
        {
            ChasePlayer();
            return;
        }
        Patrol();
    }

    /// <summary>
    /// Handles the patrolling behavior, moving between predefined patrol points.
    /// Waits at each point for a specified time before moving to the next.
    /// </summary>
    private void Patrol()
    {

        if (patrolPoints.Length == 0) return;

        currentTarget = patrolPoints[currentPointIndex];
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
        if (direction.x < 0 && facingRight)
        {
            Flip();
        }
        else if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        EnemyAnimator.SetBool("Walk", true);
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            EnemyAnimator.SetBool("Walk", false);
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                waitTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Flips the enemy's facing direction by reversing the local scale.
    /// </summary>
    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

    }

    /// <summary>
    /// Called when another collider enters the enemy's trigger collider.
    /// If the collider is the player, starts chasing the player.
    /// </summary>
    /// <param name="collision">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            chase = true;
            currentTarget = player.transform;
        }
    }

    /// <summary>
    /// Called when another collider exits the enemy's trigger collider.
    /// If the collider is the player, stops chasing and returns to patrolling.
    /// </summary>
    /// <param name="collision">The collider that exited the trigger.</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            chase = false;
            currentTarget = player.transform;
        }
    }


    /// <summary>
    /// Handles the chasing behavior when the player is in range.
    /// Moves towards the player and attacks if close enough.
    /// </summary>
    private void ChasePlayer()
    {
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        if (direction.x < 0 && facingRight)
        {
            Flip();
        }
        else if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.9f)
        {
            EnemyAnimator.SetBool("Walk", false);
            EnemyAnimator.SetTrigger("Attack");
            return;
        }
        else
        {
                        EnemyAnimator.SetBool("Walk", true);
            transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
        }
        
    }

    /// <summary>
    /// Checks if there is ground ahead using a raycast from the groundCheck position.
    /// Used to prevent the enemy from walking off edges.
    /// </summary>
    /// <returns>True if ground is detected, false otherwise.</returns>
    private bool IsGroundAhead()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, Color.yellow);

        return hit.collider != null;
    }
}