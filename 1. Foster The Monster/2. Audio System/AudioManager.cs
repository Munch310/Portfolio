using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static Enums;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private const float MIN_VOLUME = 0.0001f;
    private const float MAX_VOLUME = 1f;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioSource bgmSource;

    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private const int INITIAL_POOL_SIZE = 5;

    protected override void Awake()
    {
        base.Awake();
        InitializeMixerGroup();
        InitializeSFXPool();
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeMixerGroup()
    {
        bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        bgmSource.volume = 0.5f;
    }

    private void InitializeSFXPool()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            AddAudioSourceToPool();
        }
    }

    private void AddAudioSourceToPool()
    {
        AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.5f;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        sfxPool.Enqueue(sfxSource);
    }

    public void PlayBGM(BGMClips bgmClips)
    {
        string clipName = bgmClips.ToString();
        AudioClip clip = ResourceManager.Instance.LoadPrefab<AudioClip>($"AudioClip/BGM/{clipName}", clipName);
        if (clip)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }


    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }
    }

    public void PlaySFX(SFXCategory category, SFXClips clip)
    {
        string categoryName = category.ToString();
        string clipName = clip.ToString();
        AudioClip audioClip = ResourceManager.Instance.LoadPrefab<AudioClip>($"AudioClip/{categoryName}/SFX/{clipName}", clipName);
        if (audioClip)
        {
            PlayAudioClip(audioClip);
        }
    }

    private void PlayAudioClip(AudioClip clip)
    {
        if (sfxPool.Count == 0)
        {
            AddAudioSourceToPool();
        }

        AudioSource audioSource = sfxPool.Dequeue();
        audioSource.clip = clip;
        audioSource.Play();
        StartCoroutine(ReturnToPoolAfterPlay(audioSource));
    }

    private IEnumerator ReturnToPoolAfterPlay(AudioSource source)
    {
        while (source.isPlaying)
        {
            yield return null;
        }
        sfxPool.Enqueue(source);
    }
    public float GetVolume(string parameterName)
    {
        if (mixer.GetFloat(parameterName, out float value))
        {
            return Mathf.Pow(10, value / 20);
        }
        return 0f;
    }

    private void SetVolume(string parameterName, float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);
        mixer.SetFloat(parameterName, Mathf.Log10(value) * 20);
    }

    public void MasterSoundVolume(float val)
    {
        SetVolume("Master", val);
        PlayerPrefs.SetFloat("MasterVolume", val);
    }

    public void BGMSoundVolume(float val)
    {
        SetVolume("BGM", val);
        PlayerPrefs.SetFloat("BGMVolume", val);
    }

    public void SFXSoundVolume(float val)
    {
        SetVolume("SFX", val);
        PlayerPrefs.SetFloat("SFXVolume", val);
    }
}