using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f; // 0 = lontano, 1 = vicino

    private Transform cam;
    private Vector3 startLocalPos;
    private float spriteWidth;
    private float pixelsPerUnit;

    void Start()
    {
        cam = Camera.main.transform;

        // Usa la posizione locale rispetto al parent
        startLocalPos = transform.localPosition;

        // Calcola la larghezza in Unity Units considerando il PPU
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        pixelsPerUnit = sr.sprite.pixelsPerUnit; 
        spriteWidth = sr.sprite.rect.width / pixelsPerUnit;
    }

    void LateUpdate()
    {
        // Movimento parallax
        Vector3 newPos = startLocalPos;
         // Pixel snap
         float ppu = pixelsPerUnit; // uguale al PPU della tua Pixel Perfect Camera
    
        float temp = newPos.x + (cam.position.x * parallaxFactor);
        newPos.x = Mathf.Round(temp * ppu) / ppu;
        newPos.y = cam.position.y;
        transform.localPosition = newPos;

        // Loop
        float distance = cam.position.x -  transform.localPosition.x;
        // Soglia basata sul parallax per evitare problemi layer vicini
        float loopThreshold = (spriteWidth * 1.1f);

        if (distance >= loopThreshold)
        {
            startLocalPos.x += spriteWidth * 2; // sposta dietro l'altra copia
        }
        else if (distance <= -loopThreshold)
        {
            startLocalPos.x -= spriteWidth * 2; // sposta dietro l'altra copia verso sinistra
        }
    }
}
