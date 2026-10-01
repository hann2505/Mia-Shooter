using UnityEngine;

namespace MiaShooter
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioClip landClip;
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float jumpHeight = 1.45f;
        [SerializeField] private float footstepInterval = 0.42f;
        [SerializeField] private float minimumLandingSpeed = 3f;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;
        private float footstepTimer;
        private float fastestFallSpeed;
        private bool wasAirborne;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            LoadGameplayAudio();
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }

            Look();
            Move();
        }

        public void Configure(Camera camera, AudioSource footsteps, AudioClip landing)
        {
            playerCamera = camera;
            footstepSource = footsteps;
            landClip = landing;
        }

        private void Look()
        {
            if (Cursor.lockState != CursorLockMode.Locked || playerCamera == null)
            {
                return;
            }

            transform.Rotate(Vector3.up * (Input.GetAxis("Mouse X") * mouseSensitivity));
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity, -85f, 85f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move()
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            bool groundedBeforeMove = controller.isGrounded;
            if (groundedBeforeMove && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (groundedBeforeMove && Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                wasAirborne = true;
                fastestFallSpeed = 0f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = transform.TransformDirection(input) * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            bool groundedAfterMove = controller.isGrounded;
            UpdateLanding(groundedAfterMove);
            UpdateFootsteps(input.sqrMagnitude > 0.05f && groundedAfterMove);
        }

        private void UpdateLanding(bool isGrounded)
        {
            if (!isGrounded)
            {
                wasAirborne = true;
                fastestFallSpeed = Mathf.Min(fastestFallSpeed, verticalVelocity);
                return;
            }

            if (wasAirborne && fastestFallSpeed <= -minimumLandingSpeed && footstepSource != null && landClip != null)
            {
                float volume = Mathf.InverseLerp(minimumLandingSpeed, 10f, -fastestFallSpeed);
                footstepSource.pitch = Random.Range(0.96f, 1.04f);
                footstepSource.PlayOneShot(landClip, Mathf.Lerp(0.55f, 1f, volume));
            }

            wasAirborne = false;
            fastestFallSpeed = 0f;
        }

        private void UpdateFootsteps(bool isWalking)
        {
            if (!isWalking || footstepSource == null || footstepSource.clip == null)
            {
                footstepTimer = 0f;
                return;
            }

            footstepTimer -= Time.deltaTime;
            if (footstepTimer > 0f)
            {
                return;
            }

            footstepSource.pitch = Random.Range(0.92f, 1.08f);
            footstepSource.PlayOneShot(footstepSource.clip, 0.55f);
            footstepTimer = footstepInterval;
        }

        private void LoadGameplayAudio()
        {
            AudioClip footstepClip = Resources.Load<AudioClip>("Audio/footstep");
            if (footstepSource != null && footstepClip != null)
            {
                footstepSource.clip = footstepClip;
            }

            landClip = Resources.Load<AudioClip>("Audio/land") ?? landClip;
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
