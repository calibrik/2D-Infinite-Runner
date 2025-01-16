using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioClip audio;
    public string key;
}
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    private Dictionary<string,AudioClip> _audioClips;
    private AudioSource _source;

    private void Start()
    {
        _source = GetComponent<AudioSource>();
        _audioClips = new Dictionary<string,AudioClip>();
        foreach (Sound sound in sounds) 
            _audioClips.Add(sound.key, sound.audio);
    }

    public void PlayOnKey(string key)
    {
        _source.clip = _audioClips[key];
        _source.Play();
    }
}
