using UnityEngine;

/// <summary>
/// Base class for all interactable objects in the game.
/// This class provides the foundation for objects that players can interact with.
/// Subclasses should override the Interact() method to implement specific interaction behavior.
/// Handles trigger detection for player proximity and manages input subscriptions.
/// </summary>

public class Interactable : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the player is currently within interaction range of this object.
    /// Used to prevent interactions when the player is too far away.
    /// </summary>
    protected bool isInRange = false;

    /// <summary>
    /// Reference to the player's input subscription component.
    /// Kept protected so derived classes can access and subscribe to additional events if needed.
    /// </summary>
    // Keep a reference to the player's input subscription while in range
    // Protected so derived classes (eg. PrepStation) can access and subscribe to additional events
    protected PlayerInputSubscription playerInputSubscription;

    /// <summary>
    /// Virtual method that defines the interaction behavior.
    /// Subclasses should override this method to provide custom interaction logic.
    /// By default, logs a message indicating no specific function is implemented.
    /// </summary>
    // Metodo virtuale: le sottoclassi lo sovrascrivono con la loro logica
    public virtual void Interact()
    {
        Debug.Log($"{gameObject.name} è stato interagito, ma non ha una funzione specifica.");
    }


    /// <summary>
    /// Called when a collider enters this object's trigger collider.
    /// If the entering object is the player, sets up interaction by subscribing to input events.
    /// Updates the player's in-range status and stores the input subscription reference.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    // Quando il player entra nel trigger, ci sottoscriviamo al suo evento di input
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        isInRange = true;
        Player player = other.GetComponent<Player>();
        player.IsInRange = isInRange;
        var pis = other.GetComponent<PlayerInputSubscription>();
        if (pis != null)
        {
            pis.OnInteractPressed += OnPlayerInteract;
        }
    }

    /// <summary>
    /// Called when a collider exits this object's trigger collider.
    /// If the exiting object is the player, cleans up by unsubscribing from input events.
    /// Updates the player's in-range status to prevent further interactions.
    /// </summary>
    /// <param name="other">The collider that exited the trigger.</param>
    // Quando il player esce, desottoscriviamoci per evitare memory leak
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        isInRange = false;
        Player player = other.GetComponent<Player>();
        player.IsInRange = isInRange;
        var pis = other.GetComponent<PlayerInputSubscription>();
        if (pis != null)
        {
            pis.OnInteractPressed -= OnPlayerInteract;
        }
    }


    /// <summary>
    /// Event handler called when the player presses the interact button.
    /// Checks if the object is still valid and in range before executing the interaction.
    /// Wraps the Interact() call in a try-catch block to handle potential errors gracefully.
    /// </summary>
    private void OnPlayerInteract()
    {
        // Safety check: if this object has been destroyed or is out of range, do nothing
        if (this == null || !isInRange)
            return;
        try
        {
            Interact();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Errore durante Interact() su {gameObject.name}: {ex.Message}");
        }
    }

}
