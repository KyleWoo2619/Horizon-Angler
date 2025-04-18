using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform), typeof(BoxCollider2D))]
public class SyncBoxColliderToRect : MonoBehaviour
{
    private RectTransform rectTransform;
    private BoxCollider2D boxCollider;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (rectTransform == null || boxCollider == null) return;

        // Match collider size to rect size (in local UI units)
        boxCollider.size = rectTransform.rect.size;
        boxCollider.offset = rectTransform.rect.center;
    }
}
