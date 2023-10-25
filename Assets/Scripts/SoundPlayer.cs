using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip clipSource;
    public AudioClip[] audioClips;
    public float currentPitchMin=1;
    public float currentPitchMax=1;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (clipSource)
        {
            audioSource.clip = clipSource;
        }
        
    }
    /// <summary>
    /// 外部调用
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="pitchMin"></param>
    /// <param name="pitchMax"></param>
    public void PlayClip(AudioClip audioClip,float pitchMin=1,float pitchMax=1)
    {
        audioSource.pitch = Random.Range(pitchMin,pitchMax);
        audioSource.PlayOneShot(audioClip);  
    }
    public void PlayRandomSound()
    {
        audioSource.pitch = Random.Range(currentPitchMin, currentPitchMax);
        audioSource.PlayOneShot(audioClips[Random.Range(0,audioClips.Length)]);
    }

    public void PlaySound(bool loop=false)
    {
        if (audioSource.clip!=null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.loop = loop;
                audioSource.Play();
            }
        }            
    }
}
