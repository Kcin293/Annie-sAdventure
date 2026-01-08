using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// This script manages the pickup behavior for Grootino items in the game.
/// When the player interacts with this object, it equips Grootino to the player and removes itself from the scene.
/// This class inherits from Interactable to handle interaction logic.
/// </summary>
public class GrootinoPickUp : Interactable
{
    /// <summary>
    /// Reference to the player's collider, stored when the player enters the trigger zone.
    /// Used to access the Player component when interacting.
    /// </summary>
    private Collider2D playerCollider;

    /// <summary>
    /// Handles the interaction when the player picks up Grootino.
    /// Logs the pickup event, equips Grootino to the player, and destroys this game object.
    /// </summary>
    public override void Interact()
    {
        Debug.Log("Grootino picked up!");
        Player player = playerCollider.GetComponentInParent<Player>();
        player.SetGrootino();
        Destroy(gameObject);
    }

    /// <summary>
    /// Called when another collider enters this object's trigger collider.
    /// Stores a reference to the player's collider if the entering object is tagged as "Player".
    /// Calls the base class method to handle additional trigger enter logic.
    /// </summary>
    /// <param name="collision">The collider that entered the trigger.</param>
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.CompareTag("Player"))
        {
            playerCollider = collision;
        }
    }
}
