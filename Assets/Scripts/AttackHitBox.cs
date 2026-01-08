/// <summary>
/// The AttackHitBox class manages collision detection for attack areas, applying damage to valid targets such as seeds and the player.
/// This script is typically attached to attack hitbox game objects to handle damage dealing logic.
/// </summary>
using UnityEngine;

/// <summary>
/// Represents an attack hitbox that deals damage upon collision.
/// </summary>
public class AttackHitBox : MonoBehaviour
{
    /// <summary>
    /// The amount of damage to deal to targets upon collision.
    /// </summary>
    [SerializeField] private int damage = 1;

    /// <summary>
    /// Called when another collider enters this trigger collider.
    /// Checks the tag of the colliding object and applies damage if it's a valid target.
    /// </summary>
    /// <param name="collision">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object is a seed
        if (collision.CompareTag("Seed"))
        {
            collision.GetComponent<Seed>().TakeDamage();
        }
        // Check if the collided object is the player
        if (collision.CompareTag("Player"))
        {
            Player target = collision.GetComponent<Player>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }
}
