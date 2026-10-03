using UnityEngine;

namespace MiaShooter
{
    public sealed class TargetMover : MonoBehaviour
    {
        [SerializeField] private Vector3 movement = new Vector3(3f, 0f, 0f);
        [SerializeField] private float speed = 1.25f;

        private Vector3 startPosition;
        private AudioSource servoAudio;

        private void Start()
        {
            startPosition = transform.position;
            SetupServoAudio();
        }

        private void SetupServoAudio()
        {
            servoAudio = gameObject.AddComponent<AudioSource>();
            servoAudio.clip = SpatialAudioUtility.GetServoHumClip();
            servoAudio.loop = true;
            servoAudio.playOnAwake = false;

            // Pure 3D spatial audio configuration with pronounced Doppler
            servoAudio.spatialBlend = 1.0f;
            servoAudio.rolloffMode = AudioRolloffMode.Logarithmic;
            servoAudio.minDistance = 2.0f;
            servoAudio.maxDistance = 24.0f;
            servoAudio.dopplerLevel = 1.8f;
            servoAudio.spread = 0f;
            servoAudio.volume = 0.30f * GameSettings.SfxVolume;

            servoAudio.Play();
        }

        public void Configure(Vector3 offset, float movementSpeed)
        {
            movement = offset;
            speed = movementSpeed;
        }

        private void Update()
        {
            float sin = Mathf.Sin(Time.time * speed);
            transform.position = startPosition + movement * sin;

            if (servoAudio != null)
            {
                // Dynamic pitch modulation reflecting instantaneous movement velocity
                float velocityFactor = Mathf.Cos(Time.time * speed) * speed;
                servoAudio.pitch = Mathf.Clamp(1.0f + velocityFactor * 0.045f, 0.85f, 1.15f);
                servoAudio.volume = 0.30f * GameSettings.SfxVolume * GameSettings.MasterVolume;
            }
        }
    }
}
