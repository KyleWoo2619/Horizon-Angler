using UnityEngine;

public class LightningController : MonoBehaviour
{
    [System.Serializable]
    public class LightningObject
    {
        public GameObject obj;
        [HideInInspector] public float timer;
        [HideInInspector] public bool isFlashing;
    }

    [Header("Lightning Settings")]
    public LightningObject[] lightningObjects;
    public float minDelay = 3.0f;
    public float maxDelay = 8.0f;
    public float flashDuration = 0.2f;

    void Start()
    {
        foreach (var lightning in lightningObjects)
        {
            lightning.timer = Random.Range(minDelay, maxDelay);
            lightning.isFlashing = false;

            if (lightning.obj != null)
                lightning.obj.SetActive(false);
        }
    }

    void Update()
    {
        foreach (var lightning in lightningObjects)
        {
            if (lightning.obj == null) continue;

            lightning.timer -= Time.deltaTime;

            if (lightning.timer <= 0f && !lightning.isFlashing)
            {
                StartCoroutine(FlashLightning(lightning));
            }
        }
    }

    System.Collections.IEnumerator FlashLightning(LightningObject lightning)
    {
        lightning.isFlashing = true;

        lightning.obj.SetActive(true);

        // Play sound if it has an AudioSource
        AudioSource audio = lightning.obj.GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.Play();
        }

        yield return new WaitForSeconds(flashDuration);

        lightning.obj.SetActive(false);

        lightning.timer = Random.Range(minDelay, maxDelay);
        lightning.isFlashing = false;
    }

}
