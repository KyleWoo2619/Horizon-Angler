using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingZoneBob : MonoBehaviour
{
    float originalY;
    public float floatStrength;

    // Start is called before the first frame update
    void Start()
    {
        floatStrength = 0.75f;
        originalY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        
    }

    void Move()
    {
        transform.position = new Vector3(transform.position.x,
            originalY + ((float)Mathf.Sin(Time.time) * floatStrength),
            transform.position.z);
    }
}
