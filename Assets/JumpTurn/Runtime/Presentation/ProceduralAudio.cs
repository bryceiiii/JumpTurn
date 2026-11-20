using UnityEngine;
using Random = UnityEngine.Random;

namespace JumpTurn
{
    public enum SfxId
    {
        Jump,
        Land,
        Rotate,
        Phase,
        Collect,
        Death,
        Complete,
        Blocked,
        Menu
    }

    public sealed class ProceduralAudio : MonoBehaviour
    {
        private const int SampleRate = 22050;
        private AudioSource _sfx;
        private AudioSource _music;
        private AudioClip _jump;
        private AudioClip _land;
        private AudioClip _rotate;
        private AudioClip _phase;
        private AudioClip _collect;
        private AudioClip _death;
        private AudioClip _complete;
        private AudioClip _blocked;
        private AudioClip _menu;

        public void Initialize()
        {
            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.spatialBlend = 0f;
            _sfx.volume = 0.55f;

            _music = gameObject.AddComponent<AudioSource>();
            _music.playOnAwake = false;
            _music.spatialBlend = 0f;
            _music.loop = true;
            _music.volume = 0.12f;

            _jump = CreateSweep("Jump", 420f, 680f, 0.12f, 0.18f, Wave.Sine);
            _land = CreateSweep("Land", 180f, 90f, 0.09f, 0.12f, Wave.Square);
            _rotate = CreateSweep("Rotate", 220f, 880f, 0.28f, 0.14f, Wave.Triangle);
            _phase = CreateTwoTone("Phase", 320f, 520f, 0.18f, 0.14f);
            _collect = CreateArpeggio("Collect", new[] { 660f, 880f, 1320f }, 0.06f, 0.16f);
            _death = CreateNoiseSweep("Death", 0.36f, 0.18f);
            _complete = CreateArpeggio("Complete", new[] { 523f, 659f, 784f, 1046f }, 0.13f, 0.18f);
            _blocked = CreateTwoTone("Blocked", 170f, 125f, 0.12f, 0.14f);
            _menu = CreateTwoTone("Menu", 440f, 660f, 0.08f, 0.1f);

            _music.clip = CreateAmbient("JumpTurn Ambience", 8f);
            _music.Play();
        }

        public void Play(SfxId id)
        {
            if (_sfx == null)
            {
                return;
            }

            AudioClip clip = id switch
            {
                SfxId.Jump => _jump,
                SfxId.Land => _land,
                SfxId.Rotate => _rotate,
                SfxId.Phase => _phase,
                SfxId.Collect => _collect,
                SfxId.Death => _death,
                SfxId.Complete => _complete,
                SfxId.Blocked => _blocked,
                _ => _menu
            };

            if (clip != null)
            {
                _sfx.PlayOneShot(clip);
            }
        }

        public void SetMusicMuted(bool muted)
        {
            if (_music != null)
            {
                _music.mute = muted;
            }
        }

        private enum Wave
        {
            Sine,
            Square,
            Triangle
        }

        private static AudioClip CreateSweep(string name, float startFrequency, float endFrequency, float duration, float volume, Wave wave)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            float phase = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += frequency / SampleRate;
                float sample = SampleWave(phase, wave);
                data[i] = sample * volume * Envelope(t);
            }

            return MakeClip(name, data);
        }

        private static AudioClip CreateTwoTone(string name, float first, float second, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float frequency = t < 0.5f ? first : second;
                float local = t < 0.5f ? t * 2f : (t - 0.5f) * 2f;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / SampleRate) * volume * Envelope(local);
            }

            return MakeClip(name, data);
        }

        private static AudioClip CreateArpeggio(string name, float[] notes, float noteDuration, float volume)
        {
            int noteSamples = Mathf.CeilToInt(SampleRate * noteDuration);
            var data = new float[noteSamples * notes.Length];
            for (int note = 0; note < notes.Length; note++)
            {
                for (int i = 0; i < noteSamples; i++)
                {
                    float t = i / (float)noteSamples;
                    float sample = Mathf.Sin(2f * Mathf.PI * notes[note] * i / SampleRate);
                    data[note * noteSamples + i] = sample * volume * Envelope(t);
                }
            }

            return MakeClip(name, data);
        }

        private static AudioClip CreateNoiseSweep(string name, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            float smooth = 0f;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                smooth = Mathf.Lerp(smooth, Random.Range(-1f, 1f), 0.12f);
                data[i] = smooth * volume * Mathf.Pow(1f - t, 1.8f);
            }

            return MakeClip(name, data);
        }

        private static AudioClip CreateAmbient(string name, float duration)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            float[] notes = { 261.63f, 329.63f, 392f, 523.25f, 392f, 329.63f };
            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)SampleRate;
                int noteIndex = Mathf.FloorToInt(time / (duration / notes.Length)) % notes.Length;
                float noteTime = time % (duration / notes.Length);
                float frequency = notes[noteIndex];
                float pluck = Mathf.Sin(2f * Mathf.PI * frequency * noteTime);
                float overtone = Mathf.Sin(2f * Mathf.PI * frequency * 2f * noteTime) * 0.22f;
                float noteEnvelope = Mathf.Exp(-noteTime * 2.4f);
                float pad = Mathf.Sin(2f * Mathf.PI * 130.81f * time) * 0.09f;
                data[i] = (pluck * 0.08f + overtone * 0.035f) * noteEnvelope + pad;
            }

            return MakeClip(name, data);
        }

        private static float SampleWave(float phase, Wave wave)
        {
            float normalized = phase - Mathf.Floor(phase);
            return wave switch
            {
                Wave.Square => normalized < 0.5f ? 1f : -1f,
                Wave.Triangle => 1f - 4f * Mathf.Abs(normalized - 0.5f),
                _ => Mathf.Sin(normalized * Mathf.PI * 2f)
            };
        }

        private static float Envelope(float t)
        {
            float attack = Mathf.Clamp01(t / 0.08f);
            float release = Mathf.Clamp01((1f - t) / 0.3f);
            return attack * release;
        }

        private static AudioClip MakeClip(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
