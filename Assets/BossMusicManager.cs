using System.Collections;
using UnityEngine;

public class BossMusicManager : MonoBehaviour
{
    [Header("Main Music")]
    public AudioSource firstPhaseAudio;
    public AudioSource secondPhaseAudio;
    public AudioSource finalPhaseAudio;
    
    [Header("Ambient & Effect Sounds")]
    public AudioSource ambientSound;           // Plays once when entering trigger zone
    public AudioSource jumpscareSound;         // Plays at specific spline starts (1, 3, 5)
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float firstSongVolume = 0.8f;
    [Range(0f, 1f)] public float secondSongVolume = 0.8f;
    [Range(0f, 1f)] public float finalSongVolume = 0.8f;
    [Range(0f, 1f)] public float ambientSoundVolume = 0.6f;
    [Range(0f, 1f)] public float jumpscareSoundVolume = 1.0f;
    
    [Header("Fade Settings")]
    public float fadeInDuration = 2.0f;
    public float fadeOutDuration = 2.0f;
    
    [Header("References")]
    public BossSplinePhaseManager splineManager;
    
    private bool isTransitioning = false;
    private Coroutine fadeCoroutine;
    private int lastProcessedPhase = -1;
    
    public void CheckAudioSetup()
    {
        Debug.Log("--- Audio Setup Check ---");
        Debug.Log("First Phase Audio: " + (firstPhaseAudio != null ? "Assigned" : "NULL"));
        Debug.Log("Second Phase Audio: " + (secondPhaseAudio != null ? "Assigned" : "NULL"));
        Debug.Log("Final Phase Audio: " + (finalPhaseAudio != null ? "Assigned" : "NULL"));
        Debug.Log("Ambient Sound: " + (ambientSound != null ? "Assigned" : "NULL"));
        Debug.Log("Jumpscare Sound: " + (jumpscareSound != null ? "Assigned" : "NULL"));
        
        Debug.Log("Spline Manager reference: " + (splineManager != null ? "Assigned" : "NULL"));
        Debug.Log("-------------------------");
    }

    void Start()
    {
        // Ensure all audio sources start muted
        if (firstPhaseAudio != null)
        {
            firstPhaseAudio.volume = 0;
            firstPhaseAudio.playOnAwake = false;
        }
        
        if (secondPhaseAudio != null)
        {
            secondPhaseAudio.volume = 0;
            secondPhaseAudio.playOnAwake = false;
        }
        
        if (finalPhaseAudio != null)
        {
            finalPhaseAudio.volume = 0;
            finalPhaseAudio.playOnAwake = false;
        }
        
        if (ambientSound != null)
        {
            ambientSound.volume = ambientSoundVolume;
            ambientSound.playOnAwake = false;
            ambientSound.loop = false;  // Not looping - just play once
        }
        
        if (jumpscareSound != null)
        {
            jumpscareSound.volume = jumpscareSoundVolume;
            jumpscareSound.playOnAwake = false;
            jumpscareSound.loop = false;  // One-shot sound
        }

        CheckAudioSetup();
    }
    
    // This method is called at the end of each phase
    public void OnPhaseComplete(int phaseIndex)
    {
        Debug.Log("OnPhaseComplete called with phase: " + phaseIndex);
        
        // Avoid processing the same phase multiple times
        if (phaseIndex == lastProcessedPhase) return;
        lastProcessedPhase = phaseIndex;
        
        HandleMusicTransition(phaseIndex);
    }
    
    // This is called when a new spline phase is about to start
    public void OnSplinePhaseStart(int phaseIndex)
    {
        Debug.Log("OnSplinePhaseStart called with phase: " + phaseIndex);
        
        // Play jumpscare sound at specific phase starts (1-indexed for clarity)
        if (phaseIndex == 0 || phaseIndex == 2 || phaseIndex == 4)  // First, third, fifth splines (0, 2, 4 in zero-indexed)
        {
            PlayJumpscareSound();
        }
    }
    
    public void HandleMusicTransition(int phaseIndex)
    {
        if (isTransitioning) return;
        
        switch (phaseIndex)
        {
            case 0: // First spline completed - first song should be playing
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeAudioSource(firstPhaseAudio, firstPhaseAudio.volume, firstSongVolume, fadeInDuration));
                break;
                
            case 1: // Second spline completed - fade out first song
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeAudioSource(firstPhaseAudio, firstPhaseAudio.volume, 0f, fadeOutDuration));
                break;
                
            case 2: // Third spline completed - second song should be playing
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                StartSecondSong();
                break;
                
            case 3: // Fourth spline completed - fade out second song
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeAudioSource(secondPhaseAudio, secondPhaseAudio.volume, 0f, fadeOutDuration));
                break;
                
            case 4: // Final spline completed - final song should be playing
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                StartFinalSong();
                break;
        }
    }
    
    public void StartFirstSong()
    {
        Debug.Log("Starting first song - Audio source exists: " + (firstPhaseAudio != null) + 
                 ", Already playing: " + (firstPhaseAudio != null && firstPhaseAudio.isPlaying) +
                 ", Target volume: " + firstSongVolume);
        
        if (firstPhaseAudio != null && !firstPhaseAudio.isPlaying)
        {
            firstPhaseAudio.Play();
            Debug.Log("First song started playing");
            fadeCoroutine = StartCoroutine(FadeAudioSource(firstPhaseAudio, 0f, firstSongVolume, fadeInDuration));
        }
    }
    
    public void StartSecondSong()
    {
        if (secondPhaseAudio != null && !secondPhaseAudio.isPlaying)
        {
            secondPhaseAudio.Play();
            fadeCoroutine = StartCoroutine(FadeAudioSource(secondPhaseAudio, 0f, secondSongVolume, fadeInDuration));
        }
    }
    
    public void StartFinalSong()
    {
        if (finalPhaseAudio != null && !finalPhaseAudio.isPlaying)
        {
            finalPhaseAudio.Play();
            fadeCoroutine = StartCoroutine(FadeAudioSource(finalPhaseAudio, 0f, finalSongVolume, fadeInDuration));
        }
    }
    
    public void PlayAmbientSound()
    {
        Debug.Log("Playing ambient sound (one-shot)");
        if (ambientSound != null)
        {
            // No fade needed for one-shot ambient sound - just play at set volume
            ambientSound.volume = ambientSoundVolume;
            ambientSound.Play();
        }
    }
    
    public void PlayJumpscareSound()
    {
        Debug.Log("Playing jumpscare sound");
        if (jumpscareSound != null)
        {
            jumpscareSound.volume = jumpscareSoundVolume;
            jumpscareSound.Play();
        }
    }
    
    private IEnumerator FadeAudioSource(AudioSource audioSource, float startVolume, float targetVolume, float duration)
    {
        if (audioSource == null) yield break;
        
        isTransitioning = true;
        float elapsedTime = 0f;
        
        // Ensure the audio source is playing if we're fading in
        if (targetVolume > 0 && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        audioSource.volume = startVolume;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            audioSource.volume = newVolume;
            yield return null;
        }
        
        audioSource.volume = targetVolume;
        
        // If we've faded to zero, stop the audio
        if (Mathf.Approximately(targetVolume, 0f) && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        isTransitioning = false;
    }
    
    // This is called when player enters the trigger zone
    public void OnTriggerEntered()
    {
        Debug.Log("OnTriggerEntered - Playing one-shot ambient sound");
        PlayAmbientSound();
    }
    
    // This method should be called from BossZoneTrigger when player enters zone
    public void InitiateBossMusic()
    {
        Debug.Log("InitiateBossMusic called");
        StartFirstSong();
    }
}