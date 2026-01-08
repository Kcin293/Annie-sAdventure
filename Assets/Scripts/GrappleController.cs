using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// Manages the grappling hook mechanics for the player.
/// Handles aiming, launching, maintaining, and retracting the vine.
/// Supports grappling to terrain for swinging and to enemies for damage.
/// Integrates with VineRenderer for visual effects and DistanceJoint2D for physics.
/// </summary>
public class GrappleController : MonoBehaviour
{
    /// <summary>
    /// Maximum distance the grapple can reach.
    /// </summary>
    [SerializeField] float maxGrappleDistance = 10f;

    /// <summary>
    /// Prefab for the aiming reticle or pointer.
    /// </summary>
    [SerializeField] GameObject AimPrefab;

    /// <summary>
    /// Component responsible for rendering the vine visually.
    /// </summary>
    [SerializeField] VineRenderer vineRenderer;

    /// <summary>
    /// Flag indicating if the grapple hit something on the last attempt.
    /// </summary>
    bool hitSomething = false;

    /// <summary>
    /// The collider that was hit by the grapple raycast.
    /// </summary>
    Collider2D hitCollider = null;

    /// <summary>
    /// Cached raycast hit for the current aim.
    /// </summary>
    RaycastHit2D cachedHit;

    /// <summary>
    /// Flag indicating if the cached hit is valid.
    /// </summary>
    bool cachedHitSomething;

    /// <summary>
    /// Rigidbody2D of the player for applying forces.
    /// </summary>
    Rigidbody2D rb;

    /// <summary>
    /// Current coroutine handling vine extension/retraction.
    /// </summary>
    private Coroutine vineRoutine;

    /// <summary>
    /// Flag indicating if grappling is currently allowed.
    /// </summary>
    private bool canGrapple = true;

    /// <summary>
    /// Cooldown duration after stopping a grapple.
    /// </summary>
    private float grappleCooldown = 0.15f;

    /// <summary>
    /// Speed at which the vine extends or retracts.
    /// </summary>
    public float vineSpeed = 12f;

    /// <summary>
    /// The raycast hit result for the grapple target.
    /// </summary>
    RaycastHit2D hitTarget;

    /// <summary>
    /// The target position for the vine end.
    /// </summary>
    Vector2 targetPosition;

    /// <summary>
    /// Current position of the vine end during animation.
    /// </summary>
    Vector2 currentEnd;

    /// <summary>
    /// Transform of the player object.
    /// </summary>
    private Transform player;

    /// <summary>
    /// Distance joint used for maintaining the grapple connection.
    /// </summary>
    [SerializeField] DistanceJoint2D grappleJoint;

