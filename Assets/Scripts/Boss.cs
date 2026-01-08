/// <summary>
/// The Boss class controls the behavior of the boss enemy in the game, including movement, attacking, and AI decision-making.
/// It manages states like idle, moving, and attacking, and interacts with seeds and the player.
/// Inherits from HealthSystem to handle health, damage, and death.
/// </summary>
using System.Collections;
using UnityEngine;

/// <summary>
/// Represents the boss enemy with AI states for idle, moving, and attacking.
/// </summary>
public class Boss : HealthSystem
{
    /// <summary>
    /// The animator component for controlling boss animations.
    /// </summary>
    [SerializeField] private Animator bossAnimator;

    /// <summary>
    /// The movement speed of the boss.
    /// </summary>
    [SerializeField] private float speed = 2f;

    /// <summary>
    /// Reference to the player's transform for targeting.
    /// </summary>
    private Transform player;

    /// <summary>
    /// Event triggered when the boss is defeated.
    /// </summary>
    public event System.EventHandler OnBossDefeated;

    /// <summary>
    /// The right boundary constraint for seed spawning.
    /// </summary>
    private GameObject RightConstraint;

    /// <summary>
    /// The left boundary constraint for seed spawning.
    /// </summary>
    private GameObject LeftConstraint;

    /// <summary>
    /// The current target the boss is moving towards or attacking.
    /// </summary>
    [SerializeField] private GameObject currentTarget;

    /// <summary>
    /// The time to wait in idle state before selecting a new target.
    /// </summary>
    [SerializeField] private float idleWait = 0.2f;

    /// <summary>
    /// The range within which the boss can attack targets.
    /// </summary>
    [SerializeField] private float bossRange = 1.6f;

    /// <summary>
    /// The current attack coroutine, if any.
    /// </summary>
    Coroutine attackCoroutine;

    /// <summary>
    /// The current idle coroutine, if any.
    /// </summary>
    Coroutine idleCoroutine;

    /// <summary>
    /// Enumeration of possible boss states.
    /// </summary>
    private enum State { Idle, Moving, Attacking }

    /// <summary>
    /// The current state of the boss AI.
    /// </summary>
    [SerializeField] private State currentState = State.Idle;

    /// <summary>
    /// Flag indicating if the boss is inactive (e.g., before fight starts).
    /// </summary>
    private bool inactive = true;

    /// <summary>
    /// Updates the boss behavior every frame based on the current state.
    /// Handles state transitions and actions for idle, moving, and attacking.
    /// </summary>
    void Update()
    {
        if (isDead) return;
        if (inactive) return;

        // Stop idle coroutine if not in idle state
        if (currentState != State.Idle && idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }

        switch (currentState)
        {
            case State.Idle:
                if (idleCoroutine == null)
                {
                    idleCoroutine = StartCoroutine(IdleAndSelect());
                }
                break;

            case State.Moving:
                if (currentTarget != null)
                {
                    Move();
                    if (Vector2.Distance(transform.position, currentTarget.transform.position) < bossRange)
                    {
                        bossAnimator.SetBool("Walk", false);
                        currentState = State.Idle;
                    }
                }
                else
                {
                    currentState = State.Idle;
                }
                break;

            case State.Attacking:
                if (attackCoroutine == null)
                {
                    attackCoroutine = StartCoroutine(PerformAttack());
                }
                break;
        }
    }

