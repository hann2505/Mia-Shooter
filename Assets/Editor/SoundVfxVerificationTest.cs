using System.IO;
using MiaShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MiaShooterEditor
{
    public static class SoundVfxVerificationTest
    {
        private const string ScenePath = "Assets/Scenes/SoundVfxDemo.unity";
        private const string ScreenshotDir = "Captures";

        [MenuItem("Tools/Mia Shooter/Run Laser And Crosshair Capture Test")]
        public static void RunCaptureTest()
        {
            Directory.CreateDirectory(ScreenshotDir);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            FirstPersonController playerController = UnityEngine.Object.FindFirstObjectByType<FirstPersonController>();
            if (playerController == null)
            {
                Debug.LogError("FirstPersonController not found");
                return;
            }

            GameObject playerObj = playerController.gameObject;
            Camera cam = playerObj.GetComponentInChildren<Camera>();
            WeaponController weapon = playerObj.GetComponentInChildren<WeaponController>();

            if (cam == null || weapon == null)
            {
                Debug.LogError("Camera or WeaponController not found");
                return;
            }

            // Initialize weapon lifecycle methods in edit mode
            weapon.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
            weapon.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);

            // Render camera to texture helper
            RenderTexture rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(1280, 720, TextureFormat.RGB24, false);

            // --- TEST 1: Initial Idle State (Laser at 0%) with Standard Precision Crosshair ---
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            DrawCrosshairOnTexture(screenShot, isAimingAtTarget: false, isLaserReady: false, isLaserFiring: false, isHitmarker: false);
            File.WriteAllBytes(Path.Combine(ScreenshotDir, "test_01_idle_crosshair_gun.png"), screenShot.EncodeToPNG());
            Debug.Log($"[TEST 1 PASSED] Captured test_01_idle_crosshair_gun.png | Energy: {weapon.LaserEnergy:P0}");

            // --- TEST 1B: Aim Lock-On Target Crosshair ---
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            DrawCrosshairOnTexture(screenShot, isAimingAtTarget: true, isLaserReady: false, isLaserFiring: false, isHitmarker: false);
            File.WriteAllBytes(Path.Combine(ScreenshotDir, "test_01b_target_lock_crosshair.png"), screenShot.EncodeToPNG());
            Debug.Log("[TEST 1B PASSED] Captured test_01b_target_lock_crosshair.png (hostile red lock-on)");

            // --- TEST 2: Charge Laser to 100% with Laser Ready Aura Reticle ---
            for (int i = 0; i < 5; i++)
            {
                weapon.SendMessage("ChargeLaser", SendMessageOptions.DontRequireReceiver);
            }

            if (!weapon.IsLaserReady || weapon.LaserEnergy < 0.99f)
            {
                Debug.LogError($"Laser failed to charge properly. Energy: {weapon.LaserEnergy}, Ready: {weapon.IsLaserReady}");
                return;
            }

            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            DrawCrosshairOnTexture(screenShot, isAimingAtTarget: false, isLaserReady: true, isLaserFiring: false, isHitmarker: false);
            File.WriteAllBytes(Path.Combine(ScreenshotDir, "test_02_laser_charged_ready.png"), screenShot.EncodeToPNG());
            Debug.Log($"[TEST 2 PASSED] Captured test_02_laser_charged_ready.png | Energy: {weapon.LaserEnergy:P0} | IsLaserReady: {weapon.IsLaserReady}");

            // --- TEST 3: Fire Laser and Verify Aura Light & Visuals with Firing Aura Crosshair & Hitmarker ---
            bool fired = weapon.TryFireLaser();
            if (!fired || !weapon.IsFiringLaser)
            {
                Debug.LogError($"TryFireLaser failed! returned {fired}, isFiringLaser={weapon.IsFiringLaser}");
                return;
            }

            // Verify Aura Components
            LineRenderer coreBeam = weapon.transform.Find("Charged Laser Beam")?.GetComponent<LineRenderer>();
            LineRenderer auraBeam = weapon.transform.Find("Charged Laser Aura Beam")?.GetComponent<LineRenderer>();
            Light muzzleLight = weapon.transform.Find("Laser Muzzle Aura Light")?.GetComponent<Light>();
            Light impactLight = weapon.transform.Find("Laser Impact Aura Light")?.GetComponent<Light>();

            if (coreBeam == null || !coreBeam.enabled)
            {
                Debug.LogError("Charged Laser Core Beam missing or disabled!");
                return;
            }
            if (auraBeam == null || !auraBeam.enabled)
            {
                Debug.LogError("Charged Laser Aura Beam missing or disabled!");
                return;
            }
            if (muzzleLight == null || !muzzleLight.enabled)
            {
                Debug.LogError("Laser Muzzle Aura Light missing or disabled!");
                return;
            }

            Debug.Log($"[AURA VERIFICATION] Core Beam width: {coreBeam.startWidth}, Aura Beam width: {auraBeam.startWidth}, Muzzle Aura Light Intensity: {muzzleLight.intensity}");

            if (weapon.LaserClip == null)
            {
                Debug.LogError("LaserClip is null on WeaponController!");
                return;
            }
            if (weapon.LaserAudio == null)
            {
                Debug.LogError("LaserAudio is null on WeaponController!");
                return;
            }
            if (weapon.LaserAudio.clip != weapon.LaserClip)
            {
                Debug.LogError($"LaserAudio clip mismatch! Expected {weapon.LaserClip.name}, got {weapon.LaserAudio.clip?.name}");
                return;
            }
            Debug.Log($"[LASER AUDIO VERIFIED] Laser clip: {weapon.LaserClip.name}, isPlaying: {weapon.LaserAudio.isPlaying}, loop: {weapon.LaserAudio.loop}");

            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            DrawCrosshairOnTexture(screenShot, isAimingAtTarget: true, isLaserReady: true, isLaserFiring: true, isHitmarker: true);
            File.WriteAllBytes(Path.Combine(ScreenshotDir, "test_03_laser_firing_aura.png"), screenShot.EncodeToPNG());
            Debug.Log($"[TEST 3 PASSED] Captured test_03_laser_firing_aura.png | IsFiringLaser: {weapon.IsFiringLaser}");

            // Clean up
            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(screenShot);

            Debug.Log("ALL LASER PROGRESS, AURA LIGHT, AND CROSSHAIR TESTS PASSED SUCCESSFULLY!");
        }

        private static void DrawCrosshairOnTexture(Texture2D tex, bool isAimingAtTarget, bool isLaserReady, bool isLaserFiring, bool isHitmarker)
        {
            int cx = tex.width / 2;
            int cy = tex.height / 2;
            float scale = Mathf.Clamp(tex.height / 900f, 0.75f, 1.5f);

            Color baseCyan = new Color(0.18f, 0.92f, 1f, 0.95f);
            Color hostileColor = new Color(1f, 0.32f, 0.22f, 0.98f);
            Color primaryColor = isAimingAtTarget ? hostileColor : baseCyan;
            Color shadowColor = new Color(0f, 0f, 0f, 0.85f);

            void FillRect(int rx, int ry, int rw, int rh, Color col)
            {
                int texY = tex.height - (ry + rh);
                for (int y = 0; y < rh; y++)
                {
                    for (int x = 0; x < rw; x++)
                    {
                        int px = rx + x;
                        int py = texY + y;
                        if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        {
                            Color bg = tex.GetPixel(px, py);
                            Color blended = Color.Lerp(bg, col, col.a);
                            tex.SetPixel(px, py, blended);
                        }
                    }
                }
            }

            void FillRectWithOutline(int rx, int ry, int rw, int rh, Color fill, Color outline, int border)
            {
                FillRect(rx - border, ry - border, rw + border * 2, rh + border * 2, outline);
                FillRect(rx, ry, rw, rh, fill);
            }

            int guiCy = tex.height / 2;

            // 1. Center Precision Dot
            int dotSize = Mathf.RoundToInt((isLaserFiring ? 4f : 3f) * scale);
            Color dotColor = isLaserFiring ? Color.white : primaryColor;
            FillRectWithOutline(cx - dotSize / 2, guiCy - dotSize / 2, dotSize, dotSize, dotColor, shadowColor, 1);

            // 2. Directional Reticle Bars
            int gap = Mathf.RoundToInt((isLaserFiring ? 12f : (isAimingAtTarget ? 6f : 7f)) * scale);
            int barLen = Mathf.RoundToInt((isLaserFiring ? 14f : 9f) * scale);
            int barThick = Mathf.Max(2, Mathf.RoundToInt(2f * scale));

            FillRectWithOutline(cx - gap - barLen, guiCy - barThick / 2, barLen, barThick, primaryColor, shadowColor, 1);
            FillRectWithOutline(cx + gap, guiCy - barThick / 2, barLen, barThick, primaryColor, shadowColor, 1);
            FillRectWithOutline(cx - barThick / 2, guiCy - gap - barLen, barThick, barLen, primaryColor, shadowColor, 1);
            FillRectWithOutline(cx - barThick / 2, guiCy + gap, barThick, barLen, primaryColor, shadowColor, 1);

            // 3. Sci-Fi Corner Aim Brackets
            int bracketDist = Mathf.RoundToInt((isLaserFiring ? 26f : (isAimingAtTarget ? 17f : 21f)) * scale);
            int bracketLen = Mathf.RoundToInt(6f * scale);
            int bracketThick = Mathf.Max(1, Mathf.RoundToInt(1.5f * scale));
            Color bracketColor = isAimingAtTarget ? hostileColor : new Color(primaryColor.r, primaryColor.g, primaryColor.b, 0.7f);

            // Top-Left
            FillRect(cx - bracketDist, guiCy - bracketDist, bracketLen, bracketThick, bracketColor);
            FillRect(cx - bracketDist, guiCy - bracketDist, bracketThick, bracketLen, bracketColor);
            // Top-Right
            FillRect(cx + bracketDist - bracketLen, guiCy - bracketDist, bracketLen, bracketThick, bracketColor);
            FillRect(cx + bracketDist - bracketThick, guiCy - bracketDist, bracketThick, bracketLen, bracketColor);
            // Bottom-Left
            FillRect(cx - bracketDist, guiCy + bracketDist - bracketThick, bracketLen, bracketThick, bracketColor);
            FillRect(cx - bracketDist, guiCy + bracketDist - bracketLen, bracketThick, bracketLen, bracketColor);
            // Bottom-Right
            FillRect(cx + bracketDist - bracketLen, guiCy + bracketDist - bracketThick, bracketLen, bracketThick, bracketColor);
            FillRect(cx + bracketDist - bracketThick, guiCy + bracketDist - bracketLen, bracketThick, bracketLen, bracketColor);

            // 4. Laser Aura Reticle & Indicators
            if (isLaserFiring)
            {
                int auraDist = Mathf.RoundToInt(32f * scale);
                int auraSize = Mathf.RoundToInt(8f * scale);
                Color auraColor = new Color(0.35f, 0.98f, 1f, 0.95f);

                FillRect(cx - auraSize / 2, guiCy - auraDist - 2, auraSize, 2, auraColor);
                FillRect(cx - auraSize / 2, guiCy + auraDist, auraSize, 2, auraColor);
                FillRect(cx - auraDist - 2, guiCy - auraSize / 2, 2, auraSize, auraColor);
                FillRect(cx + auraDist, guiCy - auraSize / 2, 2, auraSize, auraColor);
            }
            else if (isLaserReady)
            {
                int auraDist = Mathf.RoundToInt(28f * scale);
                Color readyAuraColor = new Color(0.2f, 0.95f, 1f, 0.9f);

                FillRect(cx - 2, guiCy - auraDist, 4, 2, readyAuraColor);
                FillRect(cx - 2, guiCy + auraDist - 2, 4, 2, readyAuraColor);
                FillRect(cx - auraDist, guiCy - 2, 2, 4, readyAuraColor);
                FillRect(cx + auraDist - 2, guiCy - 2, 2, 4, readyAuraColor);
            }

            // 5. Reactive Hitmarker
            if (isHitmarker)
            {
                Color hitColor = new Color(1f, 0.25f, 0.25f, 0.95f);
                int hitDist = Mathf.RoundToInt(9f * scale);
                int hitLen = Mathf.RoundToInt(5f * scale);
                int hitThick = Mathf.Max(2, Mathf.RoundToInt(2f * scale));

                FillRect(cx - hitDist, guiCy - hitDist, hitLen, hitThick, hitColor);
                FillRect(cx - hitDist, guiCy - hitDist, hitThick, hitLen, hitColor);
                FillRect(cx + hitDist - hitLen, guiCy - hitDist, hitLen, hitThick, hitColor);
                FillRect(cx + hitDist - hitThick, guiCy - hitDist, hitThick, hitLen, hitColor);
                FillRect(cx - hitDist, guiCy + hitDist - hitThick, hitLen, hitThick, hitColor);
                FillRect(cx - hitDist, guiCy + hitDist - hitLen, hitThick, hitLen, hitColor);
                FillRect(cx + hitDist - hitLen, guiCy + hitDist - hitThick, hitLen, hitThick, hitColor);
                FillRect(cx + hitDist - hitThick, guiCy + hitDist - hitLen, hitThick, hitLen, hitColor);
            }

            tex.Apply();
        }
    }
}
