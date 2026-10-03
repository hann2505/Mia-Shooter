using UnityEngine;

namespace MiaShooter
{
    [DisallowMultipleComponent]
    public sealed class GrenadeThrower : MonoBehaviour
    {
        [SerializeField] private Camera throwCamera;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private AudioClip fragmentationClip;
        [SerializeField] private AudioClip smokeClip;
        [SerializeField] private GrenadeThrowAnimation throwAnimation;
        [SerializeField] private KeyCode fragmentationKey = KeyCode.G;
        [SerializeField] private KeyCode smokeKey = KeyCode.H;
        [SerializeField] private float throwSpeed = 11f;
        [SerializeField] private float upwardBoost = 2.8f;
        [SerializeField] private float throwCooldown = 0.75f;

        private float nextThrowTime;
        private float weaponUnlockTime;
        private Transform weaponRoot;
        private GrenadeType selectedGrenadeType;

        public bool IsGrenadeEquipped => throwAnimation != null && throwAnimation.IsEquipped;
        public bool BlocksWeaponInput => throwAnimation != null
            && (throwAnimation.IsEquipped || throwAnimation.IsPlaying || Time.time < weaponUnlockTime);
        public string SelectedGrenadeName => selectedGrenadeType == GrenadeType.Smoke ? "SMOKE GRENADE" : "FRAG GRENADE";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstalled()
        {
            FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
            if (player != null && player.GetComponent<GrenadeThrower>() == null)
            {
                player.gameObject.AddComponent<GrenadeThrower>();
            }
        }

        private void Awake()
        {
            ResolveRuntimeReferences();
            if (!IsGrenadeEquipped && weaponRoot != null)
            {
                SetWeaponVisible(true);
            }
        }

        private void Start()
        {
            ResolveRuntimeReferences();
            if (!IsGrenadeEquipped && weaponRoot != null)
            {
                SetWeaponVisible(true);
            }
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            if (Input.GetKeyDown(fragmentationKey))
            {
                EquipGrenade(GrenadeType.Fragmentation);
            }
            else if (Input.GetKeyDown(smokeKey))
            {
                EquipGrenade(GrenadeType.Smoke);
            }
            else if (IsGrenadeEquipped && Input.GetButtonDown("Fire1"))
            {
                TryThrow();
            }
        }

        public bool EquipGrenade(GrenadeType grenadeType)
        {
            ResolveRuntimeReferences();
            if (throwAnimation == null || throwAnimation.IsPlaying)
            {
                return false;
            }

            if (throwAnimation.IsEquipped && throwAnimation.EquippedType == grenadeType)
            {
                return throwAnimation.Unequip(RestoreWeapon);
            }

            selectedGrenadeType = grenadeType;
            SetWeaponVisible(false);
            if (throwAnimation.Equip(grenadeType))
            {
                return true;
            }

            RestoreWeapon();
            return false;
        }

        public bool TryThrow()
        {
            ResolveRuntimeReferences();
            if (throwCamera == null || throwAnimation == null || !throwAnimation.IsReadyToThrow || Time.time < nextThrowTime)
            {
                return false;
            }

            GrenadeType grenadeType = selectedGrenadeType;
            if (!throwAnimation.Throw(
                    releasePosition => ReleaseGrenade(releasePosition, grenadeType),
                    RestoreWeapon))
            {
                return false;
            }

            nextThrowTime = Time.time + throwCooldown;
            return true;
        }

        private void ReleaseGrenade(Vector3 releasePosition, GrenadeType grenadeType)
        {
            GameObject grenade = CreateGrenade(releasePosition, grenadeType);
            GrenadeProjectile projectile = grenade.GetComponent<GrenadeProjectile>();
            AudioClip effectClip = grenadeType == GrenadeType.Smoke ? smokeClip : fragmentationClip;
            projectile.Initialize(grenadeType, effectClip, cameraShake);

            Rigidbody body = grenade.GetComponent<Rigidbody>();
            body.linearVelocity = throwCamera.transform.forward * throwSpeed + Vector3.up * upwardBoost;
            body.AddTorque(throwCamera.transform.right * 8f + Random.insideUnitSphere * 3f, ForceMode.VelocityChange);

            Collider grenadeCollider = grenade.GetComponent<Collider>();
            foreach (Collider ownerCollider in GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(grenadeCollider, ownerCollider, true);
            }
        }

        public void Configure(Camera camera, CameraShake shake, AudioClip fragmentation, AudioClip smoke)
        {
            throwCamera = camera;
            cameraShake = shake;
            fragmentationClip = fragmentation;
            smokeClip = smoke;
        }

        private void ResolveRuntimeReferences()
        {
            throwCamera ??= GetComponentInChildren<Camera>();
            if (cameraShake == null && throwCamera != null)
            {
                cameraShake = throwCamera.GetComponent<CameraShake>() ?? throwCamera.gameObject.AddComponent<CameraShake>();
            }

            if (throwAnimation == null && throwCamera != null)
            {
                throwAnimation = throwCamera.GetComponent<GrenadeThrowAnimation>()
                    ?? throwCamera.gameObject.AddComponent<GrenadeThrowAnimation>();
            }

            if (weaponRoot == null && throwCamera != null)
            {
                for (int i = 0; i < throwCamera.transform.childCount; i++)
                {
                    Transform child = throwCamera.transform.GetChild(i);
                    if (child.name == "Demo Blaster")
                    {
                        weaponRoot = child;
                        break;
                    }
                }
            }

            if (weaponRoot == null)
            {
                foreach (Transform t in GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Demo Blaster")
                    {
                        weaponRoot = t;
                        break;
                    }
                }
            }

            fragmentationClip ??= Resources.Load<AudioClip>("Audio/bomb-explosion");
            smokeClip ??= Resources.Load<AudioClip>("Audio/smoke");
        }

        private static GameObject CreateGrenade(Vector3 position, GrenadeType grenadeType)
        {
            string name = grenadeType == GrenadeType.Smoke ? "Thrown Smoke Grenade" : "Thrown Fragmentation Grenade";
            GameObject grenade = GrenadeVisualFactory.Create(name, grenadeType);
            grenade.transform.position = position;

            Rigidbody body = grenade.AddComponent<Rigidbody>();
            body.mass = 0.55f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            grenade.AddComponent<GrenadeProjectile>();
            return grenade;
        }

        private void RestoreWeapon()
        {
            SetWeaponVisible(true);
            weaponUnlockTime = Time.time + 0.15f;
        }

        private void SetWeaponVisible(bool visible)
        {
            if (weaponRoot != null)
            {
                weaponRoot.gameObject.SetActive(visible);
            }
        }

        private void OnDisable()
        {
            SetWeaponVisible(true);
        }
    }
}
