using UnityEngine;
using System.Collections;

public class RandomAmbiencePlayer : MonoBehaviour
{
    [Header("Ambience Settings")]
    public AudioClip[] ambienceClips;
    public AudioSource audioSource;

    [Header("Timing Settings")]
    public float minDelay = 5f;
    public float maxDelay = 15f;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayRandomAmbience());
    }

    private IEnumerator PlayRandomAmbience()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            if (ambienceClips.Length > 0)
            {
                AudioClip clip = ambienceClips[Random.Range(0, ambienceClips.Length)];
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
