using UnityEngine;
namespace ProjectMeadow.Audio
{
    public sealed class LevelAudio : MonoBehaviour
    {
        private AudioSource music, effects;
        private AudioClip musicClip, victoryClip;
        public bool Muted { get; private set; }
        private void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            effects = gameObject.AddComponent<AudioSource>();
            music.playOnAwake = effects.playOnAwake = false;
            music.spatialBlend = effects.spatialBlend = 0;
            musicClip = Melody("Original meadow lullaby", new float[] { 261.63f, 329.63f, 392, 329.63f, 293.66f, 349.23f, 440, 349.23f, 261.63f, 329.63f, 392, 523.25f, 392, 329.63f, 293.66f, 261.63f }, .65f);
            victoryClip = Melody("Original level complete", new float[] { 392, 493.88f, 587.33f, 783.99f }, .24f);
            music.clip = musicClip; music.loop = true; music.volume = .16f;
            effects.volume = .35f;
            SetMuted(PlayerPrefs.GetInt("Meadow.SoundMuted", 0) != 0);
            music.Play();
        }
        public void SetMuted(bool muted)
        {
            Muted = muted; music.mute = effects.mute = muted;
            PlayerPrefs.SetInt("Meadow.SoundMuted", muted ? 1 : 0);
        }
        public void PlayVictory() { music.Stop(); effects.PlayOneShot(victoryClip); }
        private static AudioClip Melody(string label, float[] notes, float duration)
        {
            const int rate = 22050;
            int length = Mathf.RoundToInt(duration * rate);
            var samples = new float[length * notes.Length];
            for (int n = 0; n < notes.Length; n++) for (int i = 0; i < length; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Sin(Mathf.PI * i / length) * Mathf.Exp(-t * 4);
                samples[n * length + i] = .5f * envelope * (Mathf.Sin(2 * Mathf.PI * notes[n] * t) + .2f * Mathf.Sin(4 * Mathf.PI * notes[n] * t));
            }
            var clip = AudioClip.Create(label, samples.Length, 1, rate, false);
            clip.SetData(samples, 0); return clip;
        }
        private void OnDestroy()
        {
            if (musicClip != null) Destroy(musicClip);
            if (victoryClip != null) Destroy(victoryClip);
        }
    }
}
