using UnityEngine;

namespace ForgottenFort.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("SFX Clips (auto-loaded from Resources/Audio/SFX if empty)")]
        public AudioClip footstep;
        public AudioClip footstepSprint;
        public AudioClip keyPickup;
        public AudioClip doorOpen;
        public AudioClip doorLocked;
        public AudioClip hurt;
        public AudioClip caught;
        public AudioClip win;
        public AudioClip puzzleSolve;
        public AudioClip guardAlert;
        public AudioClip menuClick;

        [Range(0f, 1f)] public float masterVolume = 0.85f;
        [Range(0f, 1f)] public float musicVolume = 0.35f;
        [Range(0f, 1f)] public float sfxVolume = 0.9f;

        AudioSource source;
        AudioSource musicSource;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = musicVolume * masterVolume;

            LoadClipsIfMissing();
            StartAmbientDrone();
        }

        void LoadClipsIfMissing()
        {
            footstep ??= Load("footstep");
            footstepSprint ??= Load("footstep_sprint");
            keyPickup ??= Load("key_pickup");
            doorOpen ??= Load("door_open");
            doorLocked ??= Load("door_locked");
            hurt ??= Load("hurt");
            caught ??= Load("caught");
            win ??= Load("win");
            puzzleSolve ??= Load("puzzle_solve");
            guardAlert ??= Load("guard_alert");
            menuClick ??= Load("menu_click");
        }

        static AudioClip Load(string name) =>
            Resources.Load<AudioClip>($"Audio/SFX/{name}");

        void StartAmbientDrone()
        {
            if (musicSource.isPlaying) return;
            musicSource.clip = ProceduralTone(55f, 4f, 0.08f);
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void PlayFootstep(bool sprint = false) =>
            Play(sprint ? footstepSprint : footstep, sprint ? 0.95f : 0.75f, Random.Range(0.92f, 1.08f));

        public void PlayKeyPickup() => Play(keyPickup, 1f);
        public void PlayDoorOpen() => Play(doorOpen, 1f);
        public void PlayDoorLocked() => Play(doorLocked, 0.9f);
        public void PlayHurt() => Play(hurt, 1f);
        public void PlayCaught() => Play(caught, 1f);
        public void PlayWin() => Play(win, 1f);
        public void PlayPuzzleSolve() => Play(puzzleSolve, 1f);
        public void PlayGuardAlert() => Play(guardAlert, 0.85f);
        public void PlayMenuClick() => Play(menuClick, 0.8f);

        void Play(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (source == null) return;
            if (clip != null)
            {
                source.pitch = pitch;
                source.PlayOneShot(clip, sfxVolume * masterVolume * volumeScale);
                return;
            }
            source.pitch = 1f;
            source.PlayOneShot(ProceduralTone(440f, 0.08f, 0.25f * volumeScale), sfxVolume * masterVolume);
        }

        static AudioClip ProceduralTone(float freq, float duration, float volume = 0.5f)
        {
            const int sampleRate = 44100;
            int sampleLength = Mathf.Max(1, (int)(sampleRate * duration));
            var clip = AudioClip.Create("tone", sampleLength, 1, sampleRate, false);
            var data = new float[sampleLength];
            for (int i = 0; i < sampleLength; i++)
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate) * volume * (1f - (float)i / sampleLength);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
