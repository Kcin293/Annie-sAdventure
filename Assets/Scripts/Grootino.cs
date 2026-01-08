using System.Collections;
using UnityEngine;

public class Grootino : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform player;
    [SerializeField] private float followSpeed = 10f;
    [SerializeField] private float launchForce = 8f;
    [SerializeField] private float returnDelay = 0.8f;
    [SerializeField] private Vector2 followOffset = new Vector2(0.5f, 0f);

    private bool isLaunched = false;
    private Coroutine currentCoroutine;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (!isLaunched)
        {
            Vector2 adjustedOffset = new Vector2(followOffset.x * player.GetComponentInChildren<Player>().GetDirection(), followOffset.y);
            Vector2 targetPos = (Vector2)player.position + adjustedOffset;
            Vector2 newPos = Vector2.Lerp(rb.position, targetPos, followSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }


    public void TryLaunch()
    {
        if (isLaunched) return;

        Launch(player.GetComponentInChildren<Player>().GetDirection());
    }

    private void Launch(float dir)
    {
        isLaunched = true;

        transform.SetParent(null);

        rb.AddForce(new Vector2(dir, 0f) * launchForce * -1, ForceMode2D.Impulse);

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ReturnAfterDelay(returnDelay));
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isLaunched = false;
        transform.SetParent(player);
        transform.localPosition = followOffset;
    }
}
