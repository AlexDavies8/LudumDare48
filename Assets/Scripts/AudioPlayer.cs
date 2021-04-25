using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource _source = null;

    public void Play(AudioClip clip)
    {
        _source.PlayOneShot(clip);
    }
    public void Play(AudioClip clip, float volume = 1f)
    {
        _source.PlayOneShot(clip, volume);
    }
    public void PlayRandom(params AudioClip[] clips)
    {
        _source.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
    public void PlayRandom(float volume, params AudioClip[] clips)
    {
        _source.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }
}
