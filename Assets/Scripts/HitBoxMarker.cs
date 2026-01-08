using UnityEngine;

public class HitBoxMarker : MonoBehaviour
{
    [SerializeField] float knockbackForce = 25f;

    private Boss boss;
    private void Start()
    {
        GameObject parent = transform.parent.gameObject;
        boss = parent.GetComponent<Boss>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
              Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // Calcola direzione orizzontale per spingere lontano dal nemico
        Vector2 direction;
        if (collision.transform.position.x < transform.position.x)
        {
            direction = Vector2.left; // Player a destra, spingi a sinistra
        }
        else
        {
            direction = Vector2.right; // Player a sinistra, spingi a destra
        }

        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.TakeKnockback(direction, knockbackForce);
            if(gameObject.CompareTag("Boss"))
            {
                boss.TakeDamage(1);
            }
            else
            {
                            player.TakeDamage(1);

            }
        }
    }
    }
}
