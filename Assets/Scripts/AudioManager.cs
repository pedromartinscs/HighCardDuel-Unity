using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HighCardDuel
{
    public sealed class AudioManager : MonoBehaviour
    {
        private static readonly string[] MusicPaths =
        {
            "Assets/Audio/Music/PMCS_RoyalCasino_Loop.mp3",
            "Assets/Audio/Music/PMCS_RoyalCasino_Loop.ogg"
        };

        private static readonly string[] CardFlipPaths =
        {
            "Assets/Audio/SFX/CardFlip_01.ogg"
        };

        private static readonly string[] ScorePointPaths =
        {
            "Assets/Audio/SFX/ScorePoint_01.ogg",
            "Assets/Audio/SFX/ScorePoint_01.wav",
            "Assets/Audio/SFX/ScorePoint_01.mp3"
        };

        private static readonly string[] VictoryPaths =
        {
            "Assets/Audio/SFX/Victory_01.ogg",
            "Assets/Audio/SFX/Victory_01.wav",
            "Assets/Audio/SFX/Victory_01.mp3"
        };

        private static readonly string[] ChipStackPaths =
        {
            "Assets/Audio/SFX/chips-stack-1.ogg",
            "Assets/Audio/SFX/chips-stack-2.ogg",
            "Assets/Audio/SFX/chips-stack-3.ogg"
        };

        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.35f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.85f;
        [SerializeField] private AudioClip musicClip;
        [SerializeField] private AudioClip cardFlipClip;
        [SerializeField] private AudioClip scorePointClip;
        [SerializeField] private AudioClip victoryClip;
        [SerializeField] private AudioClip[] chipStackClips;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private bool isConfigured;

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Mathf.Clamp01(value);
                ApplyVolumes();
            }
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set => sfxVolume = Mathf.Clamp01(value);
        }

        private void Awake()
        {
            EnsureSources();
        }

        private void Start()
        {
            if (!isConfigured)
            {
                Configure();
            }
        }

        public void Configure()
        {
            isConfigured = true;
            EnsureSources();
            LoadClips();
            ApplyVolumes();
            PlayMusic();
        }

        public void PlayMusic()
        {
            if (musicClip == null)
            {
                return;
            }

            EnsureSources();

            if (musicSource.clip == musicClip && musicSource.isPlaying)
            {
                return;
            }

            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlayCardFlip()
        {
            PlaySfx(cardFlipClip);
        }

        public void PlayScorePoint()
        {
            PlaySfx(scorePointClip);
        }

        public void PlayVictory()
        {
            PlaySfx(victoryClip);
        }

        public void PlayChipStack()
        {
            PlaySfx(GetRandomClip(chipStackClips));
        }

        private void EnsureSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
                musicSource.spatialBlend = 0f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
                sfxSource.spatialBlend = 0f;
            }
        }

        private void LoadClips()
        {
            musicClip = musicClip != null ? musicClip : LoadAudioClip(MusicPaths);
            cardFlipClip = cardFlipClip != null ? cardFlipClip : LoadAudioClip(CardFlipPaths);
            scorePointClip = scorePointClip != null ? scorePointClip : LoadAudioClip(ScorePointPaths);
            victoryClip = victoryClip != null ? victoryClip : LoadAudioClip(VictoryPaths);
            chipStackClips = HasAnyClip(chipStackClips) ? chipStackClips : LoadAudioClips(ChipStackPaths);
        }

        private void ApplyVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            EnsureSources();
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        private static AudioClip LoadAudioClip(string[] assetPaths)
        {
#if UNITY_EDITOR
            foreach (var assetPath in assetPaths)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip != null)
                {
                    return clip;
                }
            }
#endif

            return null;
        }

        private static AudioClip[] LoadAudioClips(string[] assetPaths)
        {
#if UNITY_EDITOR
            var clips = new List<AudioClip>(assetPaths.Length);
            foreach (var assetPath in assetPaths)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (clip != null)
                {
                    clips.Add(clip);
                }
            }

            return clips.ToArray();
#else
            return null;
#endif
        }

        private static AudioClip GetRandomClip(AudioClip[] clips)
        {
            if (!HasAnyClip(clips))
            {
                return null;
            }

            var startIndex = Random.Range(0, clips.Length);
            for (var offset = 0; offset < clips.Length; offset++)
            {
                var clip = clips[(startIndex + offset) % clips.Length];
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        private static bool HasAnyClip(AudioClip[] clips)
        {
            if (clips == null)
            {
                return false;
            }

            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
