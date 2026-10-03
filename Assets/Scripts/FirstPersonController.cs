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
                if (GameSettings.CurrentlyRebindingAction == null)
                {
                    GameSettings.IsSettingsOpen = !GameSettings.IsSettingsOpen;
                    SetCursorLocked(!GameSettings.IsSettingsOpen);
                }
            }

            if (!GameSettings.IsSettingsOpen && Cursor.lockState != CursorLockMode.Locked)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    SetCursorLocked(true);
                }
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
            if (Cursor.lockState != CursorLockMode.Locked || playerCamera == null || GameSettings.IsSettingsOpen)
            {
                return;
            }

            float currentSensitivity = mouseSensitivity * (GameSettings.MouseSensitivity / 2f);
            transform.Rotate(Vector3.up * (Input.GetAxis("Mouse X") * currentSensitivity));
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * currentSensitivity, -85f, 85f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move()
        {
            if (GameSettings.IsSettingsOpen)
            {
                return;
            }

            float h = 0f;
            if (Input.GetKey(GameSettings.MoveLeftKey)) h -= 1f;
            if (Input.GetKey(GameSettings.MoveRightKey)) h += 1f;

            float v = 0f;
            if (Input.GetKey(GameSettings.MoveForwardKey)) v += 1f;
            if (Input.GetKey(GameSettings.MoveBackwardKey)) v -= 1f;

            if (Mathf.Approximately(h, 0f)) h = Input.GetAxisRaw("Horizontal");
            if (Mathf.Approximately(v, 0f)) v = Input.GetAxisRaw("Vertical");

            Vector3 input = new Vector3(h, 0f, v);
            input = Vector3.ClampMagnitude(input, 1f);

            bool groundedBeforeMove = controller.isGrounded;
            if (groundedBeforeMove && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            if (groundedBeforeMove && (Input.GetKeyDown(GameSettings.JumpKey) || Input.GetButtonDown("Jump")))
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
