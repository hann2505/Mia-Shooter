using UnityEngine;

namespace MiaShooter
{
    public sealed class SpatialAudioProbe : MonoBehaviour
    {
        public static SpatialAudioProbe Instance { get; private set; }

        [SerializeField] private float orbitRadius = 3.8f;
        [SerializeField] private float orbitSpeed = 48.0f; // degrees per second (approx 7.5s for 360°)
        [SerializeField] private float pingInterval = 0.62f;

        private Camera playerCamera;
        private Transform probeVisual;
        private Light probeLight;
        private MeshRenderer probeRenderer;

        private bool isActive = false;
        private float currentAngle = 0f;
        private float nextPingTime = 0f;
        private float currentAzimuth = 0f;

        public bool IsActive => isActive;
        public float CurrentAzimuth => currentAzimuth;
        public float OrbitRadius => orbitRadius;
        public Vector3 ProbePosition => probeVisual != null ? probeVisual.position : transform.position;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            BuildProbeVisual();
            SetVisualActive(false);
        }

        private void Start()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
        }

        private void BuildProbeVisual()
        {
            GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualObj.name = "3D Audio Probe Orb";
            visualObj.transform.SetParent(transform, true);
            visualObj.transform.localScale = Vector3.one * 0.26f;

            Collider col = visualObj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            probeRenderer = visualObj.GetComponent<MeshRenderer>();
            Material glowMat = Resources.Load<Material>("Materials/CyanGlow");
            if (glowMat == null)
            {
                glowMat = new Material(Shader.Find("Standard"));
                glowMat.color = new Color(0.18f, 0.92f, 1f);
                glowMat.EnableKeyword("_EMISSION");
                glowMat.SetColor("_EmissionColor", new Color(0.18f, 0.92f, 1f) * 3f);
            }
            probeRenderer.material = glowMat;

            GameObject lightObj = new GameObject("Probe Light");
            lightObj.transform.SetParent(visualObj.transform, false);
            probeLight = lightObj.AddComponent<Light>();
            probeLight.type = LightType.Point;
            probeLight.color = new Color(0.18f, 0.92f, 1f);
            probeLight.intensity = 3.5f;
            probeLight.range = 5.0f;
            probeLight.shadows = LightShadows.None;

            probeVisual = visualObj.transform;
        }

        private void SetVisualActive(bool active)
        {
            if (probeVisual != null)
            {
                probeVisual.gameObject.SetActive(active);
            }
        }

        public void Toggle()
        {
            SetActive(!isActive);
        }

        public void SetActive(bool active)
        {
            isActive = active;
            GameSettings.SpatialAudioDemoActive = active;
            SetVisualActive(active);

            if (active)
            {
                if (playerCamera == null) playerCamera = Camera.main;
                currentAngle = 0f;
                nextPingTime = Time.time + 0.1f;
            }
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            // Update Orbit Position around Camera
            currentAngle += orbitSpeed * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;

            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 center = playerCamera.transform.position;
            Vector3 offset = new Vector3(Mathf.Sin(rad) * orbitRadius, 0f, Mathf.Cos(rad) * orbitRadius);
            Vector3 targetPos = center + offset;

            probeVisual.position = targetPos;

            // Calculate Azimuth relative to Camera Forward (0° = Ahead, 90° = Right, 180° = Behind, -90° = Left)
            Vector3 toProbe = (targetPos - center);
            Vector3 flatCamFwd = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized;
            Vector3 flatToProbe = Vector3.ProjectOnPlane(toProbe, Vector3.up).normalized;

            if (flatCamFwd.sqrMagnitude > 0.001f && flatToProbe.sqrMagnitude > 0.001f)
            {
                currentAzimuth = Vector3.SignedAngle(flatCamFwd, flatToProbe, Vector3.up);
            }

            // Periodic Ping
            if (Time.time >= nextPingTime)
            {
                nextPingTime = Time.time + pingInterval;
                PlayProbePing(targetPos);
            }

            // Gentle light pulsation
            if (probeLight != null)
            {
                probeLight.intensity = 2.8f + 1.2f * Mathf.Sin(Time.time * 16f);
            }
        }

        private void PlayProbePing(Vector3 position)
        {
            AudioClip pingClip = SpatialAudioUtility.GetProbePingClip();
            SpatialAudioUtility.PlayClipAtPoint3D(
                pingClip,
                position,
                volume: 0.95f,
                minDistance: 1.0f,
                maxDistance: 22.0f,
                pitch: 1.0f,
                cueColor: new Color(0.18f, 0.92f, 1f)
            );
        }
    }
}
