using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMover : MonoBehaviour
{
    private RectTransform rectTransform;
    public float moveSpeed = 250f;

    private bool isBeingDestroyed = false;
    private bool hitRod = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        Debug.Log($"[Obstacle] {gameObject.name} started with tag {gameObject.tag}");

    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += Vector2.down * moveSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Obstacle] Triggered by: {other.name} (tag: {other.tag})");

        if (isBeingDestroyed) return;

        isBeingDestroyed = true; // ← Set this immediately to prevent multiple entries

        Debug.Log($"[Obstacle] Triggered by: {other.name}");

        if (other.CompareTag("Rod"))
        {
            Debug.Log("[Obstacle] Hit the rod!");
            hitRod = true;

            Set6RodAlignment rodScript = other.GetComponentInParent<Set6RodAlignment>();
            if (rodScript != null)
            {
                rodScript.OnRodHit();
            }
        }

        StartCoroutine(DelayedDestroy()); // Moved out of both blocks so it's always run once
    }


    private IEnumerator DelayedDestroy()
    {
        isBeingDestroyed = true;
        yield return null;

        if (hitRod)
            Debug.Log($"[Obstacle] Deactivating {gameObject.name} (from rod hit)");
        else
            Debug.Log($"[Obstacle] Deactivating {gameObject.name} (from destroyer)");

        gameObject.SetActive(false);

        // Return to pool only AFTER deactivation
        ObstaclePooler.Instance.ReturnToPool(gameObject);
    }


    void OnEnable()
    {
        isBeingDestroyed = false;
        hitRod = false;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }
}
