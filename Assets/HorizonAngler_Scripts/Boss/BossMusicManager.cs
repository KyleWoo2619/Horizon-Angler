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
    private AudioSource currentlyPlayingMusic = null;
    
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
        // Ensure all audio sources start muted and stopped
        InitializeAudioSources();
        CheckAudioSetup();
    }
    
    private void InitializeAudioSources()
    {
        // Initialize music tracks
        if (firstPhaseAudio != null)
        {
            firstPhaseAudio.volume = 0;
            firstPhaseAudio.playOnAwake = false;
            firstPhaseAudio.Stop();
        }
        
        if (secondPhaseAudio != null)
        {
            secondPhaseAudio.volume = 0;
            secondPhaseAudio.playOnAwake = false;
            secondPhaseAudio.Stop();
        }
        
        if (finalPhaseAudio != null)
        {
            finalPhaseAudio.volume = 0;
            finalPhaseAudio.playOnAwake = false;
            finalPhaseAudio.Stop();
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
        
        // Play jumpscare sound at specific phase starts (boss appears)
        // Using zero-indexed values (0, 2, 4 = first, third, fifth splines)
        if (phaseIndex == 0 || phaseIndex == 2 || phaseIndex == 4)
        {
            PlayJumpscareSound();
        }
    }
    
    public void HandleMusicTransition(int phaseIndex)
    {
        if (isTransitioning) return;
        
        Debug.Log($"Handling music transition for phase: {phaseIndex}");
        
        // Stop any existing fade coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        switch (phaseIndex)
        {
            case 0: // First spline completed - start first song
                Debug.Log("First spline completed - starting first phase music");
                FadeOutCurrentMusic();
                StartCoroutine(DelayedStartMusic(firstPhaseAudio, firstSongVolume, fadeOutDuration * 0.5f));
                break;
                
            case 1: // Second spline completed - fade out first song
                Debug.Log("Second spline completed - fading out first phase music");
                if (firstPhaseAudio != null && firstPhaseAudio.isPlaying)
                {
                    fadeCoroutine = StartCoroutine(FadeAudioSource(firstPhaseAudio, firstPhaseAudio.volume, 0f, fadeOutDuration));
                }
                break;
                
            case 2: // Third spline completed - transition to second song
                Debug.Log("Third spline completed - transitioning to second phase music");
                FadeOutCurrentMusic();
                StartCoroutine(DelayedStartMusic(secondPhaseAudio, secondSongVolume, fadeOutDuration * 0.5f));
                break;
                
            case 3: // Fourth spline completed - fade out second song
                Debug.Log("Fourth spline completed - fading out second phase music");
                if (secondPhaseAudio != null && secondPhaseAudio.isPlaying)
                {
                    fadeCoroutine = StartCoroutine(FadeAudioSource(secondPhaseAudio, secondPhaseAudio.volume, 0f, fadeOutDuration));
                }
                break;
                
            case 4: // Fifth spline completed - transition to final song
                Debug.Log("Fifth spline completed - transitioning to final phase music");
                FadeOutCurrentMusic();
                StartCoroutine(DelayedStartMusic(finalPhaseAudio, finalSongVolume, fadeOutDuration * 0.5f));
                break;
        }
    }

    // Helper method to fade out whatever music is currently playing
    private void FadeOutCurrentMusic()
    {
        // Check all music sources and fade out any that are playing
        if (firstPhaseAudio != null && firstPhaseAudio.isPlaying && firstPhaseAudio.volume > 0)
        {
            fadeCoroutine = StartCoroutine(FadeAudioSource(firstPhaseAudio, firstPhaseAudio.volume, 0f, fadeOutDuration));
        }
        
        if (secondPhaseAudio != null && secondPhaseAudio.isPlaying && secondPhaseAudio.volume > 0)
        {
            fadeCoroutine = StartCoroutine(FadeAudioSource(secondPhaseAudio, secondPhaseAudio.volume, 0f, fadeOutDuration));
        }
        
        if (finalPhaseAudio != null && finalPhaseAudio.isPlaying && finalPhaseAudio.volume > 0)
        {
            fadeCoroutine = StartCoroutine(FadeAudioSource(finalPhaseAudio, finalPhaseAudio.volume, 0f, fadeOutDuration));
        }
    }

    // Helper method for delayed song start
    private IEnumerator DelayedStartMusic(AudioSource audio, float targetVolume, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (audio != null)
        {
            // Update the current playing music reference
            currentlyPlayingMusic = audio;
            
            // Ensure it's not already playing
            if (!audio.isPlaying)
            {
                audio.Play();
                Debug.Log($"Started playing {audio.gameObject.name}");
            }
            
            fadeCoroutine = StartCoroutine(FadeAudioSource(audio, 0f, targetVolume, fadeInDuration));
        }
    }
    
    // Public methods for starting specific songs - used by InitiateBossMusic
    public void StartFirstSong()
    {
        Debug.Log("StartFirstSong called - but NOT starting first song yet (will start after first spline)");
        // We'll no longer start the first song here - it starts after the first spline completes
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
        Debug.Log("InitiateBossMusic called - only playing ambient sound, music will start after first spline");
        // We're not starting the first song right away anymore
        // We'll play it after the first spline completes
        PlayAmbientSound();
    }
}