using System.Collections;
using UnityEngine;

namespace MiaShooter
{
    public sealed class ShootableTarget : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 2;
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private float respawnDelay = 2.5f;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip explosionClip;

        private Renderer[] targetRenderers;
        private Collider[] targetColliders;
        private int health;
        private bool destroyed;

        private Vector3 lastShotDirection = Vector3.forward;

        private void Awake()
        {
            targetRenderers = GetComponentsInChildren<Renderer>();
            targetColliders = GetComponentsInChildren<Collider>();
            health = maxHealth;
        }

        public void Configure(AudioClip hit, AudioClip explosion)
        {
            hitClip = hit;
            explosionClip = explosion;
        }

        public void TakeDamage(int amount, Vector3 hitPoint, Vector3 shotDirection)
        {
            if (destroyed)
            {
                return;
            }

            lastShotDirection = shotDirection;
            health -= amount;
            VfxUtility.SpawnImpact(hitPoint, -shotDirection, new Color(0.15f, 0.9f, 1f));
            if (hitClip != null)
            {
                SpatialAudioUtility.PlayClipAtPoint3D(hitClip, hitPoint, 0.85f, 2.0f, 30f, pitch: Random.Range(0.96f, 1.04f), cueColor: GetPrimaryColor());
            }

            if (health <= 0)
            {
                StartCoroutine(DestroyAndRespawn());
            }
            else
            {
                StartCoroutine(Flash());
            }
        }

        private IEnumerator Flash()
        {
            SetEmission(Color.white * 2.5f);
            yield return new WaitForSeconds(0.08f);
            SetEmission(Color.black);
        }

        private IEnumerator DestroyAndRespawn()
        {
            destroyed = true;
            Color primaryColor = GetPrimaryColor();

            DemoGameManager.Instance?.RegisterDestroyedTarget(scoreValue, transform.position, primaryColor);
            VfxUtility.SpawnTargetEliminationEffect(transform.position, primaryColor, lastShotDirection);

            AudioClip killChime = VfxUtility.GetKillChimeAudio();
            if (killChime != null)
            {
                SpatialAudioUtility.PlayClipAtPoint3D(killChime, transform.position, 1.0f, 3.0f, 55f, 1.0f, Color.white);
            }

            if (explosionClip != null)
            {
                SpatialAudioUtility.PlayClipAtPoint3D(explosionClip, transform.position, 0.85f, 3.0f, 45f, 1.0f, primaryColor);
            }

            CameraShake shake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;
            if (shake != null)
            {
                shake.Play(0.14f, 0.045f);
            }

            SetVisible(false);
            yield return new WaitForSeconds(respawnDelay);
            health = maxHealth;
            SetVisible(true);
            SetEmission(Color.black);
            destroyed = false;
        }

        private void SetVisible(bool visible)
        {
            foreach (Renderer targetRenderer in targetRenderers)
            {
                targetRenderer.enabled = visible;
            }

            foreach (Collider targetCollider in targetColliders)
            {
                targetCollider.enabled = visible;
            }
        }

        private void SetEmission(Color color)
        {
            foreach (Renderer targetRenderer in targetRenderers)
            {
                foreach (Material material in targetRenderer.materials)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", color);
                }
            }
        }

        public Color GetPrimaryColor()
        {
            if (targetRenderers != null)
            {
                foreach (Renderer r in targetRenderers)
                {
                    if (r != null && r.gameObject.name.Contains("Plate"))
                    {
                        return r.sharedMaterial != null ? r.sharedMaterial.color : r.material.color;
                    }
                }

                if (targetRenderers.Length > 0 && targetRenderers[0] != null)
                {
                    return targetRenderers[0].sharedMaterial != null ? targetRenderers[0].sharedMaterial.color : targetRenderers[0].material.color;
                }
            }

            return Color.cyan;
        }
    }
}
