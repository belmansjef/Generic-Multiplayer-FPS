using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    // Singleton
    public static SoundManager instance;

    #region Public Fields
    
    public enum Sound
    {
        HitMarker,
        HitHurt,
        AkShot,
        SniperShot,
        AkReload,
        SniperReload,
        SniperBoltForward,
        SniperBoltBackward,
        confirmKill
    }
    
    [Header("Sounds:")]
    public SoundAudioClip[] SoundAudioClips;
    
    [Header("Mixers:")]
    public AudioMixer masterMixer;

    #endregion

    #region Private Fields

    private Dictionary<Sound, float> soundTimerDictionary;

    #endregion
    
    #region MonoBehaviour Callbacks

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    private void Start()
    {
        soundTimerDictionary = new Dictionary<Sound, float>();
        soundTimerDictionary[Sound.HitHurt] = 0f;
    }

    #endregion

    #region Public Methods

    public void PlaySound(Sound _sound, Vector3 _position)
    {
        if(CanPlaySound(_sound))
        {
            // Initialize object
            GameObject soundGameObject = new GameObject("Sound");
            AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
            AudioClip clip = GetAudioClip(_sound);
            soundGameObject.transform.position = _position;

            // Audio source properties
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 10f;
            audioSource.maxDistance = 75f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;

            // Mixer settings
            switch (_sound)
            {
                case Sound.AkShot:    
                    audioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
                case Sound.SniperShot:
                    audioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
            }

            // Play clip
            audioSource.clip = clip;
            audioSource.Play();
            Destroy(soundGameObject, clip.length);
        }
    }
    
    public void PlaySound(Sound _sound)
    {
        if(CanPlaySound(_sound))
        {
            GameObject oneShotGameObject = new GameObject("Sound");
            AudioSource oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
            AudioClip clip = GetAudioClip(_sound);
            oneShotGameObject.transform.parent = transform;

            // Mixer settings
            switch (_sound)
            {
                case Sound.AkReload:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
                case Sound.SniperReload:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
                case Sound.SniperBoltBackward:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
                case Sound.SniperBoltForward:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
                case Sound.AkShot:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
                case Sound.SniperShot:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
                case Sound.HitHurt:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("HitHurt")[0];
                    break;
                case Sound.HitMarker:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("HitMarkers")[0];
                    break;
                case Sound.confirmKill:
                    oneShotAudioSource.outputAudioMixerGroup = masterMixer.FindMatchingGroups("WeaponShots")[0];
                    break;
            }
            
            // Play clip
            oneShotAudioSource.PlayOneShot(clip);
            Destroy(oneShotGameObject, clip.length);
        }
    }

    #endregion

    #region Private Methods

    private AudioClip GetAudioClip(Sound _sound)
    {
        foreach (SoundAudioClip soundAudioClip in SoundAudioClips)
        {
            if (soundAudioClip.sound == _sound)
            {
                return soundAudioClip.audioClips[Random.Range(0, soundAudioClip.audioClips.Length)];
            }
        }
        Debug.LogError($"SoundManager: Sound {_sound} not found!");
        return null;
    }

    private bool CanPlaySound(Sound _sound)
    {
        switch (_sound)
        {
            default:
                return true;
            case Sound.HitHurt:
                if (soundTimerDictionary.ContainsKey(_sound))
                {
                    float lastTimePlayed = soundTimerDictionary[_sound];
                    float hurtTimerMax = .5f;
                    if (lastTimePlayed + hurtTimerMax < Time.time)
                    {
                        soundTimerDictionary[_sound] = Time.time;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
        }
    }

    #endregion
}

[System.Serializable]
public class SoundAudioClip
{
    public SoundManager.Sound sound;
    public AudioClip[] audioClips;
}