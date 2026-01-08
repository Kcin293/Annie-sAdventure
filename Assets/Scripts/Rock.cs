using UnityEngine;

public class Rock : MonoBehaviour
{
    public event System.Action OnRockFinished;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Boss"))
        {
            Boss boss = collision.GetComponentInParent<Boss>();
            boss.TakeDamage(1);
                OnRockFinished?.Invoke();
            Destroy(gameObject);
        }
    }

public bool IsDestroyed { get; private set; }


    // Update is called once per frame
    void Update()
    {
        
    }
}
