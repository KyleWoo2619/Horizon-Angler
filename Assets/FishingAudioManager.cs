using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingAudioManager : MonoBehaviour
{
    public static FishingAudioManager Instance;

    [Header("Casting Sounds")]
    public AudioClip castingSound;
    
    [Header("Fishing Sounds")]
    public AudioClip reelInSound;
    public AudioClip waterSplashSound;
    
    [Header("Result Sounds")]
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip bossSound;
    public AudioClip waterOutSplashSound;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float castVolume = 0.7f;
    [Range(0f, 1f)]
    public float fishingVolume = 0.5f;
    [Range(0f, 1f)]
    public float resultVolume = 0.8f;
    [Range(0f, 1f)] public float waterOutVolume = 0.7f;
    
    private AudioSource mainAudioSource;
    private AudioSource loopingAudioSource;
    private AudioSource waterLoopAudioSource;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        mainAudioSource = gameObject.AddComponent<AudioSource>();
        loopingAudioSource = gameObject.AddComponent<AudioSource>();
        waterLoopAudioSource = gameObject.AddComponent<AudioSource>(); // << new

        loopingAudioSource.loop = true;
        loopingAudioSource.playOnAwake = false;

        waterLoopAudioSource.loop = true; // << important
        waterLoopAudioSource.playOnAwake = false;

        mainAudioSource.playOnAwake = false;
    }
    
    public void PlayCastSound()
    {
        PlayOneShot(castingSound, castVolume);
    }
    
    public void StartFishingSound()
    {
        // Start looping reel sound
        loopingAudioSource.clip = reelInSound;
        loopingAudioSource.volume = fishingVolume;
        loopingAudioSource.Play();

        // Start looping water splash sound
        waterLoopAudioSource.clip = waterSplashSound;
        waterLoopAudioSource.volume = fishingVolume * 0.7f; // softer
        waterLoopAudioSource.Play();
    }
    
    public void StopFishingSound()
    {
        if (loopingAudioSource.isPlaying)
            loopingAudioSource.Stop();
        if (waterLoopAudioSource.isPlaying)
            waterLoopAudioSource.Stop();
    }
    
    public void PlaySuccessSound()
    {
        PlayOneShot(successSound, resultVolume);
        PlayWaterOutSound(); // << Play water out after success
    }
    
    public void PlayFailSound()
    {
        PlayOneShot(failSound, resultVolume);
        PlayWaterOutSound(); // << Play water out after fail
    }
    
    public void PlayBossSound()
    {
        PlayOneShot(bossSound, resultVolume);
        PlayWaterOutSound(); // << Play water out after boss catch
    }

    public void PlayWaterOutSound()
    {
        PlayOneShot(waterOutSplashSound, waterOutVolume); // << Single shot water out sound
    }
    
    private void PlayOneShot(AudioClip clip, float volume)
    {
        if (clip != null && mainAudioSource != null)
        {
            mainAudioSource.PlayOneShot(clip, volume);
        }
    }
}