    /// <summary>
    /// Initializes the grapple controller.
    /// Sets up references to player and rigidbody, and disables the joint initially.
    /// </summary>
    void Start()
    {
        player = (transform.parent != null) ? transform.parent : transform;
        grappleJoint.enabled = false;
        rb = player.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Updates the grapple aim every frame.
    /// </summary>
    // Update is called once per frame
    void Update()
    {
        Aim();

    }

    /// <summary>
    /// Handles the aiming logic for the grapple.
    /// Determines the target position based on mouse
    /// Performs raycast to check for valid grapple points and caches the result.
    /// </summary>
    private void Aim()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 screenPos = Vector2.zero;
        if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch != null)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            screenPos = Input.mousePosition;
        }

        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
        worldPos.z = 0f;

        if (AimPrefab != null)
            AimPrefab.transform.position = worldPos;
        Vector2 direction = (worldPos - player.position).normalized;

        int layerMask = LayerMask.GetMask("Player", "Ignore Raycast");
        RaycastHit2D hit = Physics2D.Raycast(
            player.position,
            direction,
            maxGrappleDistance,
            ~layerMask
        );


        if (hit.collider != null)
        {
            Debug.Log("Hit: " + hit.collider.name);
            cachedHit = hit;
            cachedHitSomething = true;
            hitCollider = hit.collider;  // save the collider for damage
            targetPosition = hit.point;
        }
        else
        {
            cachedHit = default;
            hitCollider = null;
            cachedHitSomething = false;
            targetPosition = (Vector2)player.position + direction * maxGrappleDistance;
        }
        Debug.DrawLine(player.position, targetPosition, hit.collider != null ? Color.green : Color.red);

    }

    /// <summary>
    /// Initiates the grapple action.
    /// Checks cooldown, sets up hit data, and starts the vine extension coroutine.
    /// </summary>
    public void StartGrapple()
    {
        // Prevent re-firing immediately after jump/stop
        if (!canGrapple) return;

        hitTarget = cachedHit;
        hitSomething = cachedHitSomething;  // reset before each grapple attempt
        hitCollider = null;
        if (vineRoutine != null)
            StopCoroutine(vineRoutine);

        vineRoutine = StartCoroutine(ExtendVine(targetPosition));
    }

    /// <summary>
    /// Stops the current grapple.
    /// Starts vine retraction and applies cooldown.
    /// </summary>
    public void StopGrapple()
    {
        if (vineRoutine != null)
            StopCoroutine(vineRoutine);

        vineRoutine = StartCoroutine(RetractVine());

        // Set cooldown to prevent immediate re-grapple
        StartCoroutine(GrappleCooldown());
    }

    /// <summary>
    /// Coroutine to extend the vine towards the target position.
    /// Updates the vine renderer and handles hit logic (damage to enemies or maintain grapple).
    /// </summary>
    /// <param name="target">The target position to extend the vine to.</param>
    IEnumerator ExtendVine(Vector3 target)
    {
        currentEnd = player.position;
        vineRenderer.enabled = true;

        while (Vector3.Distance(currentEnd, target) > 0.1f)
        {
            vineRenderer.UpdateLineRenderer(player.position, currentEnd);
            currentEnd = Vector3.MoveTowards(currentEnd, target, vineSpeed * Time.deltaTime);
            yield return null;
        }
        currentEnd = target;  // snap to exact hit point
        vineRenderer.UpdateLineRenderer(player.position, currentEnd);

        if (hitSomething)
        {
            if (hitTarget.collider != null && hitTarget.collider.CompareTag("Enemy"))
            {
                Enemy enemy = hitTarget.collider.GetComponentInParent<Enemy>();
                enemy.TakeDamage(1);
                yield return new WaitForSeconds(0.1f);
                vineRoutine = StartCoroutine(RetractVine());
            }
            else
                vineRoutine = StartCoroutine(MantainVine());
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            vineRoutine = StartCoroutine(RetractVine());
        }
    }

    /// <summary>
    /// Coroutine to maintain the vine connection after a successful grapple.
    /// Enables the distance joint, applies downward force, and keeps the player under the anchor.
    /// Handles damage to enemies if applicable.
    /// </summary>
    IEnumerator MantainVine()
    {
        grappleJoint.enabled = true;
        player.GetComponent<Player>().SetGrappleState(true);
        grappleJoint.connectedAnchor = currentEnd;
        grappleJoint.distance = Vector2.Distance(player.position, currentEnd);
        grappleJoint.autoConfigureDistance = false;

        // Check if we hit an enemy and apply damage
        if (hitCollider != null && hitCollider.CompareTag("Enemy"))
        {
            hitSomething = false;  // stop maintaining after damage
        }
        else
        {
            // Normal grapple on terrain
            while (hitSomething)
            {
                if (rb != null)
                    rb.AddForce(Vector2.down * 12f, ForceMode2D.Force);
                KeepPlayerUnderAnchor();
                vineRenderer.UpdateLineRenderer(player.position, currentEnd);
                yield return null;
            }
        }

        // restore rotation freedom
        if (rb != null)
            rb.freezeRotation = false;
        grappleJoint.enabled = false;
    }

    /// <summary>
    /// Coroutine to retract the vine back to the player.
    /// Updates the vine renderer and disables components when complete.
    /// </summary>
    IEnumerator RetractVine()
    {
        hitSomething = false;
        currentEnd = vineRenderer.GetPointerPosition();

        while (Vector3.Distance(currentEnd, player.position) > 0.1f)
        {
            currentEnd = Vector3.MoveTowards(currentEnd, player.position, vineSpeed * Time.deltaTime);
            vineRenderer.UpdateLineRenderer(player.position, currentEnd);
            yield return null;
        }
        grappleJoint.enabled = false;
        vineRenderer.HidePointer();
        vineRenderer.enabled = false;
    }

    /// <summary>
    /// Ensures the player stays under the grapple anchor to prevent dangerous swinging.
    /// Applies downward force if the player is above the anchor.
    /// </summary>
    void KeepPlayerUnderAnchor()
    {
        Vector2 toPlayer = (Vector2)player.position - currentEnd;
        float dot = Vector2.Dot(toPlayer.normalized, Vector2.down);

        // dot sarà > 0 solo se il player sta sopra l'anchor (pericoloso)
        if (dot < 0)
        {
            // forza verso il basso o tangenziale che lo fa tornare sotto
            rb.AddForce(Vector2.down * 15f, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// Coroutine to handle the grapple cooldown period.
    /// Prevents immediate re-grappling after stopping.
    /// </summary>
    IEnumerator GrappleCooldown()
    {
        canGrapple = false;
        yield return new WaitForSeconds(grappleCooldown);
        canGrapple = true;
    }

}
