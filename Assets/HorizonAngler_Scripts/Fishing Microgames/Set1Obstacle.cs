using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Set1Obstacle : MonoBehaviour
{
    public RectTransform heartTx;
    private Rigidbody2D rb;
    private Vector2 heartPos, dest;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveToHeart()
    {
        //Debug.Log("MoveToHeart called by " + this.gameObject);
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
        Untouched();
    }

    void Disable()
    {
        this.gameObject.SetActive(false);
        rb.velocity = Vector2.zero;
    }

    void Untouched()
    {
        Disable();
        // Bonus to progress bar
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Penalty to progress bar
        //if (collision.tag == "Player")
        //{
            //Debug.Log(this.gameObject.name + " got hit");
            Disable();
        //}
    }
}
