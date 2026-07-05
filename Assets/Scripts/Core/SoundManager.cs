using UnityEngine;

namespace ForgottenFort.Core
{
    /// <summary>
    /// Procedural sound effects using Unity's audio synthesis (no external files required).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }
        AudioSource source;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            source = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }

        public void PlayKeyPickup() => PlayTone(880f, 0.1f);
        public void PlayDoorOpen() => PlayTone(220f, 0.2f);
        public void PlayFootstep() => PlayTone(120f, 0.05f, 0.3f);
        public void PlayCaught() => PlayTone(110f, 0.5f);
        public void PlayWin() => PlayTone(523f, 0.3f);

        void PlayTone(float freq, float duration, float volume = 0.5f)
        {
            if (source == null) return;
            int sampleRate = 44100;
            int sampleLength = (int)(sampleRate * duration);
            var clip = AudioClip.Create("tone", sampleLength, 1, sampleRate, false);
            var data = new float[sampleLength];
            for (int i = 0; i < sampleLength; i++)
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * i / sampleRate) * volume * (1f - (float)i / sampleLength);
            clip.SetData(data, 0);
            source.PlayOneShot(clip);
        }
    }
}
