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

            health -= amount;
            VfxUtility.SpawnImpact(hitPoint, -shotDirection, new Color(0.15f, 0.9f, 1f));
            if (hitClip != null)
            {
                AudioSource.PlayClipAtPoint(hitClip, hitPoint, 0.75f);
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
            DemoGameManager.Instance?.RegisterDestroyedTarget(scoreValue);
            VfxUtility.SpawnExplosion(transform.position, new Color(0.1f, 0.8f, 1f));
            VfxUtility.SpawnFragments(transform.position, GetPrimaryColor());

            if (explosionClip != null)
            {
                AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);
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

        private Color GetPrimaryColor()
        {
            return targetRenderers.Length > 0 ? targetRenderers[0].material.color : Color.cyan;
        }
    }
}
