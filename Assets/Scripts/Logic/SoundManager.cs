using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager Instance;

    public AudioSource audioSource;
    public AudioClip capturedSound;

       public void Awake()
    {
        Instance = this;
    }

    
    public void PlayCapturedSound()
    {
        if (capturedSound != null)
            audioSource.PlayOneShot(capturedSound);
    }
}
