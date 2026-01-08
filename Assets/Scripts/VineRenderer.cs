using System.Collections.Generic;
using UnityEngine;

public class VineRenderer : MonoBehaviour
{
    [SerializeField] float segmentSize = 0.16f;
    [SerializeField] List<GameObject> segmentObjects = new List<GameObject>();
    [SerializeField] GameObject segmentPrefab;
    [SerializeField] GameObject pointerPrefab;
    private GameObject pointerInstance;

    void Start()
    {
        if (pointerPrefab != null)
        {
            pointerInstance = Instantiate(pointerPrefab);
            pointerInstance.transform.parent = this.transform;
            pointerInstance.SetActive(false);
        }
    }

    public void UpdateLineRenderer(Vector3 playerPos, Vector3 hookPoint)
    {
        pointerInstance.SetActive(true);
        Vector2 dir = (hookPoint - playerPos).normalized;
        float distance = Vector2.Distance(playerPos, hookPoint);
        int segmentCount = Mathf.FloorToInt(distance / segmentSize);
        
        for (int i = 0; i < segmentCount; i++)
        {
            if (i >= segmentObjects.Count)
            {
                segmentObjects.Add(Instantiate(segmentPrefab));
                segmentObjects[i].transform.parent = this.transform;
            }

            Vector2 pos = Vector2.Lerp(playerPos, hookPoint, (float)i / segmentCount);
            segmentObjects[i].transform.position = pos;
            segmentObjects[i].transform.right = dir;
            segmentObjects[i].SetActive(true);
        }

        for (int i = segmentCount; i < segmentObjects.Count; i++)
        {
            segmentObjects[i].SetActive(false);
        }
        

        if (pointerInstance != null)
        {
            pointerInstance.transform.position = hookPoint;
            pointerInstance.transform.right = dir;
        }
    }

    public Vector2 GetPointerPosition()
    {
        if (pointerInstance != null)
        {
            return pointerInstance.transform.position;
        }
        return Vector2.zero;
    }

    public void HidePointer()
    {
        if (pointerInstance != null)
        {
            pointerInstance.SetActive(false);
        }
    }
}