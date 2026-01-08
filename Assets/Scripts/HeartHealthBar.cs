using UnityEngine;
using UnityEngine.UI;

public class HeartHealthBar : MonoBehaviour
{
    [SerializeField] private Image[] hearts; // Drag & drop dei cuori nell'Inspector
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;

    public void UpdateHearts(int currentHealth, int maxHealth)
    {
        float healthPerHeart = maxHealth / hearts.Length;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (currentHealth >= (i + 1) * healthPerHeart)
            {
                hearts[i].sprite = fullHeart;
            }
            else if (currentHealth >= i * healthPerHeart + healthPerHeart / 2f)
            {
                hearts[i].sprite = halfHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}
