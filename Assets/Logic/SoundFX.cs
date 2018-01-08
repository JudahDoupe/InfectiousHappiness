using System.Collections.Generic;
using UnityEngine;

public class SoundFX : MonoBehaviour
{
    public static SoundFX Instance;

    private AudioSource Voice;

    public AudioClip Bounce;
    public List<AudioClip> Death;
    public List<AudioClip> Downer;
    public List<AudioClip> Drop;
    public AudioClip DropVoice;
    public List<AudioClip> Infect;
    public List<AudioClip> InfectCombo;
    public List<AudioClip> Jump;
    public List<AudioClip> Lift;
    public AudioClip LiftVoice;
    public List<AudioClip> Punch;
    public AudioClip PunchVoice;
    public List<AudioClip> Push;
    public List<AudioClip> Switch;

    void Start()
    {
        if (Instance == null)
            Instance = this;
        Voice = gameObject.AddComponent<AudioSource>();
    }

    public void PlayRandomClip(List<AudioClip> clips)
    {
        Voice.PlayOneShot(clips[Mathf.RoundToInt(Random.Range(0,clips.Count))]);
    }

    public void PlayClip(AudioClip clip)
    {
        Voice.PlayOneShot(clip);
    }

    public void PlayInfect(bool combo = false)
    {
        PlayRandomClip(combo ? InfectCombo : Infect);
    }
    public void PlaySwitch(int degree = 90)
    {
        Voice.PlayOneShot(degree == 90 ? Switch[0] : Switch[1]);
    }

}
