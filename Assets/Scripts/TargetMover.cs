using UnityEngine;

namespace MiaShooter
{
    public sealed class TargetMover : MonoBehaviour
    {
        [SerializeField] private Vector3 movement = new Vector3(3f, 0f, 0f);
        [SerializeField] private float speed = 1.25f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        public void Configure(Vector3 offset, float movementSpeed)
        {
            movement = offset;
            speed = movementSpeed;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * speed);
            transform.position = startPosition + movement * offset;
        }
    }
}
