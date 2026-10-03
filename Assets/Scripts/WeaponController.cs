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
        [SerializeField] private AudioClip laserClip;
        [SerializeField] private AudioSource laserAudio;
        [SerializeField] private CameraShake cameraShake;
        [SerializeField] private float fireInterval = 0.18f;
        [SerializeField] private float reloadDuration = 1.25f;
        [SerializeField] private float range = 100f;
        [SerializeField] private int damage = 1;
        [SerializeField] private int magazineSize = 12;
        [Header("Charged Laser")]
        [SerializeField] private int targetsPerLaserCharge = 5;
        [SerializeField] private float laserDuration = 4f;
        [SerializeField] private float laserDamageInterval = 0.12f;
        [SerializeField] private float laserWidth = 0.075f;

        private float nextFireTime;
        private float nextLaserDamageTime;
        private float laserEnergy;
        private int currentAmmo;
        private bool isReloading;
        private bool laserUnlocked;
        private bool isFiringLaser;
        private Vector3 weaponRestPosition;
        private Quaternion weaponRestRotation;
        private Vector3 magazineRestPosition;
        private GrenadeThrower grenadeThrower;
        private Transform laserMonitorMount;
        private TextMesh laserReadoutText;
        private readonly Renderer[] laserSegments = new Renderer[5];
        private Transform laserGaugeFill;
        private Renderer laserGaugeRenderer;
        private Transform topRailFill;
        private Renderer topRailRenderer;
        private Renderer rearChargeStripRenderer;
        private Light ionCoreLight;
        public static event System.Action OnHitTarget;

        private LineRenderer laserBeam;
        private LineRenderer laserAuraBeam;
        private Light muzzleAuraLight;
        private Light impactAuraLight;
        private float nextLaserVfxTime;

        private const float FullGaugeWidth = 0.176f;
        private const float FullRailLength = 0.36f;
        private const float RailZStart = -0.14f;
        private static readonly Color LaserHotColor = new Color(1f, 0.40f, 0.04f); // Fiery blazing orange
        private static readonly Color LaserHotRed = new Color(1f, 0.14f, 0.04f);   // Deep fiery crimson red
        private static readonly Color LaserHotGold = new Color(1f, 0.88f, 0.20f);  // White-hot solar flare
        private static readonly Color DimSegmentColor = new Color(0.18f, 0.04f, 0.02f); // Dark charred ember

        // 5 progressive hot segment colors (Red -> Vermilion -> Orange -> Amber -> Gold)
        private static readonly Color[] HotSegmentColors = new Color[5]
        {
            new Color(1f, 0.16f, 0.04f), // Cell 1: Deep flame red
            new Color(1f, 0.30f, 0.04f), // Cell 2: Vermilion red-orange
            new Color(1f, 0.48f, 0.04f), // Cell 3: Hot blaze orange
            new Color(1f, 0.70f, 0.06f), // Cell 4: Vivid golden orange
            new Color(1f, 0.90f, 0.18f)  // Cell 5: Solar gold
        };

        public int CurrentAmmo => currentAmmo;
        public int MagazineSize => magazineSize;
        public bool IsReloading => isReloading;
        public float LaserEnergy => laserEnergy;
        public bool IsLaserReady => laserUnlocked && laserEnergy > 0f;
        public bool IsFiringLaser => isFiringLaser;
        public AudioClip LaserClip => laserClip;
        public AudioSource LaserAudio => laserAudio;

        private void Awake()
        {
            ResolveRuntimeReferences();
            LoadGameplayAudio();
            currentAmmo = magazineSize;
            EnsureLaserVisuals();
            UpdateEnergyBar();
            if (weaponRoot != null)
            {
                weaponRoot.gameObject.SetActive(true);
            }
        }

        private void OnEnable()
        {
            DemoGameManager.TargetDestroyed += ChargeLaser;
            if (weaponRoot != null && (grenadeThrower == null || !grenadeThrower.IsGrenadeEquipped))
            {
                weaponRoot.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            DemoGameManager.TargetDestroyed -= ChargeLaser;
            StopLaser();
        }

        private void Start()
        {
            ResolveRuntimeReferences();
            if (weaponRoot != null)
            {
                weaponRoot.gameObject.SetActive(true);
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
            if (Cursor.lockState != CursorLockMode.Locked || GameSettings.IsSettingsOpen)
            {
                StopLaser();
                return;
            }

            grenadeThrower ??= GetComponent<GrenadeThrower>();
            if (grenadeThrower != null && grenadeThrower.BlocksWeaponInput)
            {
                StopLaser();
                return;
            }

            if (GameSettings.IsFire2Held() || Input.GetButton("Fire2"))
            {
                if (TryFireLaser())
                {
                    return;
                }
            }
            else
            {
                StopLaser();
                if (laserUnlocked)
                {
                    UpdatePulse();
                }
            }

            if (Input.GetKeyDown(GameSettings.ReloadKey) || Input.GetKeyDown(KeyCode.R))
            {
                TryReload(true);
            }
            else if (GameSettings.IsFire1Held() || Input.GetButton("Fire1"))
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
            CameraShake shake,
            AudioClip laser = null)
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
            if (laser != null)
            {
                laserClip = laser;
            }
        }

        public bool TryFireLaser()
        {
            if (isReloading || !laserUnlocked || laserEnergy <= 0f || aimCamera == null || muzzle == null)
            {
                StopLaser();
                return false;
            }

            EnsureLaserVisuals();
            if (laserBeam == null)
            {
                StopLaser();
                return false;
            }

            isFiringLaser = true;
            laserBeam.enabled = true;
            if (laserAuraBeam != null)
            {
                laserAuraBeam.enabled = true;
            }

            if (laserAudio != null && laserClip != null)
            {
                if (!laserAudio.isPlaying)
                {
                    laserAudio.clip = laserClip;
                    laserAudio.loop = true;
                    laserAudio.volume = 0.85f;
                    laserAudio.Play();
                }
                laserAudio.pitch = 1.0f + 0.025f * Mathf.Sin(Time.time * 28f);
            }

            Ray ray = new Ray(muzzle.position, muzzle.forward);
            Vector3 endPoint = ray.GetPoint(range);
            Vector3 hitNormal = -ray.direction;
            bool hitSomething = false;

            if (Physics.Raycast(ray, out RaycastHit hit, range, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                endPoint = hit.point;
                hitNormal = hit.normal;
                hitSomething = true;

                if (Time.time >= nextLaserDamageTime)
                {
                    nextLaserDamageTime = Time.time + laserDamageInterval;
                    ShootableTarget target = hit.collider.GetComponentInParent<ShootableTarget>();
                    if (target != null)
                    {
                        target.TakeDamage(damage, hit.point, ray.direction);
                        OnHitTarget?.Invoke();
                    }
                    else
                    {
                        VfxUtility.SpawnImpact(hit.point, hit.normal, LaserHotColor);
                    }
                }
            }

            // Core beam: sharp white-hot energy
            laserBeam.SetPosition(0, muzzle.position);
            laserBeam.SetPosition(1, endPoint);

            // Wide pulsating laser aura beam
            if (laserAuraBeam != null)
            {
                laserAuraBeam.SetPosition(0, muzzle.position);
                laserAuraBeam.SetPosition(1, endPoint);
                float auraPulse = 1f + 0.16f * Mathf.Sin(Time.time * 26f);
                laserAuraBeam.startWidth = laserWidth * 2.8f * auraPulse;
                laserAuraBeam.endWidth = laserWidth * 1.6f * auraPulse;
            }

            // Muzzle Aura Light: radiates dynamic hot orange light across the blaster and forward environment
            if (muzzleAuraLight != null)
            {
                muzzleAuraLight.enabled = true;
                muzzleAuraLight.transform.position = muzzle.position;
                muzzleAuraLight.intensity = 4.8f + UnityEngine.Random.Range(-0.35f, 0.35f);
                muzzleAuraLight.range = 15f;
            }

            // Impact Aura Light: illuminates the impact point on target/surfaces
            if (impactAuraLight != null)
            {
                impactAuraLight.enabled = hitSomething;
                if (hitSomething)
                {
                    impactAuraLight.transform.position = endPoint + hitNormal * 0.18f;
                    impactAuraLight.intensity = 4.4f + UnityEngine.Random.Range(-0.3f, 0.3f);
                    impactAuraLight.range = 11f;
                }
            }

            // Continuous impact plasma sizzle / sparks
            if (hitSomething && Time.time >= nextLaserVfxTime)
            {
                nextLaserVfxTime = Time.time + 0.05f;
                VfxUtility.SpawnImpact(endPoint, hitNormal, LaserHotColor);
            }

            laserEnergy = Mathf.Max(0f, laserEnergy - Time.deltaTime / Mathf.Max(0.1f, laserDuration));
            UpdateEnergyBar();

            if (laserEnergy <= 0f)
            {
                laserUnlocked = false;
                StopLaser();
            }

            return true;
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

            // Pronounced acoustic slapback reflections when shooting indoors (hangar complex)
            if (transform.position.z < 20.5f)
            {
                StartCoroutine(IndoorSlapbackEchoRoutine(gunshotClip, 0.9f));
            }

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
                    OnHitTarget?.Invoke();
                }
                else
                {
                    VfxUtility.SpawnImpact(hit.point, hit.normal, new Color(1f, 0.75f, 0.2f));
                    if (emptyClip != null)
                    {
                        SpatialAudioUtility.PlayClipAtPoint3D(emptyClip, hit.point, 0.35f, 1.5f, 25f, pitch: Random.Range(0.92f, 1.08f));
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

        private void ChargeLaser()
        {
            if (isFiringLaser)
            {
                return;
            }

            laserEnergy = Mathf.Clamp01(laserEnergy + 1f / Mathf.Max(1, targetsPerLaserCharge));
            if (laserEnergy >= 0.999f)
            {
                laserEnergy = 1f;
                laserUnlocked = true;
            }

            UpdateEnergyBar();
        }

        private void StopLaser()
        {
            isFiringLaser = false;
            if (laserAudio != null && laserAudio.isPlaying)
            {
                laserAudio.Stop();
            }
            if (laserBeam != null)
            {
                laserBeam.enabled = false;
            }
            if (laserAuraBeam != null)
            {
                laserAuraBeam.enabled = false;
            }
            if (impactAuraLight != null)
            {
                impactAuraLight.enabled = false;
            }
            if (muzzleAuraLight != null)
            {
                bool isReady = laserUnlocked && laserEnergy > 0f;
                muzzleAuraLight.enabled = isReady;
                if (isReady)
                {
                    muzzleAuraLight.intensity = 0.8f;
                    muzzleAuraLight.range = 4f;
                }
            }
        }

        private void EnsureLaserVisuals()
        {
            if (laserBeam != null && laserAuraBeam != null && muzzleAuraLight != null && impactAuraLight != null && laserGaugeFill != null && laserReadoutText != null)
            {
                return;
            }

            ResolveRuntimeReferences();
            CreateLaserVisuals();
            UpdateEnergyBar();
        }

        private void CreateLaserVisuals()
        {
            if (weaponRoot == null)
            {
                return;
            }

            laserMonitorMount ??= weaponRoot.Find("Laser Monitor Mount");
            if (laserMonitorMount == null)
            {
                GameObject mountObj = new GameObject("Laser Monitor Mount");
                mountObj.transform.SetParent(weaponRoot, false);
                mountObj.transform.localPosition = new Vector3(-0.14f, 0.22f, -0.14f);
                mountObj.transform.localRotation = Quaternion.Euler(14f, -24f, 0f);
                laserMonitorMount = mountObj.transform;

                CreateGunPart("Laser Mount Bracket", laserMonitorMount, new Vector3(0.06f, -0.05f, 0.03f), new Vector3(0.08f, 0.04f, 0.06f), new Color(0.16f, 0.2f, 0.24f), false);
                CreateGunPart("Laser Monitor Casing", laserMonitorMount, Vector3.zero, new Vector3(0.23f, 0.15f, 0.03f), new Color(0.025f, 0.035f, 0.045f), false);
                CreateGunPart("Laser Monitor Screen", laserMonitorMount, new Vector3(0f, 0f, -0.016f), new Vector3(0.21f, 0.13f, 0.005f), new Color(0.01f, 0.015f, 0.025f), false);

                GameObject readoutObj = new GameObject("Laser Monitor Readout");
                readoutObj.transform.SetParent(laserMonitorMount, false);
                readoutObj.transform.localPosition = new Vector3(0f, 0.04f, -0.022f);
                laserReadoutText = readoutObj.AddComponent<TextMesh>();
                laserReadoutText.text = "LASER  0%";
                laserReadoutText.anchor = TextAnchor.MiddleCenter;
                laserReadoutText.alignment = TextAlignment.Center;
                laserReadoutText.fontSize = 58;
                laserReadoutText.characterSize = 0.0022f;
                laserReadoutText.fontStyle = FontStyle.Bold;
                laserReadoutText.color = LaserHotColor;

                GameObject segmentsRoot = new GameObject("Laser Segments Root");
                segmentsRoot.transform.SetParent(laserMonitorMount, false);
                segmentsRoot.transform.localPosition = new Vector3(0f, -0.008f, -0.022f);
                for (int i = 0; i < 5; i++)
                {
                    GameObject seg = CreateGunPart($"Laser Segment {i + 1}", segmentsRoot.transform, new Vector3(-0.072f + i * 0.036f, 0f, 0f), new Vector3(0.028f, 0.032f, 0.008f), DimSegmentColor, false);
                    laserSegments[i] = seg.GetComponent<Renderer>();
                }

                CreateGunPart("Laser Gauge Trough", laserMonitorMount, new Vector3(0f, -0.046f, -0.020f), new Vector3(0.184f, 0.018f, 0.006f), new Color(0.03f, 0.04f, 0.05f), false);
                GameObject gaugeFillObj = CreateGunPart("Laser Gauge Fill", laserMonitorMount, new Vector3(-FullGaugeWidth * 0.5f, -0.046f, -0.024f), new Vector3(0.001f, 0.012f, 0.006f), LaserHotRed, true);
                laserGaugeFill = gaugeFillObj.transform;
                laserGaugeRenderer = gaugeFillObj.GetComponent<Renderer>();
            }

            topRailFill ??= weaponRoot.Find("Top Energy Rail/Top Rail Fill");
            if (topRailFill == null)
            {
                Transform topRail = weaponRoot.Find("Top Energy Rail");
                if (topRail == null)
                {
                    GameObject railObj = new GameObject("Top Energy Rail");
                    railObj.transform.SetParent(weaponRoot, false);
                    railObj.transform.localPosition = new Vector3(0f, 0.225f, 0.04f);
                    topRail = railObj.transform;
                    CreateGunPart("Top Rail Trough", topRail, Vector3.zero, new Vector3(0.08f, 0.02f, 0.38f), new Color(0.025f, 0.035f, 0.045f), false);
                }
                GameObject railFillObj = CreateGunPart("Top Rail Fill", topRail, new Vector3(0f, 0.011f, RailZStart), new Vector3(0.055f, 0.012f, 0.001f), LaserHotColor, true);
                topRailFill = railFillObj.transform;
                topRailRenderer = railFillObj.GetComponent<Renderer>();
            }

            if (laserBeam == null)
            {
                GameObject beamObject = new GameObject("Charged Laser Beam");
                beamObject.transform.SetParent(transform, false);
                laserBeam = beamObject.AddComponent<LineRenderer>();
                laserBeam.useWorldSpace = true;
                laserBeam.positionCount = 2;
                laserBeam.startWidth = laserWidth;
                laserBeam.endWidth = laserWidth * 0.42f;
                laserBeam.startColor = Color.white;
                laserBeam.endColor = new Color(LaserHotColor.r, LaserHotColor.g, LaserHotColor.b, 0.55f);
                laserBeam.material = CreateLaserCoreMaterial();
                laserBeam.enabled = false;
            }

            if (laserAuraBeam == null)
            {
                GameObject auraObject = new GameObject("Charged Laser Aura Beam");
                auraObject.transform.SetParent(transform, false);
                laserAuraBeam = auraObject.AddComponent<LineRenderer>();
                laserAuraBeam.useWorldSpace = true;
                laserAuraBeam.positionCount = 2;
                laserAuraBeam.startWidth = laserWidth * 2.8f;
                laserAuraBeam.endWidth = laserWidth * 1.6f;
                laserAuraBeam.startColor = new Color(1f, 0.22f, 0.04f, 0.75f);
                laserAuraBeam.endColor = new Color(0.9f, 0.12f, 0.02f, 0.25f);
                laserAuraBeam.material = CreateLaserAuraMaterial();
                laserAuraBeam.enabled = false;
            }

            if (muzzleAuraLight == null)
            {
                GameObject muzzleLightObj = new GameObject("Laser Muzzle Aura Light");
                muzzleLightObj.transform.SetParent(transform, false);
                muzzleAuraLight = muzzleLightObj.AddComponent<Light>();
                muzzleAuraLight.type = LightType.Point;
                muzzleAuraLight.color = LaserHotColor;
                muzzleAuraLight.range = 15f;
                muzzleAuraLight.intensity = 4.8f;
                muzzleAuraLight.enabled = false;
            }

            if (impactAuraLight == null)
            {
                GameObject impactLightObj = new GameObject("Laser Impact Aura Light");
                impactLightObj.transform.SetParent(transform, false);
                impactAuraLight = impactLightObj.AddComponent<Light>();
                impactAuraLight.type = LightType.Point;
                impactAuraLight.color = LaserHotColor;
                impactAuraLight.range = 11f;
                impactAuraLight.intensity = 4.4f;
                impactAuraLight.enabled = false;
            }
        }

        private static GameObject CreateGunPart(
            string objectName,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            bool emissive)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = objectName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            UnityEngine.Object.Destroy(part.GetComponent<Collider>());

            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 3f);
            }
            part.GetComponent<Renderer>().material = material;
            return part;
        }

        private static Material CreateLaserCoreMaterial()
        {
            Shader shader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = Color.white;
            return material;
        }

        private static Material CreateLaserAuraMaterial()
        {
            Shader shader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = new Color(LaserHotColor.r, LaserHotColor.g, LaserHotColor.b, 0.75f);
            return material;
        }

        private static Material CreateLaserMaterial()
        {
            return CreateLaserCoreMaterial();
        }

        private void UpdateEnergyBar()
        {
            float energy = Mathf.Clamp01(laserEnergy);
            bool isFull = laserUnlocked && energy >= 0.999f;
            float pulse = isFull ? (0.85f + 0.15f * Mathf.Sin(Time.time * 8.5f)) : 1f;

            // 1. Digital Text Readout in hot colors
            if (laserReadoutText != null)
            {
                if (isFiringLaser)
                {
                    laserReadoutText.text = $"LASER {Mathf.CeilToInt(energy * 100f)}%";
                    laserReadoutText.color = LaserHotColor;
                }
                else if (isFull)
                {
                    laserReadoutText.text = "READY 100%";
                    laserReadoutText.color = Color.Lerp(LaserHotColor, LaserHotGold, 0.5f + 0.5f * Mathf.Sin(Time.time * 6f));
                }
                else
                {
                    int pct = Mathf.RoundToInt(energy * 100f);
                    laserReadoutText.text = pct > 0 ? $"LASER  {pct}%" : "LASER  0%";
                    laserReadoutText.color = pct > 0 ? LaserHotColor : new Color(0.7f, 0.22f, 0.08f);
                }
            }

            // 2. 5 Segmented Hot Energy Battery Cells (Red -> Vermilion -> Orange -> Golden Orange -> Solar Gold)
            for (int i = 0; i < 5; i++)
            {
                Renderer seg = laserSegments[i];
                if (seg == null) continue;

                float threshold = (i + 1) * 0.2f - 0.005f;
                bool active = energy >= threshold;
                if (active)
                {
                    Color segColor = HotSegmentColors[i];
                    seg.material.color = segColor;
                    seg.material.EnableKeyword("_EMISSION");
                    seg.material.SetColor("_EmissionColor", segColor * (isFull ? 2.6f * pulse : 2.0f));
                }
                else
                {
                    seg.material.color = DimSegmentColor;
                    seg.material.DisableKeyword("_EMISSION");
                    seg.material.SetColor("_EmissionColor", Color.black);
                }
            }

            // 3. Monitor Continuous Hot Gauge Fill (Gradient Red to Orange)
            if (laserGaugeFill != null)
            {
                float visibleWidth = FullGaugeWidth * energy;
                Vector3 scale = laserGaugeFill.localScale;
                scale.x = Mathf.Max(0.001f, visibleWidth);
                laserGaugeFill.localScale = scale;

                Vector3 pos = laserGaugeFill.localPosition;
                pos.x = -FullGaugeWidth * 0.5f + visibleWidth * 0.5f;
                laserGaugeFill.localPosition = pos;

                if (laserGaugeRenderer != null)
                {
                    Color fillColor = Color.Lerp(LaserHotRed, LaserHotColor, energy);
                    laserGaugeRenderer.material.color = fillColor;
                    if (energy > 0.01f)
                    {
                        laserGaugeRenderer.material.EnableKeyword("_EMISSION");
                        laserGaugeRenderer.material.SetColor("_EmissionColor", fillColor * (isFull ? 2.4f * pulse : 1.8f));
                    }
                    else
                    {
                        laserGaugeRenderer.material.DisableKeyword("_EMISSION");
                        laserGaugeRenderer.material.SetColor("_EmissionColor", Color.black);
                    }
                }
            }

            // 4. Top Rail Conduit Hot Fill
            if (topRailFill != null)
            {
                float visibleLength = FullRailLength * energy;
                Vector3 scale = topRailFill.localScale;
                scale.z = Mathf.Max(0.001f, visibleLength);
                topRailFill.localScale = scale;

                Vector3 pos = topRailFill.localPosition;
                pos.z = RailZStart + visibleLength * 0.5f;
                topRailFill.localPosition = pos;

                if (topRailRenderer != null)
                {
                    Color railColor = Color.Lerp(LaserHotRed, LaserHotColor, energy);
                    topRailRenderer.material.color = railColor;
                    if (energy > 0.01f)
                    {
                        topRailRenderer.material.EnableKeyword("_EMISSION");
                        topRailRenderer.material.SetColor("_EmissionColor", railColor * (isFull ? 2.5f * pulse : 1.9f));
                    }
                    else
                    {
                        topRailRenderer.material.DisableKeyword("_EMISSION");
                        topRailRenderer.material.SetColor("_EmissionColor", Color.black);
                    }
                }
            }

            // 5. Rear Charge Strip Hot Reactive Lighting
            if (rearChargeStripRenderer != null)
            {
                Color stripColor = isFull
                    ? Color.Lerp(LaserHotRed, LaserHotColor, 0.5f + 0.5f * Mathf.Sin(Time.time * 5f))
                    : Color.Lerp(new Color(0.22f, 0.05f, 0.02f), LaserHotColor, energy);
                rearChargeStripRenderer.material.color = stripColor;
                if (energy > 0.05f)
                {
                    rearChargeStripRenderer.material.EnableKeyword("_EMISSION");
                    rearChargeStripRenderer.material.SetColor("_EmissionColor", stripColor * (isFull ? 2.0f * pulse : 0.8f + 1.2f * energy));
                }
                else
                {
                    rearChargeStripRenderer.material.EnableKeyword("_EMISSION");
                    rearChargeStripRenderer.material.SetColor("_EmissionColor", stripColor * 0.3f);
                }
            }

            // 6. Ion Core Light
            if (ionCoreLight != null)
            {
                ionCoreLight.intensity = Mathf.Lerp(1.2f, isFull ? 2.8f * pulse : 2.2f, energy);
            }
        }

        private void UpdatePulse()
        {
            UpdateEnergyBar();

            if (!isFiringLaser && muzzleAuraLight != null)
            {
                bool isReady = laserUnlocked && laserEnergy > 0f;
                muzzleAuraLight.enabled = isReady;
                if (isReady && muzzle != null)
                {
                    muzzleAuraLight.transform.position = muzzle.position;
                    float breath = 0.6f + 0.45f * Mathf.Sin(Time.time * 4f);
                    muzzleAuraLight.intensity = breath;
                    muzzleAuraLight.range = 3.5f + 1.2f * breath;
                }
            }
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

        private IEnumerator IndoorSlapbackEchoRoutine(AudioClip clip, float baseVolume)
        {
            if (weaponAudio == null || clip == null)
            {
                yield break;
            }

            // Early acoustic slapback bounce (~65ms delay bouncing off hangar walls & roof)
            yield return new WaitForSeconds(0.062f);
            if (weaponAudio != null && clip != null)
            {
                weaponAudio.PlayOneShot(clip, baseVolume * 0.42f);
            }

            // Secondary reflection (~135ms delay bouncing off far rear steel partitions)
            yield return new WaitForSeconds(0.072f);
            if (weaponAudio != null && clip != null)
            {
                weaponAudio.PlayOneShot(clip, baseVolume * 0.20f);
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
                for (int i = 0; i < aimCamera.transform.childCount; i++)
                {
                    Transform child = aimCamera.transform.GetChild(i);
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

            if (weaponRoot != null)
            {
                if (grenadeThrower == null || !grenadeThrower.IsGrenadeEquipped)
                {
                    weaponRoot.gameObject.SetActive(true);
                }

                magazine ??= weaponRoot.Find("Magazine");
                muzzle ??= weaponRoot.Find("Muzzle");

                laserMonitorMount ??= weaponRoot.Find("Laser Monitor Mount");
                if (laserMonitorMount != null)
                {
                    Transform readoutObj = laserMonitorMount.Find("Laser Monitor Readout");
                    laserReadoutText ??= readoutObj != null ? readoutObj.GetComponent<TextMesh>() : null;

                    Transform gaugeFillObj = laserMonitorMount.Find("Laser Gauge Fill");
                    if (gaugeFillObj != null)
                    {
                        laserGaugeFill = gaugeFillObj;
                        laserGaugeRenderer = gaugeFillObj.GetComponent<Renderer>();
                    }

                    Transform segRoot = laserMonitorMount.Find("Laser Segments Root") ?? laserMonitorMount;
                    for (int i = 0; i < 5; i++)
                    {
                        if (laserSegments[i] == null)
                        {
                            Transform seg = segRoot.Find($"Laser Segment {i + 1}");
                            if (seg != null)
                            {
                                laserSegments[i] = seg.GetComponent<Renderer>();
                            }
                        }
                    }
                }

                Transform railFillObj = weaponRoot.Find("Top Energy Rail/Top Rail Fill");
                if (railFillObj != null)
                {
                    topRailFill = railFillObj;
                    topRailRenderer = railFillObj.GetComponent<Renderer>();
                }

                Transform rearStripObj = weaponRoot.Find("Rear Charge Strip");
                if (rearStripObj != null)
                {
                    rearChargeStripRenderer = rearStripObj.GetComponent<Renderer>();
                }

                Transform coreLightObj = weaponRoot.Find("Ion Core Light");
                if (coreLightObj != null)
                {
                    ionCoreLight = coreLightObj.GetComponent<Light>();
                }
            }

            if (laserBeam == null)
            {
                Transform beamObj = transform.Find("Charged Laser Beam");
                if (beamObj != null)
                {
                    laserBeam = beamObj.GetComponent<LineRenderer>();
                }
            }
            if (laserAuraBeam == null)
            {
                Transform auraObj = transform.Find("Charged Laser Aura Beam");
                if (auraObj != null)
                {
                    laserAuraBeam = auraObj.GetComponent<LineRenderer>();
                }
            }
            if (muzzleAuraLight == null)
            {
                Transform muzzleLight = transform.Find("Laser Muzzle Aura Light");
                if (muzzleLight != null)
                {
                    muzzleAuraLight = muzzleLight.GetComponent<Light>();
                }
            }
            if (impactAuraLight == null)
            {
                Transform impactLight = transform.Find("Laser Impact Aura Light");
                if (impactLight != null)
                {
                    impactAuraLight = impactLight.GetComponent<Light>();
                }
            }

            weaponAudio ??= GetComponent<AudioSource>();
            if (laserAudio == null)
            {
                Transform audioChild = transform.Find("Laser Audio Source");
                if (audioChild != null)
                {
                    laserAudio = audioChild.GetComponent<AudioSource>();
                }
                else
                {
                    GameObject audioObj = new GameObject("Laser Audio Source");
                    audioObj.transform.SetParent(transform, false);
                    laserAudio = audioObj.AddComponent<AudioSource>();
                    laserAudio.playOnAwake = false;
                    laserAudio.loop = true;
                    laserAudio.spatialBlend = 0f;
                    laserAudio.volume = 0.85f;
                }
            }
            cameraShake ??= aimCamera != null ? aimCamera.GetComponent<CameraShake>() : null;
        }

        private void LoadGameplayAudio()
        {
            gunshotClip = Resources.Load<AudioClip>("Audio/gunshot") ?? gunshotClip;
            reloadClip = Resources.Load<AudioClip>("Audio/reload") ?? reloadClip;
            laserClip = Resources.Load<AudioClip>("Audio/laser") ?? laserClip;
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
