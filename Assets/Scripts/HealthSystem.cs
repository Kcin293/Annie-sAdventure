using UnityEngine;
using System;

/// <summary>
/// Base class for managing health in game entities.
/// Provides functionality for taking damage, healing, death handling, and temporary invincibility.
/// Designed to be inherited by player and enemy classes for customized behavior.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    /// <summary>
    /// Maximum health points for this entity.
    /// </summary>
    [SerializeField] protected int maxHealth = 3;

    /// <summary>
    /// Current health points remaining.
    /// </summary>
    [SerializeField] protected int currentHealth;

    /// <summary>
    /// Flag indicating if the entity is dead.
    /// </summary>
    protected bool isDead = false;

    /// <summary>
    /// Flag indicating if the entity is currently invincible to damage.
    /// </summary>
    protected bool isInvincible = false;

    /// <summary>
    /// Initializes the health system by setting current health to maximum.
    /// </summary>
    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage to the entity.
    /// Reduces current health, triggers visual feedback, and starts invincibility.
    /// Calls Die() if health reaches zero.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public virtual void TakeDamage(int damage)
    {
        if (currentHealth <= 0 || isInvincible) return;  // already dead or invincible
        
        currentHealth -= damage;        
        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}");
        FlashRed(0.8f);
        StartInvincibility(0.8f);  // 1 second invincibility
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Restores health to the entity, up to the maximum.
    /// </summary>
    /// <param name="amount">Amount of health to restore.</param>
    public virtual void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} healed for {amount}. Health: {currentHealth}");
    }

    /// <summary>
    /// Handles the death of the entity.
    /// Sets the dead flag and logs the event.
    /// Can be overridden for custom death behavior.
    /// </summary>
    protected virtual void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} died.");
    }

    /// <summary>
    /// Gets the current health value.
    /// </summary>
    /// <returns>Current health points.</returns>
    public int GetCurrentHealth() => currentHealth;

    /// <summary>
    /// Gets the maximum health value.
    /// </summary>
    /// <returns>Maximum health points.</returns>
    public int GetMaxHealth() => maxHealth;

    /// <summary>
    /// Checks if the entity is alive.
    /// </summary>
    /// <returns>True if current health is greater than zero.</returns>
    public bool IsAlive() => currentHealth > 0;

    /// <summary>
    /// Starts the invincibility period after taking damage.
    /// </summary>
    /// <param name="duration">Duration of invincibility in seconds.</param>
    protected void StartInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    /// <summary>
    /// Coroutine that manages the invincibility timer.
    /// Sets invincibility flag and waits for the duration.
    /// </summary>
    /// <param name="duration">Duration to remain invincible.</param>
    private System.Collections.IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }

    /// <summary>
    /// Virtual method for visual feedback when taking damage (e.g., flashing red).
    /// Can be overridden in derived classes for specific implementations.
    /// </summary>
    /// <param name="duration">Duration of the flash effect.</param>
    protected virtual void FlashRed(float duration) { }
}