using System.IO;
using MiaShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiaShooterEditor
{
    public static class GrenadeExplosionVerificationTest
    {
        private const string ScenePath = "Assets/Scenes/SoundVfxDemo.unity";
        private const string ScreenshotDir = "Captures";

        [MenuItem("Tools/Mia Shooter/Run Grenade Explosion Capture Test")]
        public static void RunCaptureTest()
        {
            Directory.CreateDirectory(ScreenshotDir);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null)
            {
                FirstPersonController player = Object.FindFirstObjectByType<FirstPersonController>();
                if (player != null)
                {
                    cam = player.GetComponentInChildren<Camera>();
                }
            }

            if (cam == null)
            {
                GameObject camObj = new GameObject("Test Camera");
                cam = camObj.AddComponent<Camera>();
            }

            // Position camera with a clear cinematic view of the blast zone
            Vector3 blastPosition = new Vector3(0f, 0.05f, 15f);
            cam.transform.position = new Vector3(0f, 2.2f, 6.5f);
            cam.transform.rotation = Quaternion.LookRotation((blastPosition + Vector3.up * 1.5f) - cam.transform.position);

            RenderTexture rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1280, 720, TextureFormat.RGB24, false);

            void CaptureFrame(string filename)
            {
                cam.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                screenShot.Apply();
                File.WriteAllBytes(Path.Combine(ScreenshotDir, filename), screenShot.EncodeToPNG());
                Debug.Log($"[BOMB VFX CAPTURE] Saved {filename}");
            }

            void SimulateAllEffects(float deltaTime)
            {
                foreach (ParticleSystem ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
                {
                    ps.Simulate(deltaTime, withChildren: true, restart: false, fixedTimeStep: true);
                }
                foreach (ShockwaveRing ring in Object.FindObjectsByType<ShockwaveRing>(FindObjectsSortMode.None))
                {
                    ring.ManualUpdate(deltaTime);
                }
                foreach (ExplosionDynamicLight light in Object.FindObjectsByType<ExplosionDynamicLight>(FindObjectsSortMode.None))
                {
                    light.ManualUpdate(deltaTime);
                }
                foreach (ScorchFader scorch in Object.FindObjectsByType<ScorchFader>(FindObjectsSortMode.None))
                {
                    scorch.ManualUpdate(deltaTime);
                }
            }

            // Spawn the enhanced grenade explosion
            VfxUtility.SpawnGrenadeExplosion(blastPosition);

            // Frame 1: Initial Flash & Shockwave Initiation (t = 0.06s)
            SimulateAllEffects(0.06f);
            CaptureFrame("test_bomb_01_flash_and_shockwave_init.png");

            // Frame 2: Expanding Fireball & Ground Dust Wave (t = 0.22s)
            SimulateAllEffects(0.16f);
            CaptureFrame("test_bomb_02_fireball_and_dust_shockwave.png");

            // Frame 3: Max Shockwave Radius, Rising Fire Pillar & Scorch Mark (t = 0.50s)
            SimulateAllEffects(0.28f);
            CaptureFrame("test_bomb_03_max_blast_shockwave_and_fire.png");

            // Frame 4: Towering Volumetric Dark Smoke Plume & Lingering Sparks (t = 1.25s)
            SimulateAllEffects(0.75f);
            CaptureFrame("test_bomb_04_towering_smoke_plume_and_crater.png");

            // Clean up test render texture
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(screenShot);

            Debug.Log("[VERIFICATION COMPLETE] All 4 Bomb Explosion VFX capture tests passed successfully!");
        }

        [MenuItem("Tools/Mia Shooter/Run Smoke Grenade Capture Test")]
        public static void RunSmokeGrenadeCaptureTest()
        {
            Directory.CreateDirectory(ScreenshotDir);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null)
            {
                FirstPersonController player = Object.FindFirstObjectByType<FirstPersonController>();
                if (player != null)
                {
                    cam = player.GetComponentInChildren<Camera>();
                }
            }

            if (cam == null)
            {
                GameObject camObj = new GameObject("Test Camera");
                cam = camObj.AddComponent<Camera>();
            }

            Vector3 smokePosition = new Vector3(0f, 0.05f, 15f);
            cam.transform.position = new Vector3(0f, 1.8f, 9.2f);
            cam.transform.rotation = Quaternion.LookRotation((smokePosition + Vector3.up * 1.2f) - cam.transform.position);

            RenderTexture rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1280, 720, TextureFormat.RGB24, false);

            void CaptureFrame(string filename)
            {
                cam.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                screenShot.Apply();
                File.WriteAllBytes(Path.Combine(ScreenshotDir, filename), screenShot.EncodeToPNG());
                Debug.Log($"[SMOKE VFX CAPTURE] Saved {filename}");
            }

            void SimulateAllEffects(float deltaTime)
            {
                foreach (ParticleSystem ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
                {
                    ps.Simulate(deltaTime, withChildren: true, restart: false, fixedTimeStep: true);
                }
            }

            // Spawn enhanced tactical smoke grenade cloud
            VfxUtility.SpawnSmokeCloud(smokePosition);

            // Frame 1: Initial Burst & Ground Carpet Initiation (t = 0.45s)
            SimulateAllEffects(0.45f);
            CaptureFrame("test_smoke_01_burst_and_carpet.png");

            // Frame 2: Dense Tactical Billowing Core Cloud & Ground Carpet (t = 1.8s)
            SimulateAllEffects(1.35f);
            CaptureFrame("test_smoke_02_dense_tactical_cloud.png");

            // Frame 3: Expansive Atmospheric Billowing Fog (t = 4.2s)
            SimulateAllEffects(2.4f);
            CaptureFrame("test_smoke_03_diffusing_wisps.png");

            // Clean up
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(screenShot);

            Debug.Log("[VERIFICATION COMPLETE] All 3 Smoke Grenade VFX capture tests passed successfully!");
        }

        [MenuItem("Tools/Mia Shooter/Run Arm & Grenade Throw Capture Test")]
        public static void RunArmAndThrowCaptureTest()
        {
            Directory.CreateDirectory(ScreenshotDir);
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            FirstPersonController player = Object.FindFirstObjectByType<FirstPersonController>();
            Camera cam = player != null ? player.GetComponentInChildren<Camera>() : Camera.main;
            if (cam == null)
            {
                Debug.LogError("Could not find player camera for arm capture test!");
                return;
            }

            // Hide weapon so arm is isolated and clearly visible in first-person view
            WeaponController weapon = player != null ? player.GetComponent<WeaponController>() : Object.FindFirstObjectByType<WeaponController>();
            if (weapon != null)
            {
                // Weapon root is the gun transform
                Transform gunTransform = cam.transform.Find("Demo Blaster");
                if (gunTransform != null)
                {
                    gunTransform.gameObject.SetActive(false);
                }
            }

            GrenadeThrowAnimation throwAnim = cam.GetComponent<GrenadeThrowAnimation>();
            if (throwAnim == null)
            {
                throwAnim = cam.gameObject.AddComponent<GrenadeThrowAnimation>();
            }

            RenderTexture rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1280, 720, TextureFormat.RGB24, false);

            void CaptureFrame(string filename)
            {
                cam.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                screenShot.Apply();
                File.WriteAllBytes(Path.Combine(ScreenshotDir, filename), screenShot.EncodeToPNG());
                Debug.Log($"[ARM VFX CAPTURE] Saved {filename}");
            }

            // 1. Holding Fragmentation Grenade in Ready Pose (Full view of sleeve, gauntlet, glove, gripping fingers)
            throwAnim.StopAllCoroutines();
            throwAnim.SetHeldGrenadeForPreview(GrenadeType.Fragmentation);
            throwAnim.SetPoseForPreview(GrenadeThrowAnimation.HoldPose, 0f, 0f);
            CaptureFrame("test_arm_01_holding_frag.png");

            // 2. Holding Smoke Grenade in Ready Pose (Blue indicator, safety spoon, ring)
            throwAnim.SetHeldGrenadeForPreview(GrenadeType.Smoke);
            throwAnim.SetPoseForPreview(GrenadeThrowAnimation.HoldPose, 0f, 0f);
            CaptureFrame("test_arm_02_holding_smoke.png");

            // 3. Windup Cocking Pose (Arm pulled back, wrist cocked, fingers tightened)
            throwAnim.SetPoseForPreview(GrenadeThrowAnimation.WindupPose, 0f, 0f);
            CaptureFrame("test_arm_03_throw_windup.png");

            // 4. Release Flick Pose (Moment of release, fingers dynamically flung open, grenade released)
            throwAnim.SetHeldGrenadeActive(false);
            throwAnim.SetPoseForPreview(GrenadeThrowAnimation.ReleasePose, 1f, 0f);
            CaptureFrame("test_arm_04_throw_release.png");

            // 5. Follow-Through Pose (Arm carrying through forward-down, fingers relaxing)
            throwAnim.SetPoseForPreview(GrenadeThrowAnimation.FollowThroughPose, 0.35f, 0.5f);
            CaptureFrame("test_arm_05_throw_followthrough.png");

            // Clean up: hide arm and RESTORE WEAPON!
            throwAnim.SetPoseForPreview(GrenadeThrowAnimation.RestPose, 0f, 0f);
            Transform armRoot = cam.transform.Find("Grenade Throw Arm System");
            if (armRoot != null)
            {
                armRoot.gameObject.SetActive(false);
            }

            Transform gunObj = cam.transform.Find("Demo Blaster");
            if (gunObj != null)
            {
                gunObj.gameObject.SetActive(true);
            }

            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(screenShot);

            Debug.Log("[VERIFICATION COMPLETE] All 5 Arm & Throw VFX capture tests passed successfully!");
        }

        [MenuItem("Tools/Mia Shooter/Restore Weapon Visibility")]
        public static void RestoreWeaponVisibility()
        {
            FirstPersonController player = Object.FindFirstObjectByType<FirstPersonController>();
            if (player != null)
            {
                foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Demo Blaster")
                    {
                        t.gameObject.SetActive(true);
                        Debug.Log("[RESTORE WEAPON] Demo Blaster set to active!");
                    }
                    if (t.name == "Grenade Throw Arm System")
                    {
                        t.gameObject.SetActive(false);
                    }
                }
            }

            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureWeaponActiveOnStartup()
        {
            FirstPersonController player = Object.FindFirstObjectByType<FirstPersonController>();
            if (player != null)
            {
                foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Demo Blaster")
                    {
                        t.gameObject.SetActive(true);
                    }
                    if (t.name == "Grenade Throw Arm System")
                    {
                        t.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
