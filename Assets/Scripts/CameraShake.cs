using System.Collections;
using UnityEngine;

namespace MiaShooter
{
    public sealed class CameraShake : MonoBehaviour
    {
        private Vector3 baseLocalPosition;
        private Coroutine activeShake;

        private void Awake()
        {
            baseLocalPosition = transform.localPosition;
        }

        public void Play(float duration, float strength)
        {
            if (activeShake != null)
            {
                StopCoroutine(activeShake);
            }

            activeShake = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float fade = 1f - elapsed / duration;
                transform.localPosition = baseLocalPosition + Random.insideUnitSphere * (strength * fade);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = baseLocalPosition;
            activeShake = null;
        }
    }
}