    /// <summary>
    /// Moves the boss towards the current target.
    /// Calculates the direction to the target, flips the sprite based on movement direction, and updates the position.
    /// Sets the walking animation accordingly.
    /// </summary>
    private void Move()
    {
        if (currentTarget != null)
        {
            if (Vector2.Distance(transform.position, currentTarget.transform.position) < bossRange)
            {
                bossAnimator.SetBool("Walk", false);
                return;
            }
            Vector2 direction = (currentTarget.transform.position - transform.position).normalized;

            // Flip based on movement direction
            if (direction.x < 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (direction.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);

            transform.position = Vector2.MoveTowards(transform.position, currentTarget.transform.position, speed * Time.deltaTime);
            bossAnimator.SetBool("Walk", true);
        }
    }

    /// <summary>
    /// Selects a new target for the boss to move towards and attack.
    /// Chooses between the player and seeds, with a preference for the seeds.
    /// </summary>
    private void SelectNewTarget()
    {
        var seeds = GameManager.Instance.GetSeeds();
        GameObject closestTarget = null;
        closestTarget = player.gameObject;
        float closestDistance = float.MaxValue;

        if (seeds.Count == 0)
        {
            closestTarget = player.gameObject;
            closestDistance = Vector2.Distance(transform.position, player.position);
        }

        foreach (var seed in seeds)
        {
            float distance = Vector2.Distance(transform.position, seed.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = seed;
            }
        }

        currentTarget = closestTarget;
    }

    /// <summary>
    /// Performs the stomp attack animation and spawns seeds as part of the attack.
    /// </summary>
    /// <returns>An enumerator for the coroutine.</returns>
    public IEnumerator PerformStompAttack()
    {
        Debug.Log("Performing Stomp Attack");
        bossAnimator.SetTrigger("Stomp");
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.SpawnSeedCutscene();
        yield return StartCoroutine(WaitForAnimation(bossAnimator, "Stomp"));
    }

    /// <summary>
    /// Performs the flame attack animation.
    /// Clears the current target after the attack.
    /// </summary>
    /// <returns>An enumerator for the coroutine.</returns>
    public IEnumerator PerformFlameAttack()
    {
        bossAnimator.SetTrigger("FireAttack");
        currentTarget = null;
        yield return StartCoroutine(WaitForAnimation(bossAnimator, "FireAttack"));
    }

    /// <summary>
    /// Performs an attack on the current target.
    /// Chooses between stomp and flame attacks based on the target type and random chance.
    /// Flips the boss sprite towards the target and waits for animation completion.
    /// </summary>
    /// <returns>An enumerator for the coroutine.</returns>
    private IEnumerator PerformAttack()
    {
        if (currentTarget != null)
        {
            Vector2 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
            if (directionToTarget.x < 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (directionToTarget.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);
            if ( GameManager.Instance.GetSeeds().Count == 0)
            {
                bossAnimator.SetTrigger("Stomp");
                yield return new WaitForSeconds(0.5f);
                Vector3 rightPosition = new Vector3(Random.Range(transform.position.x + 2f, RightConstraint.transform.position.x), transform.position.y, transform.position.z);
                Vector3 leftPosition = new Vector3(Random.Range(LeftConstraint.transform.position.x, transform.position.x - 2f), transform.position.y, transform.position.z);
                GameManager.Instance.SpawnSeedAtPosition(rightPosition + Vector3.up * 1.5f + Vector3.right * -1f);
                GameManager.Instance.SpawnSeedAtPosition(leftPosition + Vector3.up * 1.5f + Vector3.right * 1f);
                currentTarget = null;
                yield return StartCoroutine(WaitForAnimation(bossAnimator, "Stomp"));
            }
            else{
            if (Vector2.Distance(transform.position, currentTarget.transform.position) < bossRange)
            {

                yield return StartCoroutine(PerformFlameAttack());

            }}
            currentState = State.Idle;
        attackCoroutine = null;
        }
        else
        {
            currentState = State.Idle;
            attackCoroutine = null;
        }
        
    }


    /// <summary>
    /// Waits for the specified idle duration before proceeding.
    /// </summary>
    /// <returns>An enumerator for the coroutine.</returns>
    private IEnumerator IdleWait()
    {
        yield return new WaitForSeconds(idleWait);
    }

    /// <summary>
    /// Handles the idle state: waits for idle time, selects a new target, and transitions to appropriate state.
    /// </summary>
    /// <returns>An enumerator for the coroutine.</returns>
    private IEnumerator IdleAndSelect()
    {
        yield return new WaitForSeconds(idleWait);
        SelectNewTarget();
        if (currentTarget != null)
        {
            if (Vector2.Distance(transform.position, currentTarget.transform.position) < bossRange || Random.value < 0.5f)
            {
                currentState = State.Attacking;
            }
            else
            {
                currentState = State.Moving;
            }
        }
        idleCoroutine = null;
    }

    /// <summary>
    /// Waits for the specified animation to complete playing.
    /// </summary>
    /// <param name="animator">The animator component controlling the animation.</param>
    /// <param name="animationName">The name of the animation state to wait for.</param>
    /// <returns>An enumerator for the coroutine.</returns>
    private IEnumerator WaitForAnimation(Animator animator, string animationName)
    {
        while (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Starts the boss fight by setting the boss to active state.
    /// </summary>
    public void StartBossFight()
    {
        inactive = false;
    }

    /// <summary>
    /// Handles the boss's death sequence.
    /// Sets the dead flag, invokes the defeat event, and destroys the boss game object.
    /// </summary>
    protected override void Die()
    {
        isDead = true;
        OnBossDefeated?.Invoke(this, System.EventArgs.Empty);
        Destroy(gameObject);
    }

    /// <summary>
    /// Applies damage to the boss and triggers the damage animation.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        bossAnimator.SetTrigger("Damage");
    }

    /// <summary>
    /// Flashes the boss's sprite red for the specified duration to indicate damage.
    /// </summary>
    /// <param name="duration">The duration of the flash effect in seconds.</param>
    protected override void FlashRed(float duration)
    {
        StartCoroutine(FlashCoroutine(duration));
    }

    /// <summary>
    /// Coroutine that alternates the sprite color between red and original to create a flash effect.
    /// </summary>
    /// <param name="duration">The total duration of the flash effect.</param>
    /// <returns>An enumerator for the coroutine.</returns>
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
                yield return new WaitForSeconds(0.1f);
                sr.color = original;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    /// <summary>
    /// Sets up the left and right movement constraints for the boss.
    /// </summary>
    /// <param name="rightConstraint">The game object representing the right boundary.</param>
    /// <param name="leftConstraint">The game object representing the left boundary.</param>
    public void SetupConstraints(GameObject rightConstraint, GameObject leftConstraint)
    {
        RightConstraint = rightConstraint;
        LeftConstraint = leftConstraint;
    }
    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
}