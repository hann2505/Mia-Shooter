using System;
using UnityEngine;

namespace MiaShooter
{
    public static class SpatialAudioUtility
    {
        public static event Action<Vector3, float, Color> OnSpatialSoundPlayed;

        public static AudioSource PlayClipAtPoint3D(
            AudioClip clip,
            Vector3 position,
            float volume = 1.0f,
            float minDistance = 2.5f,
            float maxDistance = 35.0f,
            float pitch = 1.0f,
            Color cueColor = default)
        {
            if (clip == null)
            {
                return null;
            }

            GameObject soundObj = new GameObject($"SpatialSound_{clip.name}");
            soundObj.transform.position = position;

            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume * GameSettings.SfxVolume * GameSettings.MasterVolume);
            source.pitch = pitch;

            // Strict 3D spatial settings for pronounced binaural / stereo perception
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
            source.dopplerLevel = 1.0f;
            source.spread = 0f; // Point source for pinpoint directional panning
            source.Play();

            float duration = clip.length / Mathf.Max(0.1f, Mathf.Abs(pitch)) + 0.15f;
            UnityEngine.Object.Destroy(soundObj, duration);

            if (cueColor == default)
            {
                cueColor = new Color(0.18f, 0.92f, 1f);
            }
            OnSpatialSoundPlayed?.Invoke(position, volume, cueColor);

            return source;
        }

        // --- Procedural High-Fidelity 3D Audio Loops & Sound Effects ---

        private static AudioClip cachedServoHum;
        private static AudioClip cachedReactorHum;
        private static AudioClip cachedRelayPulse;
        private static AudioClip cachedProbePing;

        /// <summary>
        /// Looping mechanical servo glide sound for moving targets with Doppler effect.
        /// </summary>
        public static AudioClip GetServoHumClip()
        {
            if (cachedServoHum != null) return cachedServoHum;

            int sampleRate = 44100;
            float duration = 1.2f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            float f0 = 145f;
            float f1 = 290f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float mod = 1f + 0.18f * Mathf.Sin(2f * Mathf.PI * 6.5f * t);
                float wave0 = Mathf.Sin(2f * Mathf.PI * f0 * t);
                float wave1 = Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.4f;
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.04f;

                // Loop-edge smoothing
                float edgeFade = 1f;
                float edgeSamples = sampleRate * 0.04f;
                if (i < edgeSamples) edgeFade = (float)i / edgeSamples;
                else if (i > samples - edgeSamples) edgeFade = (float)(samples - i) / edgeSamples;

                data[i] = (wave0 * 0.65f + wave1 + noise) * mod * edgeFade * 0.6f;
            }

            cachedServoHum = AudioClip.Create("TargetServoHum", samples, 1, sampleRate, false);
            cachedServoHum.SetData(data, 0);
            return cachedServoHum;
        }

        /// <summary>
        /// Deep resonant plasma reactor hum for the left wall power generator.
        /// </summary>
        public static AudioClip GetPlasmaReactorClip()
        {
            if (cachedReactorHum != null) return cachedReactorHum;

            int sampleRate = 44100;
            float duration = 2.0f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float sub = Mathf.Sin(2f * Mathf.PI * 55f * t);      // A1 (55 Hz)
                float fundamental = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.55f; // A2 (110 Hz)
                float harmonic = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.25f;    // A3 (220 Hz)
                float pulse = 1f + 0.15f * Mathf.Sin(2f * Mathf.PI * 1.8f * t);

                float edgeFade = 1f;
                float edgeSamples = sampleRate * 0.05f;
                if (i < edgeSamples) edgeFade = (float)i / edgeSamples;
                else if (i > samples - edgeSamples) edgeFade = (float)(samples - i) / edgeSamples;

                data[i] = (sub + fundamental + harmonic) * pulse * edgeFade * 0.5f;
            }

            cachedReactorHum = AudioClip.Create("PlasmaReactorHum", samples, 1, sampleRate, false);
            cachedReactorHum.SetData(data, 0);
            return cachedReactorHum;
        }

        /// <summary>
        /// High-tech digital frequency pulse for the right wall quantum data relay.
        /// </summary>
        public static AudioClip GetQuantumRelayClip()
        {
            if (cachedRelayPulse != null) return cachedRelayPulse;

            int sampleRate = 44100;
            float duration = 1.8f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float carrier = Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.3f;
                float shimmer = Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.15f;
                float rhythmicPulse = Mathf.Pow(Mathf.Sin(2f * Mathf.PI * 1.11f * t), 12f);

                float edgeFade = 1f;
                float edgeSamples = sampleRate * 0.04f;
                if (i < edgeSamples) edgeFade = (float)i / edgeSamples;
                else if (i > samples - edgeSamples) edgeFade = (float)(samples - i) / edgeSamples;

                data[i] = (carrier + shimmer * rhythmicPulse) * edgeFade * 0.45f;
            }

            cachedRelayPulse = AudioClip.Create("QuantumRelayPulse", samples, 1, sampleRate, false);
            cachedRelayPulse.SetData(data, 0);
            return cachedRelayPulse;
        }

        /// <summary>
        /// Crystal-clear 880Hz test tone for the 3D Audio Orbiting Probe.
        /// </summary>
        public static AudioClip GetProbePingClip()
        {
            if (cachedProbePing != null) return cachedProbePing;

            int sampleRate = 44100;
            float duration = 0.24f;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            float f0 = 880f;  // A5
            float f1 = 1760f; // A6 overtone

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float attack = Mathf.Clamp01(t / 0.003f);
                float decay = Mathf.Exp(-t * 14f);

                float wave = Mathf.Sin(2f * Mathf.PI * f0 * t) * 0.8f + Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.25f;
                data[i] = wave * attack * decay * 0.85f;
            }

            cachedProbePing = AudioClip.Create("ProbePing3D", samples, 1, sampleRate, false);
            cachedProbePing.SetData(data, 0);
            return cachedProbePing;
        }
    }
}
