using System.Collections.Generic;
using UnityEngine;

namespace MiaShooter
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public sealed class GrenadeProjectile : MonoBehaviour
    {
        [SerializeField] private float blastRadius = 11.5f;
        [SerializeField] private float blastForce = 1700f;
        [SerializeField] private int damage = 3;
        [SerializeField] private float maximumLifetime = 6f;

        private AudioClip effectClip;
        private CameraShake cameraShake;
        private GrenadeType grenadeType;
        private bool detonated;

        public void Initialize(GrenadeType type, AudioClip clip, CameraShake shake)
        {
            grenadeType = type;
            effectClip = clip;
            cameraShake = shake;
        }

        private void Start()
        {
            Destroy(gameObject, maximumLifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (detonated)
            {
                return;
            }

            detonated = true;
            Vector3 impactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            if (grenadeType == GrenadeType.Smoke)
            {
                DeploySmoke(impactPoint);
            }
            else
            {
                Explode(impactPoint);
            }
        }

        private void Explode(Vector3 position)
        {
            VfxUtility.SpawnGrenadeExplosion(position);
            PlayExplosionAudio(position, 1f);
            ApplyBlastDamageAndForce(position);
            ShakeCamera(position, 32f, 0.58f, 0.55f);
            Destroy(gameObject);
        }

        private void ApplyBlastDamageAndForce(Vector3 position)
        {
            HashSet<ShootableTarget> damagedTargets = new HashSet<ShootableTarget>();
            foreach (Collider hit in Physics.OverlapSphere(position, blastRadius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                Rigidbody body = hit.attachedRigidbody;
                if (body != null && body.gameObject != gameObject)
                {
                    body.AddExplosionForce(blastForce, position, blastRadius, 1.2f, ForceMode.Impulse);
                }

                ShootableTarget target = hit.GetComponentInParent<ShootableTarget>();
                if (target != null && damagedTargets.Add(target))
                {
                    Vector3 direction = target.transform.position - position;
                    target.TakeDamage(damage, target.transform.position, direction.sqrMagnitude > 0.01f ? direction.normalized : Vector3.up);
                }
            }
        }

        private void DeploySmoke(Vector3 position)
        {
            VfxUtility.SpawnSmokeCloud(position);
            PlayExplosionAudio(position, 0.45f);
            ShakeCamera(position, 12f, 0.16f, 0.25f);
            Destroy(gameObject);
        }

        private void ShakeCamera(Vector3 position, float maxDistance, float amplitude, float duration)
        {
            if (cameraShake == null)
            {
                return;
            }

            float distance = Vector3.Distance(cameraShake.transform.position, position);
            if (distance > maxDistance)
            {
                return;
            }

            float normalizedDist = Mathf.Clamp01(distance / maxDistance);
            float strengthMultiplier = Mathf.Pow(1f - normalizedDist, 1.25f);
            float actualDuration = Mathf.Max(duration * strengthMultiplier, 0.22f);
            float actualStrength = amplitude * strengthMultiplier;

            cameraShake.Play(actualDuration, actualStrength);
        }

        private void PlayExplosionAudio(Vector3 position, float volume)
        {
            if (effectClip == null)
            {
                return;
            }

            GameObject audioObject = new GameObject("Grenade Explosion Audio");
            audioObject.transform.position = position;

            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.clip = effectClip;
            source.volume = volume;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = 4f;
            source.maxDistance = 65f;
            source.Play();

            Destroy(audioObject, effectClip.length + 0.1f);
        }
    }
}
