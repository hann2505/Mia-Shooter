using System.Collections;
using UnityEngine;

namespace MiaShooter
{
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField] private Camera aimCamera;
        [SerializeField] private Transform weaponRoot;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Transform magazine;
        [SerializeField] private AudioSource weaponAudio;
        [SerializeField] private AudioClip gunshotClip;
        [SerializeField] private AudioClip emptyClip;
        [SerializeField] private AudioClip reloadClip;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private float fireInterval = 0.18f;
        [SerializeField] private float reloadDuration = 1.25f;
        [SerializeField] private float range = 100f;
        [SerializeField] private int damage = 1;
        [SerializeField] private int magazineSize = 12;

        private float nextFireTime;
        private int currentAmmo;
        private bool isReloading;
        private Vector3 weaponRestPosition;
        private Quaternion weaponRestRotation;
        private Vector3 magazineRestPosition;
        private GrenadeThrower grenadeThrower;

        public int CurrentAmmo => currentAmmo;
        public int MagazineSize => magazineSize;
        public bool IsReloading => isReloading;

        private void Awake()
        {
            ResolveRuntimeReferences();
            LoadGameplayAudio();
            currentAmmo = magazineSize;
        }

        private void Start()
        {
            ResolveRuntimeReferences();
            if (weaponRoot != null)
            {
                weaponRestPosition = weaponRoot.localPosition;
                weaponRestRotation = weaponRoot.localRotation;
            }

            if (magazine != null)
            {
                magazineRestPosition = magazine.localPosition;
            }
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            grenadeThrower ??= GetComponent<GrenadeThrower>();
            if (grenadeThrower != null && grenadeThrower.BlocksWeaponInput)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                TryReload(true);
            }
            else if (Input.GetButton("Fire1"))
            {
                TryFire();
            }
        }

        public void Configure(
            Camera camera,
            Transform root,
            Transform muzzleTransform,
            Transform magazineTransform,
            AudioSource audioSource,
            AudioClip shot,
            AudioClip empty,
            AudioClip reload,
            CameraShake shake)
        {
            aimCamera = camera;
            weaponRoot = root;
            muzzle = muzzleTransform;
            magazine = magazineTransform;
            weaponAudio = audioSource;
            gunshotClip = shot;
            emptyClip = empty;
            reloadClip = reload;
            cameraShake = shake;
        }

        private void TryFire()
        {
            if (isReloading || Time.time < nextFireTime || aimCamera == null || muzzle == null)
            {
                return;
            }

            nextFireTime = Time.time + fireInterval;
            if (currentAmmo <= 0)
            {
                PlayOneShot(emptyClip, 0.4f);
                TryReload();
                return;
            }

            currentAmmo--;
            if (weaponAudio != null)
            {
                weaponAudio.pitch = Random.Range(0.96f, 1.04f);
            }
            PlayOneShot(gunshotClip, 0.9f);
            cameraShake?.Play(0.09f, 0.035f);
            StartCoroutine(RecoilRoutine());
            VfxUtility.SpawnMuzzleFlash(muzzle.position, muzzle.forward);

            Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
            Vector3 endPoint = ray.GetPoint(range);

            if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                endPoint = hit.point;
                ShootableTarget target = hit.collider.GetComponentInParent<ShootableTarget>();
                if (target != null)
                {
                    target.TakeDamage(damage, hit.point, ray.direction);
                }
                else
                {
                    VfxUtility.SpawnImpact(hit.point, hit.normal, new Color(1f, 0.75f, 0.2f));
                    if (emptyClip != null)
                    {
                        AudioSource.PlayClipAtPoint(emptyClip, hit.point, 0.25f);
                    }
                }
            }

            VfxUtility.SpawnTracer(muzzle.position, endPoint);
        }

        public bool TryReload(bool force = false)
        {
            if (isReloading || (!force && currentAmmo >= magazineSize))
            {
                return false;
            }

            StartCoroutine(ReloadRoutine());
            return true;
        }

        private IEnumerator ReloadRoutine()
        {
            isReloading = true;
            if (weaponAudio != null)
            {
                weaponAudio.pitch = 1f;
            }
            PlayOneShot(reloadClip, 0.85f);

            Transform effectAnchor = magazine != null ? magazine : weaponRoot;
            if (effectAnchor != null)
            {
                VfxUtility.SpawnReloadPulse(effectAnchor.position);
            }

            float elapsed = 0f;
            while (elapsed < reloadDuration)
            {
                float progress = elapsed / reloadDuration;
                float lowerAmount = Mathf.Sin(progress * Mathf.PI);
                if (weaponRoot != null)
                {
                    weaponRoot.localPosition = weaponRestPosition + new Vector3(0f, -0.22f, -0.08f) * lowerAmount;
                    weaponRoot.localRotation = weaponRestRotation * Quaternion.Euler(12f * lowerAmount, 0f, 16f * lowerAmount);
                }

                if (magazine != null)
                {
                    float magazineDrop = Mathf.Sin(Mathf.Clamp01(progress * 1.55f) * Mathf.PI);
                    magazine.localPosition = magazineRestPosition + Vector3.down * (0.32f * magazineDrop);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (weaponRoot != null)
            {
                weaponRoot.localPosition = weaponRestPosition;
                weaponRoot.localRotation = weaponRestRotation;
            }
            if (magazine != null)
            {
                magazine.localPosition = magazineRestPosition;
            }

            currentAmmo = magazineSize;
            isReloading = false;
        }

        private IEnumerator RecoilRoutine()
        {
            if (weaponRoot == null)
            {
                yield break;
            }

            Vector3 kickedPosition = weaponRestPosition + new Vector3(0f, 0.015f, -0.075f);
            float elapsed = 0f;
            const float duration = 0.11f;
            while (elapsed < duration && !isReloading)
            {
                float progress = elapsed / duration;
                weaponRoot.localPosition = Vector3.Lerp(kickedPosition, weaponRestPosition, progress);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!isReloading)
            {
                weaponRoot.localPosition = weaponRestPosition;
            }
        }

        private void ResolveRuntimeReferences()
        {
            if (aimCamera == null)
            {
                aimCamera = GetComponentInChildren<Camera>();
            }

            if (weaponRoot == null && aimCamera != null)
            {
                weaponRoot = aimCamera.transform.Find("Demo Blaster");
            }

            if (weaponRoot != null)
            {
                magazine ??= weaponRoot.Find("Magazine");
                muzzle ??= weaponRoot.Find("Muzzle");
            }

            weaponAudio ??= GetComponent<AudioSource>();
            cameraShake ??= aimCamera != null ? aimCamera.GetComponent<CameraShake>() : null;
        }

        private void LoadGameplayAudio()
        {
            gunshotClip = Resources.Load<AudioClip>("Audio/gunshot") ?? gunshotClip;
            reloadClip = Resources.Load<AudioClip>("Audio/reload") ?? reloadClip;
        }

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (weaponAudio != null && clip != null)
            {
                weaponAudio.PlayOneShot(clip, volume);
            }
        }
    }
}
