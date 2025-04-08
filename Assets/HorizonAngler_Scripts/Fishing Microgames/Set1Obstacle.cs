using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Set1Obstacle : MonoBehaviour
{
    public RectTransform heartTx;
    private Rigidbody2D rb;
    private Vector2 heartPos, dest;
    private bool touched = false; // To prevent double bonus/penalty

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveToHeart()
    {
        heartPos = heartTx.position;
        dest = rb.position;
        StartCoroutine(UntouchedTimer());
    }

    private void FixedUpdate()
    {
        Vector2 velo = (heartPos - dest) * Time.fixedDeltaTime;
        rb.velocity = velo * 75;
    }

    IEnumerator UntouchedTimer()
    {
        yield return new WaitForSeconds(1.5f);
        if (!touched)
        {
            Untouched();
        }
    }

    void Untouched()
    {
        Disable();
    }

    void Disable()
    {
        this.gameObject.SetActive(false);
        rb.velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!touched)
        {
            touched = true;
            Disable();
            FishingProgress.Instance.Set1ObstaclePenalty(); // Call penalty on collision
        }
    }
}
