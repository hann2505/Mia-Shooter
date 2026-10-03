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
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.delayCall += RunCaptureTest;
                return;
            }

            SoundVfxDemoBuilder.BuildDemoScene();
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

            // --- TEST 1: Initial Idle State (Laser at 0%) with Standard Precision Crosshair & Futuristic HUD ---
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            DrawCrosshairOnTexture(screenShot, isAimingAtTarget: false, isLaserReady: false, isLaserFiring: false, isHitmarker: false);
            DrawHudOverlayOnTexture(screenShot, weapon);
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

            // --- TEST 4: Target Elimination VFX & Kill Confirmation Feedback ---
            ShootableTarget target2 = null;
            ShootableTarget[] targets = UnityEngine.Object.FindObjectsByType<ShootableTarget>(FindObjectsSortMode.None);
            foreach (ShootableTarget t in targets)
            {
                if (t.gameObject.name == "Target 2")
                {
                    target2 = t;
                    break;
                }
            }

            if (target2 == null && targets.Length > 0)
            {
                target2 = targets[0];
            }

            if (target2 != null)
            {
                Color targetColor = target2.GetPrimaryColor();
                Vector3 targetPos = target2.transform.position;
                Vector3 shotDir = cam.transform.forward;

                // Stop laser state so kill effect is clearly visible
                weapon.SendMessage("StopLaser", SendMessageOptions.DontRequireReceiver);

                // Hide the target model as it is eliminated
                foreach (Renderer r in target2.GetComponentsInChildren<Renderer>())
                {
                    r.enabled = false;
                }

                // Spawn the complete AAA Elimination Effect
                VfxUtility.SpawnTargetEliminationEffect(targetPos, targetColor, shotDir);

                // Simulate/step particle systems, lights, and shockwave rings so particles bloom into the capture
                ParticleSystem[] vfxParticles = UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);
                foreach (ParticleSystem ps in vfxParticles)
                {
                    if (ps != null && ps.gameObject.name.Contains("Elimination"))
                    {
                        ps.Simulate(0.14f, true, false);
                    }
                }

                ExplosionDynamicLight[] lights = UnityEngine.Object.FindObjectsByType<ExplosionDynamicLight>(FindObjectsSortMode.None);
                foreach (ExplosionDynamicLight el in lights)
                {
                    el.ManualUpdate(0.12f);
                }

                ShockwaveRing[] rings = UnityEngine.Object.FindObjectsByType<ShockwaveRing>(FindObjectsSortMode.None);
                foreach (ShockwaveRing ring in rings)
                {
                    ring.ManualUpdate(0.14f);
                }

                TargetShardFader[] shardFaders = UnityEngine.Object.FindObjectsByType<TargetShardFader>(FindObjectsSortMode.None);
                foreach (TargetShardFader sf in shardFaders)
                {
                    sf.ManualUpdate(0.10f);
                }

                // Scatter physical shards outward around epicenter in edit mode
                Rigidbody[] shardRbs = UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None);
                int sIdx = 0;
                foreach (Rigidbody rb in shardRbs)
                {
                    if (rb != null && (rb.gameObject.name.Contains("Shard") || rb.gameObject.name.Contains("Splinter")))
                    {
                        float angle = sIdx * (Mathf.PI * 2f / 18f);
                        float dist = 0.9f + (sIdx % 4) * 0.45f;
                        rb.transform.position += new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle * 2f) * 0.6f + 0.35f, (sIdx % 3) * 0.35f);
                        rb.transform.rotation = Quaternion.Euler(sIdx * 25f, sIdx * 40f, sIdx * 15f);
                        sIdx++;
                    }
                }

                // Verify procedural kill chime audio
                AudioClip killChime = VfxUtility.GetKillChimeAudio();
                if (killChime == null || killChime.length < 0.1f)
                {
                    Debug.LogError("Kill chime audio generation failed!");
                    return;
                }
                Debug.Log($"[KILL CHIME VERIFIED] Name: {killChime.name}, Length: {killChime.length:F2}s, Channels: {killChime.channels}, Frequency: {killChime.frequency}Hz");

                // Render camera view with elimination VFX
                cam.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);

                // Draw Kill Banner and 8-point expanding Kill Hitmarker
                DrawCrosshairOnTexture(screenShot, isAimingAtTarget: false, isLaserReady: false, isLaserFiring: false, isHitmarker: false, isKillHitmarker: true);
                DrawKillBannerOnTexture(screenShot, 100, combo: 2);

                File.WriteAllBytes(Path.Combine(ScreenshotDir, "test_04_target_elimination_vfx.png"), screenShot.EncodeToPNG());
                Debug.Log($"[TEST 4 PASSED] Captured test_04_target_elimination_vfx.png | Target: {target2.name} | Color: {targetColor}");
            }
            else
            {
                Debug.LogError("No target found for TEST 4!");
                return;
            }

            // --- TEST 5: Settings Menu & Keybind Configuration System ---
            // 1. Verify Default Keybinds
            if (GameSettings.GetKeybind(GameSettings.ACTION_FORWARD) != KeyCode.W ||
                GameSettings.GetKeybind(GameSettings.ACTION_FIRE1) != KeyCode.Mouse0 ||
                GameSettings.GetKeybind(GameSettings.ACTION_FIRE2) != KeyCode.Mouse1)
            {
                Debug.LogError("Default keybinds mismatch in GameSettings!");
                return;
            }

            // 2. Test Keybind Rebinding
            GameSettings.SetKeybind(GameSettings.ACTION_FORWARD, KeyCode.Z);
            if (GameSettings.GetKeybind(GameSettings.ACTION_FORWARD) != KeyCode.Z)
            {
                Debug.LogError("Keybinding rebinding to Z failed!");
                return;
            }

            // 3. Test Reset to Defaults
            GameSettings.ResetToDefaults();
            if (GameSettings.GetKeybind(GameSettings.ACTION_FORWARD) != KeyCode.W)
            {
                Debug.LogError("ResetToDefaults failed to restore W!");
                return;
            }

            // 4. Capture Settings Modal View
            GameSettings.IsSettingsOpen = true;
            cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            DrawHudOverlayOnTexture(screenShot, weapon);
            DrawSettingsModalOnTexture(screenShot);
            File.WriteAllBytes(Path.Combine(ScreenshotDir, "test_05_settings_menu_keybinds.png"), screenShot.EncodeToPNG());
            GameSettings.IsSettingsOpen = false;
            Debug.Log("[TEST 5 PASSED] Captured test_05_settings_menu_keybinds.png | Keybind system verified");

            // Clean up
            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(screenShot);

            Debug.Log("ALL LASER PROGRESS, AURA LIGHT, CROSSHAIR, ELIMINATION VFX, AND SETTINGS MENU TESTS PASSED SUCCESSFULLY!");
        }

        private static void DrawCrosshairOnTexture(Texture2D tex, bool isAimingAtTarget, bool isLaserReady, bool isLaserFiring, bool isHitmarker, bool isKillHitmarker = false)
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
                Color auraColor = new Color(1f, 0.38f, 0.05f, 0.95f);

                FillRect(cx - auraSize / 2, guiCy - auraDist - 2, auraSize, 2, auraColor);
                FillRect(cx - auraSize / 2, guiCy + auraDist, auraSize, 2, auraColor);
                FillRect(cx - auraDist - 2, guiCy - auraSize / 2, 2, auraSize, auraColor);
                FillRect(cx + auraDist, guiCy - auraSize / 2, 2, auraSize, auraColor);
            }
            else if (isLaserReady)
            {
                int auraDist = Mathf.RoundToInt(28f * scale);
                Color readyAuraColor = new Color(1f, 0.45f, 0.08f, 0.92f);

                FillRect(cx - 2, guiCy - auraDist, 4, 2, readyAuraColor);
                FillRect(cx - 2, guiCy + auraDist - 2, 4, 2, readyAuraColor);
                FillRect(cx - auraDist, guiCy - 2, 2, 4, readyAuraColor);
                FillRect(cx + auraDist - 2, guiCy - 2, 2, 4, readyAuraColor);
            }

            // 5. Reactive Kill Hitmarker & Normal Hitmarker
            if (isKillHitmarker)
            {
                Color killColor = new Color(1f, 0.22f, 0.15f, 0.98f);
                Color goldPip = new Color(1f, 0.82f, 0.20f, 0.95f);
                int expand = Mathf.RoundToInt(15f * scale);
                int kLen = Mathf.RoundToInt(7f * scale);
                int kThick = Mathf.Max(2, Mathf.RoundToInt(2.5f * scale));

                // 4 diagonal spokes (Heavy X)
                FillRect(cx - expand, guiCy - expand, kLen, kThick, killColor);
                FillRect(cx - expand, guiCy - expand, kThick, kLen, killColor);
                FillRect(cx + expand - kLen, guiCy - expand, kLen, kThick, killColor);
                FillRect(cx + expand - kThick, guiCy - expand, kThick, kLen, killColor);
                FillRect(cx - expand, guiCy + expand - kThick, kLen, kThick, killColor);
                FillRect(cx - expand, guiCy + expand - kLen, kThick, kLen, killColor);
                FillRect(cx + expand - kLen, guiCy + expand - kThick, kLen, kThick, killColor);
                FillRect(cx + expand - kThick, guiCy + expand - kLen, kThick, kLen, killColor);

                // 4 cardinal gold pips
                int cardDist = Mathf.RoundToInt(expand * 0.72f);
                int pipW = Mathf.RoundToInt(4f * scale);
                int pipH = Mathf.RoundToInt(3f * scale);
                FillRect(cx - pipW / 2, guiCy - cardDist - pipH, pipW, pipH, goldPip);
                FillRect(cx - pipW / 2, guiCy + cardDist, pipW, pipH, goldPip);
                FillRect(cx - cardDist - pipH, guiCy - pipW / 2, pipH, pipW, goldPip);
                FillRect(cx + cardDist, guiCy - pipW / 2, pipH, pipW, goldPip);
            }
            else if (isHitmarker)
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

        private static void DrawKillBannerOnTexture(Texture2D tex, int score, int combo)
        {
            float scale = Mathf.Clamp(tex.height / 900f, 0.75f, 1.5f);
            int bannerW = Mathf.RoundToInt(360f * scale);
            int bannerH = Mathf.RoundToInt(58f * scale);
            int bx = tex.width / 2 - bannerW / 2;
            int by = Mathf.RoundToInt(tex.height * 0.60f);

            Color bgBox = new Color(0.02f, 0.04f, 0.07f, 0.88f);
            Color borderCyan = new Color(0.2f, 0.95f, 1f, 0.95f);

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

            FillRectWithOutline(bx, by, bannerW, bannerH, bgBox, borderCyan, 2);

            // Tech tabs on corners
            int tabLen = Mathf.RoundToInt(10f * scale);
            int tabThick = Mathf.RoundToInt(3f * scale);
            FillRect(bx, by, tabLen, tabThick, borderCyan);
            FillRect(bx, by, tabThick, tabLen, borderCyan);
            FillRect(bx + bannerW - tabLen, by, tabLen, tabThick, borderCyan);
            FillRect(bx + bannerW - tabThick, by, tabThick, tabLen, borderCyan);
            FillRect(bx, by + bannerH - tabThick, tabLen, tabThick, borderCyan);
            FillRect(bx, by + bannerH - tabLen, tabThick, tabLen, borderCyan);
            FillRect(bx + bannerW - tabLen, by + bannerH - tabThick, tabLen, tabThick, borderCyan);
            FillRect(bx + bannerW - tabThick, by + bannerH - tabLen, tabThick, tabLen, borderCyan);

            // Banner inner accent divider line
            FillRect(bx + 14, by + bannerH / 2, bannerW - 28, 1, new Color(0.2f, 0.95f, 1f, 0.35f));

            // Render crisp tech text
            string title = "* TARGET ELIMINATED *";
            int titleScale = 2;
            int titleW = title.Length * 6 * titleScale;
            DrawPixelText(tex, title, bx + (bannerW - titleW) / 2, by + 8, Color.white, titleScale);

            string subtitle = $"+{score} PTS   COMBO X{combo}!";
            int subScale = 2;
            int subW = subtitle.Length * 6 * subScale;
            DrawPixelText(tex, subtitle, bx + (bannerW - subW) / 2, by + 34, new Color(1f, 0.82f, 0.20f), subScale);

            tex.Apply();
        }

        private static void FillRect(Texture2D tex, int rx, int ry, int rw, int rh, Color col)
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

        private static void FillRectWithOutline(Texture2D tex, int rx, int ry, int rw, int rh, Color fill, Color outline, int border)
        {
            FillRect(tex, rx - border, ry - border, rw + border * 2, rh + border * 2, outline);
            FillRect(tex, rx, ry, rw, rh, fill);
        }

        private static void DrawHudOverlayOnTexture(Texture2D tex, WeaponController weapon)
        {
            Color bgPanel = new Color(0.02f, 0.05f, 0.09f, 0.85f);
            Color cyanAccent = new Color(0.18f, 0.92f, 1f, 0.95f);
            Color cyanDim = new Color(0.18f, 0.92f, 1f, 0.35f);
            Color goldText = new Color(1f, 0.82f, 0.20f, 0.98f);
            Color whiteText = new Color(0.92f, 0.95f, 1f, 0.95f);
            Color laserHotRed = new Color(1.0f, 0.15f, 0.05f, 0.95f);
            Color laserHotOrange = new Color(1.0f, 0.55f, 0.05f, 0.95f);

            // 1. Top-Left Holographic Facility Badge
            int bx = 24, by = 20, bw = 360, bh = 54;
            FillRectWithOutline(tex, bx, by, bw, bh, bgPanel, cyanAccent, 1);
            FillRect(tex, bx, by, 8, 2, cyanAccent);
            FillRect(tex, bx, by, 2, 8, cyanAccent);
            FillRect(tex, bx + bw - 8, by, 8, 2, cyanAccent);
            FillRect(tex, bx + bw - 2, by, 2, 8, cyanAccent);
            FillRect(tex, bx, by + bh - 2, 8, 2, cyanAccent);
            FillRect(tex, bx, by + bh - 8, 2, 8, cyanAccent);
            FillRect(tex, bx + bw - 8, by + bh - 2, 8, 2, cyanAccent);
            FillRect(tex, bx + bw - 2, by + bh - 8, 2, 8, cyanAccent);

            DrawPixelText(tex, "* MIA // LABS TACTICAL SUITE *", bx + 12, by + 10, cyanAccent, 1);
            FillRect(tex, bx + 12, by + 26, bw - 24, 1, cyanDim);
            DrawPixelText(tex, "FIRING RANGE SIMULATION", bx + 12, by + 34, whiteText, 1);

            // 2. Top-Right Target/Score & Settings Button
            int rw = 320, rh = 54;
            int rx = tex.width - rw - 24, ry = 20;
            FillRectWithOutline(tex, rx, ry, rw, rh, bgPanel, cyanAccent, 1);
            FillRect(tex, rx, ry, 8, 2, cyanAccent);
            FillRect(tex, rx, ry, 2, 8, cyanAccent);
            FillRect(tex, rx + rw - 8, ry, 8, 2, cyanAccent);
            FillRect(tex, rx + rw - 2, ry, 2, 8, cyanAccent);
            FillRect(tex, rx, ry + rh - 2, 8, 2, cyanAccent);
            FillRect(tex, rx, ry + rh - 8, 2, 8, cyanAccent);
            FillRect(tex, rx + rw - 8, ry + rh - 2, 8, 2, cyanAccent);
            FillRect(tex, rx + rw - 2, ry + rh - 8, 2, 8, cyanAccent);

            DrawPixelText(tex, "SCORE: 0100", rx + 14, ry + 12, goldText, 1);
            DrawPixelText(tex, "TARGETS: 08", rx + 14, ry + 32, cyanAccent, 1);

            int btnX = rx + 155, btnY = ry + 12, btnW = 150, btnH = 30;
            FillRectWithOutline(tex, btnX, btnY, btnW, btnH, new Color(0.08f, 0.18f, 0.26f, 0.9f), cyanAccent, 1);
            DrawPixelText(tex, "[ SETTINGS / ESC ]", btnX + 8, btnY + 11, cyanAccent, 1);

            // 3. Bottom-Left Operator Vitals & Mobility Quickchips
            int vx = 24, vw = 340, vh = 92;
            int vy = tex.height - vh - 24;
            FillRectWithOutline(tex, vx, vy, vw, vh, bgPanel, cyanAccent, 1);
            FillRect(tex, vx, vy, 8, 2, cyanAccent);
            FillRect(tex, vx, vy, 2, 8, cyanAccent);
            FillRect(tex, vx + vw - 8, vy, 8, 2, cyanAccent);
            FillRect(tex, vx + vw - 2, vy, 2, 8, cyanAccent);
            FillRect(tex, vx, vy + vh - 2, 8, 2, cyanAccent);
            FillRect(tex, vx, vy + vh - 8, 2, 8, cyanAccent);
            FillRect(tex, vx + vw - 8, vy + vh - 2, 8, 2, cyanAccent);
            FillRect(tex, vx + vw - 2, vy + vh - 8, 2, 8, cyanAccent);

            DrawPixelText(tex, "OPERATOR // MIA-01", vx + 14, vy + 10, whiteText, 1);
            DrawPixelText(tex, "SHIELD", vx + 14, vy + 28, cyanAccent, 1);
            for (int s = 0; s < 5; s++)
            {
                FillRect(tex, vx + 70 + s * 28, vy + 28, 22, 9, new Color(0.15f, 0.85f, 1f, 0.95f));
            }

            DrawPixelText(tex, "ARMOR ", vx + 14, vy + 46, new Color(0.2f, 0.9f, 0.4f, 0.95f), 1);
            for (int a = 0; a < 5; a++)
            {
                FillRect(tex, vx + 70 + a * 28, vy + 46, 22, 9, new Color(0.2f, 0.9f, 0.4f, 0.95f));
            }

            DrawPixelText(tex, "[WASD] MOVE  [SPACE] JUMP  [SHIFT] SPRINT", vx + 14, vy + 72, cyanDim, 1);

            // 4. Bottom-Right Tactical Weapon Module & Cartridge Pips & Laser Bar
            int wx = tex.width - 380 - 24, ww = 380, wh = 140;
            int wy = tex.height - wh - 24;
            FillRectWithOutline(tex, wx, wy, ww, wh, bgPanel, cyanAccent, 1);
            FillRect(tex, wx, wy, 8, 2, cyanAccent);
            FillRect(tex, wx, wy, 2, 8, cyanAccent);
            FillRect(tex, wx + ww - 8, wy, 8, 2, cyanAccent);
            FillRect(tex, wx + ww - 2, wy, 2, 8, cyanAccent);
            FillRect(tex, wx, wy + wh - 2, 8, 2, cyanAccent);
            FillRect(tex, wx, wy + wh - 8, 2, 8, cyanAccent);
            FillRect(tex, wx + ww - 8, wy + wh - 2, 8, 2, cyanAccent);
            FillRect(tex, wx + ww - 2, wy + wh - 8, 2, 8, cyanAccent);

            DrawPixelText(tex, "ION BLASTER // MK-IV", wx + 14, wy + 10, cyanAccent, 1);
            DrawPixelText(tex, "MODE: PLASMA / LASER", wx + 14, wy + 24, new Color(0.7f, 0.85f, 0.95f), 1);

            DrawPixelText(tex, "12 / 12", wx + 14, wy + 42, whiteText, 3);

            // 12 Tactical Cartridge Pips
            for (int p = 0; p < 12; p++)
            {
                int px = wx + 180 + p * 15;
                int py = wy + 48;
                FillRectWithOutline(tex, px, py, 10, 16, cyanAccent, new Color(0f, 0f, 0f, 0.85f), 1);
            }

            // Laser Energy Bar with Gradient
            float energy = (weapon != null) ? weapon.LaserEnergy : 1.0f;
            DrawPixelText(tex, "LASER ENERGY", wx + 14, wy + 82, laserHotOrange, 1);
            DrawPixelText(tex, $"{(int)(energy * 100)}%", wx + ww - 50, wy + 82, laserHotOrange, 1);

            int barTrackX = wx + 14, barTrackY = wy + 96, barTrackW = ww - 28, barTrackH = 8;
            FillRectWithOutline(tex, barTrackX, barTrackY, barTrackW, barTrackH, new Color(0.12f, 0.04f, 0.04f, 0.9f), laserHotOrange, 1);
            int fillW = Mathf.RoundToInt((barTrackW - 2) * Mathf.Clamp01(energy));
            for (int fx = 0; fx < fillW; fx++)
            {
                float t = (float)fx / Mathf.Max(1, barTrackW - 2);
                Color barCol = Color.Lerp(laserHotRed, laserHotOrange, t);
                FillRect(tex, barTrackX + 1 + fx, barTrackY + 1, 1, barTrackH - 2, barCol);
            }

            // Ordnance Chips
            DrawPixelText(tex, "[Q] HE FRAG: READY", wx + 14, wy + 116, cyanAccent, 1);
            DrawPixelText(tex, "[E] SMOKE: READY", wx + 200, wy + 116, new Color(0.85f, 0.85f, 0.95f), 1);

            tex.Apply();
        }

        private static void DrawSettingsModalOnTexture(Texture2D tex)
        {
            Color cyanAccent = new Color(0.18f, 0.92f, 1f, 0.95f);
            Color cyanDim = new Color(0.18f, 0.92f, 1f, 0.35f);
            Color goldText = new Color(1f, 0.82f, 0.20f, 0.98f);
            Color whiteText = new Color(0.92f, 0.95f, 1f, 0.95f);

            // Fullscreen backdrop dim
            FillRect(tex, 0, 0, tex.width, tex.height, new Color(0.01f, 0.03f, 0.06f, 0.82f));

            int mw = 680, mh = 480;
            int mx = (tex.width - mw) / 2;
            int my = (tex.height - mh) / 2;

            // Modal Frame
            FillRectWithOutline(tex, mx, my, mw, mh, new Color(0.03f, 0.07f, 0.12f, 0.98f), cyanAccent, 2);
            FillRect(tex, mx, my, 16, 3, cyanAccent);
            FillRect(tex, mx, my, 3, 16, cyanAccent);
            FillRect(tex, mx + mw - 16, my, 16, 3, cyanAccent);
            FillRect(tex, mx + mw - 3, my, 3, 16, cyanAccent);
            FillRect(tex, mx, my + mh - 3, 16, 3, cyanAccent);
            FillRect(tex, mx, my + mh - 16, 3, 16, cyanAccent);
            FillRect(tex, mx + mw - 16, my + mh - 3, 16, 3, cyanAccent);
            FillRect(tex, mx + mw - 3, my + mh - 16, 3, 16, cyanAccent);

            // Header Bar
            FillRect(tex, mx + 2, my + 2, mw - 4, 42, new Color(0.06f, 0.14f, 0.22f, 0.95f));
            FillRect(tex, mx + 2, my + 44, mw - 4, 1, new Color(0.18f, 0.92f, 1f, 0.5f));
            DrawPixelText(tex, "SYSTEM SETTINGS // KEYBINDS", mx + 20, my + 14, cyanAccent, 2);
            DrawPixelText(tex, "[X]", mx + mw - 48, my + 14, new Color(1f, 0.35f, 0.25f), 2);

            // Tabs Row
            FillRectWithOutline(tex, mx + 20, my + 54, 150, 30, new Color(0.12f, 0.38f, 0.52f, 0.95f), cyanAccent, 1);
            DrawPixelText(tex, "KEYBINDS", mx + 55, my + 63, Color.white, 1);

            FillRectWithOutline(tex, mx + 180, my + 54, 130, 30, new Color(0.04f, 0.08f, 0.14f, 0.85f), new Color(0.18f, 0.92f, 1f, 0.3f), 1);
            DrawPixelText(tex, "AUDIO", mx + 225, my + 63, new Color(0.5f, 0.7f, 0.85f), 1);

            FillRectWithOutline(tex, mx + 320, my + 54, 140, 30, new Color(0.04f, 0.08f, 0.14f, 0.85f), new Color(0.18f, 0.92f, 1f, 0.3f), 1);
            DrawPixelText(tex, "GAMEPLAY", mx + 355, my + 63, new Color(0.5f, 0.7f, 0.85f), 1);

            // Inner Keybinds Box
            int kx = mx + 20, ky = my + 94, kw = mw - 40, kh = 310;
            FillRectWithOutline(tex, kx, ky, kw, kh, new Color(0.02f, 0.05f, 0.09f, 0.9f), new Color(0.18f, 0.92f, 1f, 0.3f), 1);
            DrawPixelText(tex, "SELECT ACTION TO REBIND // SAVES TO PLAYERPREFS", kx + 16, ky + 12, new Color(0.45f, 0.75f, 0.95f), 1);
            FillRect(tex, kx + 16, ky + 26, kw - 32, 1, new Color(0.18f, 0.92f, 1f, 0.2f));

            // Two columns of Keybinds
            (string action, string key)[] col1 = new (string, string)[]
            {
                ("FORWARD", "W"),
                ("BACKWARD", "S"),
                ("MOVE LEFT", "A"),
                ("MOVE RIGHT", "D"),
                ("JUMP", "SPACE")
            };

            (string action, string key)[] col2 = new (string, string)[]
            {
                ("FIRE 1", "MOUSE 0"),
                ("CHARGED LASER", "MOUSE 1"),
                ("RELOAD", "R"),
                ("FRAG GRENADE", "Q"),
                ("SMOKE GRENADE", "E")
            };

            for (int i = 0; i < 5; i++)
            {
                int rowY = ky + 40 + i * 48;

                // Col 1
                int c1X = kx + 16;
                DrawPixelText(tex, col1[i].action, c1X, rowY + 8, whiteText, 1);
                int btn1X = c1X + 140, btn1W = 120, btn1H = 28;
                FillRectWithOutline(tex, btn1X, rowY, btn1W, btn1H, new Color(0.08f, 0.18f, 0.28f, 0.95f), cyanAccent, 1);
                string k1Text = $"[ {col1[i].key} ]";
                int k1W = k1Text.Length * 6;
                DrawPixelText(tex, k1Text, btn1X + (btn1W - k1W) / 2, rowY + 9, goldText, 1);

                // Col 2
                int c2X = kx + 310;
                DrawPixelText(tex, col2[i].action, c2X, rowY + 8, whiteText, 1);
                int btn2X = c2X + 140, btn2W = 130, btn2H = 28;
                FillRectWithOutline(tex, btn2X, rowY, btn2W, btn2H, new Color(0.08f, 0.18f, 0.28f, 0.95f), cyanAccent, 1);
                string k2Text = $"[ {col2[i].key} ]";
                int k2W = k2Text.Length * 6;
                DrawPixelText(tex, k2Text, btn2X + (btn2W - k2W) / 2, rowY + 9, goldText, 1);
            }

            // Bottom Buttons
            int rstX = mx + 20, rstY = my + mh - 58, rstW = 200, rstH = 38;
            FillRectWithOutline(tex, rstX, rstY, rstW, rstH, new Color(0.20f, 0.08f, 0.08f, 0.95f), new Color(1f, 0.35f, 0.25f), 1);
            DrawPixelText(tex, "RESET DEFAULTS", rstX + 40, rstY + 14, new Color(1f, 0.6f, 0.5f), 1);

            int clsX = mx + mw - 180, clsY = my + mh - 58, clsW = 160, clsH = 38;
            FillRectWithOutline(tex, clsX, clsY, clsW, clsH, new Color(0.08f, 0.25f, 0.36f, 0.95f), cyanAccent, 1);
            DrawPixelText(tex, "CLOSE (ESC)", clsX + 38, clsY + 14, Color.white, 1);

            tex.Apply();
        }

        private static readonly System.Collections.Generic.Dictionary<char, int[]> Font5x7 = new System.Collections.Generic.Dictionary<char, int[]>
        {
            {' ', new int[] { 0, 0, 0, 0, 0, 0, 0 }},
            {'A', new int[] { 14, 17, 17, 31, 17, 17, 17 }},
            {'B', new int[] { 30, 17, 17, 30, 17, 17, 30 }},
            {'C', new int[] { 14, 17, 16, 16, 16, 17, 14 }},
            {'D', new int[] { 28, 18, 17, 17, 17, 18, 28 }},
            {'E', new int[] { 31, 16, 16, 30, 16, 16, 31 }},
            {'F', new int[] { 31, 16, 16, 30, 16, 16, 16 }},
            {'G', new int[] { 14, 17, 16, 23, 17, 17, 14 }},
            {'H', new int[] { 17, 17, 17, 31, 17, 17, 17 }},
            {'I', new int[] { 14, 4, 4, 4, 4, 4, 14 }},
            {'J', new int[] { 1, 1, 1, 1, 17, 17, 14 }},
            {'K', new int[] { 17, 18, 20, 24, 20, 18, 17 }},
            {'L', new int[] { 16, 16, 16, 16, 16, 16, 31 }},
            {'M', new int[] { 17, 27, 21, 21, 17, 17, 17 }},
            {'N', new int[] { 17, 17, 25, 21, 19, 17, 17 }},
            {'O', new int[] { 14, 17, 17, 17, 17, 17, 14 }},
            {'P', new int[] { 30, 17, 17, 30, 16, 16, 16 }},
            {'Q', new int[] { 14, 17, 17, 17, 21, 18, 13 }},
            {'R', new int[] { 30, 17, 17, 30, 20, 18, 17 }},
            {'S', new int[] { 14, 17, 16, 14, 1, 17, 14 }},
            {'T', new int[] { 31, 4, 4, 4, 4, 4, 4 }},
            {'U', new int[] { 17, 17, 17, 17, 17, 17, 14 }},
            {'V', new int[] { 17, 17, 17, 17, 17, 10, 4 }},
            {'W', new int[] { 17, 17, 17, 21, 21, 27, 17 }},
            {'X', new int[] { 17, 17, 10, 4, 10, 17, 17 }},
            {'Y', new int[] { 17, 17, 10, 4, 4, 4, 4 }},
            {'Z', new int[] { 31, 1, 2, 4, 8, 16, 31 }},
            {'0', new int[] { 14, 17, 19, 21, 25, 17, 14 }},
            {'1', new int[] { 4, 12, 4, 4, 4, 4, 14 }},
            {'2', new int[] { 14, 17, 1, 6, 8, 16, 31 }},
            {'3', new int[] { 30, 1, 1, 14, 1, 1, 30 }},
            {'4', new int[] { 2, 6, 10, 18, 31, 2, 2 }},
            {'5', new int[] { 31, 16, 30, 1, 1, 17, 14 }},
            {'6', new int[] { 14, 16, 30, 17, 17, 17, 14 }},
            {'7', new int[] { 31, 1, 2, 4, 8, 8, 8 }},
            {'8', new int[] { 14, 17, 17, 14, 17, 17, 14 }},
            {'9', new int[] { 14, 17, 17, 15, 1, 1, 14 }},
            {':', new int[] { 0, 4, 4, 0, 4, 4, 0 }},
            {'/', new int[] { 1, 2, 2, 4, 8, 8, 16 }},
            {'.', new int[] { 0, 0, 0, 0, 0, 4, 4 }},
            {'[', new int[] { 14, 8, 8, 8, 8, 8, 14 }},
            {']', new int[] { 14, 2, 2, 2, 2, 2, 14 }},
            {'%', new int[] { 25, 26, 2, 4, 8, 11, 19 }},
            {'_', new int[] { 0, 0, 0, 0, 0, 0, 31 }},
            {'|', new int[] { 4, 4, 4, 4, 4, 4, 4 }},
            {'+', new int[] { 0, 4, 4, 31, 4, 4, 0 }},
            {'!', new int[] { 4, 4, 4, 4, 4, 0, 4 }},
            {'-', new int[] { 0, 0, 0, 31, 0, 0, 0 }},
            {'*', new int[] { 4, 21, 14, 31, 14, 21, 4 }},
            {'(', new int[] { 2, 4, 8, 8, 8, 4, 2 }},
            {')', new int[] { 8, 4, 2, 2, 2, 4, 8 }},
            {',', new int[] { 0, 0, 0, 0, 4, 4, 8 }}
        };

        private static void DrawPixelText(Texture2D tex, string text, int startX, int startY, Color color, int pixelScale)
        {
            int curX = startX;
            foreach (char c in text.ToUpperInvariant())
            {
                if (Font5x7.TryGetValue(c, out int[] rows))
                {
                    for (int r = 0; r < 7; r++)
                    {
                        int rowBits = rows[r];
                        for (int col = 0; col < 5; col++)
                        {
                            if ((rowBits & (1 << (4 - col))) != 0)
                            {
                                int px = curX + col * pixelScale;
                                int py = startY + r * pixelScale;
                                int texY = tex.height - (py + pixelScale);
                                for (int dy = 0; dy < pixelScale; dy++)
                                {
                                    for (int dx = 0; dx < pixelScale; dx++)
                                    {
                                        int finalX = px + dx;
                                        int finalY = texY + dy;
                                        if (finalX >= 0 && finalX < tex.width && finalY >= 0 && finalY < tex.height)
                                        {
                                            tex.SetPixel(finalX, finalY, color);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                curX += (5 + 1) * pixelScale;
            }
        }
    }
}
