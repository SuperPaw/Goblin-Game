using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class SoundBank : MonoBehaviour
{
    private static SoundBank Instance;

    public enum UiSound
    {
        MapClick,
        ButtonClick,
        MenuClick,
        PopUpOption,
        Event,
        LevelUp,
    }

    public enum Stinger
    {
        GameLoss,
        GameStart,
        Sneaking,
        Sacrifice
    }
    public enum Background
    {
        Forest,
        Night
    }

    public enum Music
    {
        NoMusic,
        Explore,
        Battle,
        Menu
    }


    public enum GoblinSound
    {
        //------------Noices--------------
        Attacking,
        Breathing,
        Grunt,
        Roar,
        Hurt,
        Laugh,
        Death,
        Growl,


        //-----------Language-------------
        Flee,
        Attack,
        Hide,
        Move,
        EnemyComing,

        //New
        Eat,
        PanicScream,
    }

    [System.Serializable]
    public struct SoundReference
    {
        public GoblinSound Type;
        public AudioClip[] Audio;
    }
    
    [System.Serializable]
    public struct UiSoundReference
    {
        public UiSound Type;
        public AudioClip[] Audio;
    }
    [System.Serializable]
    public struct StingerSoundReference
    {
        public Stinger Type;
        public AudioClip[] Audio;
    }
    [System.Serializable]
    public struct BackgroundSoundref
    {
        public Background Type;
        public AudioClip[] Audio;
    }
    [System.Serializable]
    public struct MusicRef
    {
        public Music Type;
        public AudioClip[] Audio;
    }

    public SoundReference[] GoblinSounds;
    public UiSoundReference[] UiSounds;
    public StingerSoundReference[] Stingers;
    public BackgroundSoundref[] Backgrounds;
    public MusicRef[] Musics;

    private void Start()
    {
        if (!Instance)
            Instance = this;
    }
    

    public static AudioClip GetSound(GoblinSound sound)
    {
        if (!Instance.GoblinSounds.Any(s => s.Type == sound))
        {
            Debug.LogWarning("No sound for " + sound);
            return null;
        }

        return Rnd(Instance.GoblinSounds.First(s => s.Type == sound).Audio);
    }

    internal static AudioClip GetSound(Background type)
    {
        if (!Instance.Backgrounds.Any(s => s.Type == type))
        {
            Debug.LogWarning("No sound for " + type);
            return null;
        }

        return Rnd(Instance.Backgrounds.First(s => s.Type == type).Audio);
    }

    internal static AudioClip GetSound(Stinger sound)
    {
        if (!Instance.Stingers.Any(s => s.Type == sound))
        {
            Debug.LogWarning("No sound for " + sound);
            return null;
        }

        return Rnd(Instance.Stingers.First(s => s.Type == sound).Audio);
    }

    public static AudioClip GetSound(UiSound sound)
    {
        if (!Instance.UiSounds.Any(s => s.Type == sound))
        {
            Debug.LogWarning("No sound for " + sound);
            return null;
        }

        return Rnd(Instance.UiSounds.First(s => s.Type == sound).Audio);
    }

    public static AudioClip GetSound(Music sound)
    {
        if (!Instance.Musics.Any(s => s.Type == sound))
        {
            Debug.LogWarning("No sound for " + sound);
            return null;
        }

        return Rnd(Instance.Musics.First(s => s.Type == sound).Audio);
    }


    private static AudioClip Rnd(AudioClip[] arr)
    {
        if (arr.Length == 0)
            return null;

        return arr[Random.Range(0, arr.Length)];
    }

}
