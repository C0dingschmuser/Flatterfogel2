using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public enum MusicID
{
    Menu,
    Track1,
    Track2,
    Mining,
    Boss,
}

public enum Sound
{
    Blus,
    BlusExplosion,
    Jump,
    Die,
    MinusShoot,
    Laser,
    PipeExplosion,
    PipeHit,
    DModeExplosion,
    LevelUp,
    MenuSelect,
    MenuSelectEnd,
    MenuError,
}

public class SoundManager : MonoBehaviour
{
    public AudioClip[] blus_simple;
    public AudioClip[] blus_explosion;
    public AudioClip[] jump;
    public AudioClip[] die;
    public AudioClip[] minusShoot;
    public AudioClip dPipe_explosion;
    public AudioClip dPipe_explosion_hit;
    public AudioClip laserSoundClip;
    public AudioClip levelUp;
    public AudioClip menuSelect;
    public AudioClip menuSelectEnd;
    public AudioClip menuError;
    public AudioClip[] dMode_Explosion;
    public AudioClip[] music;

    public float musicVolume = 1, effectVolume = 1;
    public AudioClip origJump, origBlus, origDie;

    public static SoundManager Instance;

    [SerializeField]
    private AudioSource musicObj = null;

    private GameObject laserSound = null;

    private ObjectPooler pooler;
    private MusicID nextMusicID;

    /*public float updateStep = 0.05f, lastBeat = -1f, lastVol = 0;
    public int sampleDataLength = 1024;
    private float currentUpdateTime = 0f;
    private float clipLoudness;
    private float[] clipSampleData;*/

    private void Awake()
    {
        Instance = this;
        pooler = ObjectPooler.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        //clipSampleData = new float[sampleDataLength];
        PlayMusic(MusicID.Menu);
    }

    public void PlayMusicFade(MusicID id, float fadeTime = 0.5f)
    {
        if (musicVolume < 0.01) return;

        DOTween.To(() => musicObj.volume, x => musicObj.volume = x, 0f, fadeTime);

        nextMusicID = id;
        Invoke(nameof(MusicBypass), fadeTime + 0.025f);
    }

    private void MusicBypass()
    {
        PlayMusic(nextMusicID);
    }

    public void SetMusicSpeed(float speed, bool tween = false, bool menu = false)
    {
        if(!tween)
        {
            musicObj.pitch = speed;
        } else
        {
            DOTween.To(() => musicObj.pitch, x => musicObj.pitch = x, speed, 0.25f);

            if(menu)
            {
                DOTween.To(() => musicObj.volume, x => musicObj.volume = x, 0f, 0.5f);
                nextMusicID = MusicID.Menu;
                Invoke(nameof(MusicBypass), 0.5f);
            }
        }
    }

    public void PlayMusic(MusicID id)
    {
        AudioClip clip = music[(int)id];

        musicObj.pitch = 1f;
        musicObj.volume = musicVolume;
        musicObj.loop = true;
        musicObj.clip = clip;

        musicObj.Play();
    }

    public void SetMusicVolume(float vol)
    {
        musicObj.volume = vol;
        musicVolume = vol;
    }

    public void SetEffectVolume(float vol)
    {
        effectVolume = vol;
    }

    public void PlaySound(Sound sound)
    {
        AudioClip clip = blus_simple[Random.Range(0, blus_simple.Length)];

        float pitch = 1f; //range -3 -> 3
        float volume = effectVolume;

        if(volume < 0.001f)
        {
            return;
        }

        GameObject newSound =
            pooler.SpawnFromPool(PoolType.SoundEffects, new Vector3(2000, 0), Quaternion.identity);

        newSound.transform.SetParent(transform);

        AudioSource audioSource = newSound.GetComponent<AudioSource>();
        audioSource.loop = false;

        switch (sound)
        {
            case Sound.BlusExplosion:
                pitch = Random.Range(0.85f, 1.15f);
                clip = blus_explosion[Random.Range(0, blus_explosion.Length)];
                break;

            case Sound.Jump:
                volume = effectVolume * 0.5f;
                if(OptionHandler.jumpEffectMode == 0)
                {
                    pitch = Random.Range(0.9f, 1.1f);
                    clip = jump[Random.Range(0, jump.Length)];
                } else
                {
                    clip = origJump;
                }
                break;

            case Sound.Die:
                if (OptionHandler.jumpEffectMode == 0)
                {
                    pitch = Random.Range(0.9f, 1.1f);
                    clip = die[Random.Range(0, die.Length)];
                } else
                {
                    clip = origDie;
                }
                break;

            case Sound.MinusShoot:
                pitch = Random.Range(0.9f, 1.1f);
                clip = minusShoot[Random.Range(0, minusShoot.Length)];
                break;

            case Sound.Laser:
                volume = effectVolume * 0.2f;

                pitch = Random.Range(0.9f, 1.1f);
                clip = laserSoundClip;
                audioSource.loop = true;

                laserSound = newSound;

                break;

            case Sound.PipeExplosion:
                pitch = Random.Range(0.85f, 1.15f);
                clip = dPipe_explosion;
                break;

            case Sound.PipeHit:
                volume = effectVolume * 0.85f;

                pitch = Random.Range(0.85f, 1.15f);
                clip = dPipe_explosion_hit;
                break;

            case Sound.DModeExplosion:
                pitch = Random.Range(0.85f, 1.15f);
                clip = dMode_Explosion[Random.Range(0, dMode_Explosion.Length)];

                break;

            case Sound.LevelUp:

                //pitch = Random.Range(0.85f, 1.15f);
                clip = levelUp;

                break;

            case Sound.MenuSelect:
                clip = menuSelect;
                break;

            case Sound.MenuSelectEnd:
                clip = menuSelectEnd;
                break;

            case Sound.MenuError:
                clip = menuError;
                break;

            default:
                if(OptionHandler.jumpEffectMode == 1)
                {
                    clip = origBlus;
                }
                break;
        }

        audioSource.pitch = pitch;
        audioSource.volume = volume;

        if(sound != Sound.Laser)
        {
            audioSource.PlayOneShot(clip);
        } else
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void StopLaserSound()
    {
        if(laserSound != null)
        {
            laserSound.GetComponent<AudioSource>().Stop();
            laserSound.GetComponent<AudioSource>().loop = false;

            laserSound.GetComponent<AudioSource>().clip = null;

            laserSound.SetActive(false);

            laserSound = null;
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        currentUpdateTime += Time.deltaTime;
        
        if(lastBeat > 0)
        {
            lastBeat -= Time.deltaTime;
        }

        if (currentUpdateTime >= updateStep && lastBeat < 0)
        {
            if(musicObj.clip != null)
            {
                currentUpdateTime = 0f;
                musicObj.clip.GetData(clipSampleData, musicObj.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
                clipLoudness = 0f;

                for(int i = 0; i < clipSampleData.Length; i++)
                {
                    clipLoudness += Mathf.Abs(clipSampleData[i]);
                }

                if (Mathf.Abs(clipSampleData[clipSampleData.Length - 1] - clipSampleData[0]) > 0.35f)
                {
                    lastBeat = 0.15f;
                    Debug.Log("beat");
                }

                clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for

                if(clipLoudness - lastVol > 0.035f)
                {
                    //lastBeat = 0.01f;
                    //Debug.Log("beat");
                }

                lastVol = clipLoudness;
            }
        }
    }*/
}
