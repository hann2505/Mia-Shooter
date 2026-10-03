using System;
using System.IO;
using MiaShooter;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiaShooterEditor
{
    public static class SoundVfxDemoBuilder
    {
        private const string ScenePath = "Assets/Scenes/SoundVfxDemo.unity";
        private static readonly string[] RequiredAudioPaths =
        {
            "Assets/Resources/Audio/gunshot.mp3",
            "Assets/Audio/impact.wav",
            "Assets/Audio/explosion.wav",
            "Assets/Resources/Audio/footstep.mp3",
            "Assets/Resources/Audio/reload.mp3",
            "Assets/Resources/Audio/land.mp3",
            "Assets/Audio/bomb-explosion.mp3",
            "Assets/Audio/smoke.mp3",
            "Assets/Audio/ambience.wav",
            "Assets/Audio/laser.mp3"
        };

        [MenuItem("Tools/Mia Shooter/Build Sound & VFX Demo")]
        public static void BuildDemoScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Materials");

            AudioClip gunshot = LoadAudio("Assets/Resources/Audio/gunshot.mp3");
            AudioClip impact = LoadAudio("Assets/Audio/impact.wav");
            AudioClip explosion = LoadAudio("Assets/Audio/explosion.wav");
            AudioClip footstep = LoadAudio("Assets/Resources/Audio/footstep.mp3");
            AudioClip reload = LoadAudio("Assets/Resources/Audio/reload.mp3");
            AudioClip land = LoadAudio("Assets/Resources/Audio/land.mp3");
            AudioClip grenadeExplosion = LoadAudio("Assets/Audio/bomb-explosion.mp3");
            AudioClip smokeGrenade = LoadAudio("Assets/Audio/smoke.mp3");
            AudioClip ambience = LoadAudio("Assets/Audio/ambience.wav");
            AudioClip laser = LoadAudio("Assets/Audio/laser.mp3");

            Material floorMaterial = CreateMaterial("Floor", new Color(0.055f, 0.075f, 0.095f), 0.55f, 0.72f);
            Material outdoorFloorMaterial = CreateMaterial("OutdoorFloor", new Color(0.10f, 0.11f, 0.13f), 0.25f, 0.45f);
            Material wallMaterial = CreateMaterial("Wall", new Color(0.095f, 0.12f, 0.15f), 0.35f, 0.55f);
            Material ceilingMaterial = CreateMaterial("Ceiling", new Color(0.06f, 0.07f, 0.085f), 0.45f, 0.5f);
            Material pillarMaterial = CreateMaterial("ConcretePillar", new Color(0.16f, 0.19f, 0.22f), 0.3f, 0.4f);
            Material hazardMaterial = CreateMaterial("HazardStripe", new Color(0.92f, 0.78f, 0.08f), 0.3f, 0.45f);
            Material containerGreen = CreateMaterial("ContainerGreen", new Color(0.14f, 0.25f, 0.17f), 0.65f, 0.5f);
            Material containerOrange = CreateMaterial("ContainerOrange", new Color(0.85f, 0.36f, 0.08f), 0.65f, 0.5f);
            Material containerBlue = CreateMaterial("ContainerBlue", new Color(0.12f, 0.34f, 0.52f), 0.65f, 0.5f);
            Material barrierMaterial = CreateMaterial("ConcreteBarrier", new Color(0.36f, 0.39f, 0.43f), 0.18f, 0.28f);
            Material cyanMaterial = CreateMaterial("TargetCyan", new Color(0.05f, 0.65f, 0.85f), 0.7f, 0.8f);
            Material orangeMaterial = CreateMaterial("TargetOrange", new Color(1f, 0.3f, 0.06f), 0.65f, 0.75f);
            Material darkMaterial = CreateMaterial("DarkMetal", new Color(0.035f, 0.045f, 0.055f), 0.8f, 0.8f);
            Material gunSteel = CreateMaterial("GunSteel", new Color(0.12f, 0.15f, 0.18f), 0.95f, 0.88f);
            Material gunPanel = CreateMaterial("GunPanel", new Color(0.035f, 0.042f, 0.05f), 0.55f, 0.45f);
            Material gunCeramic = CreateMaterial("GunCeramic", new Color(0.72f, 0.78f, 0.84f), 0.68f, 0.9f);
            Material cyanGlow = CreateEmissiveMaterial("CyanGlow", new Color(0.04f, 0.7f, 1f), 3.5f);
            Material orangeGlow = CreateEmissiveMaterial("OrangeGlow", new Color(1f, 0.38f, 0.04f), 3.5f);
            Material hotRedGlow = CreateEmissiveMaterial("HotRedGlow", new Color(1f, 0.14f, 0.03f), 3.6f);
            Material hotGoldGlow = CreateEmissiveMaterial("HotGoldGlow", new Color(1f, 0.82f, 0.15f), 3.2f);
            Material ceilingLampGlow = CreateEmissiveMaterial("CeilingLampGlow", new Color(0.85f, 0.95f, 1f), 3.0f);
            Material warningAmberGlow = CreateEmissiveMaterial("WarningAmberGlow", new Color(1f, 0.65f, 0.05f), 3.5f);
            Material opticGlass = CreateMaterial("OpticGlass", new Color(0.08f, 0.16f, 0.2f), 0.95f, 0.95f);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SoundVfxDemo";

            ConfigureEnvironment();
            BuildArena(floorMaterial, outdoorFloorMaterial, wallMaterial, ceilingMaterial, darkMaterial, pillarMaterial, hazardMaterial, containerGreen, containerOrange, containerBlue, barrierMaterial, cyanGlow, orangeGlow, ceilingLampGlow, warningAmberGlow);
            BuildLighting();
            BuildTargets(cyanMaterial, orangeMaterial, darkMaterial, hit: impact, explosion: explosion);
            BuildPlayer(gunshot, impact, footstep, reload, land, grenadeExplosion, smokeGrenade, laser, gunSteel, gunPanel, gunCeramic, cyanGlow, orangeGlow, hotRedGlow, hotGoldGlow, opticGlass, darkMaterial);
            BuildGameSystems(ambience);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.productName = "Mia Shooter - Sound and VFX Demo";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);

            Debug.Log($"Mia Shooter demo created successfully: {ScenePath}");
        }

        [MenuItem("Tools/Mia Shooter/Validate Demo Scene")]
        public static void ValidateDemoScene()
        {
            if (!File.Exists(ScenePath))
            {
                throw new InvalidOperationException($"Demo scene is missing: {ScenePath}");
            }

            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            ShootableTarget[] targets = UnityEngine.Object.FindObjectsByType<ShootableTarget>(FindObjectsSortMode.None);
            FirstPersonController player = UnityEngine.Object.FindFirstObjectByType<FirstPersonController>();
            WeaponController weapon = UnityEngine.Object.FindFirstObjectByType<WeaponController>();
            GrenadeThrower grenadeThrower = UnityEngine.Object.FindFirstObjectByType<GrenadeThrower>();
            DemoGameManager manager = UnityEngine.Object.FindFirstObjectByType<DemoGameManager>();
            AudioSource[] audioSources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            GameObject blaster = null;
            if (player != null)
            {
                foreach (Transform t in player.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Demo Blaster")
                    {
                        blaster = t.gameObject;
                        break;
                    }
                }
            }
            if (blaster == null)
            {
                blaster = GameObject.Find("Demo Blaster");
            }

            if (targets.Length != 8)
            {
                throw new InvalidOperationException($"Expected 8 targets (4 indoor, 4 outdoor), found {targets.Length}.");
            }

            if (player == null || weapon == null || grenadeThrower == null || manager == null)
            {
                throw new InvalidOperationException("The demo is missing a required gameplay component.");
            }

            if (weapon.LaserClip == null)
            {
                throw new InvalidOperationException("WeaponController is missing the laser audio clip.");
            }

            if (audioSources.Length < 3)
            {
                throw new InvalidOperationException($"Expected at least 3 audio sources, found {audioSources.Length}.");
            }

            if (blaster == null || blaster.GetComponentsInChildren<Renderer>().Length < 25 || blaster.transform.Find("Ion Core") == null)
            {
                throw new InvalidOperationException("The detailed blaster model is missing or incomplete.");
            }

            if (blaster.transform.Find("Laser Monitor Mount") == null
                || blaster.transform.Find("Laser Monitor Mount/Laser Gauge Fill") == null
                || blaster.transform.Find("Laser Monitor Mount/Laser Monitor Readout") == null)
            {
                throw new InvalidOperationException("The blaster laser energy progress display is missing or incomplete.");
            }

            foreach (string audioPath in RequiredAudioPaths)
            {
                if (AssetDatabase.LoadAssetAtPath<AudioClip>(audioPath) == null)
                {
                    throw new InvalidOperationException($"Audio clip is missing or invalid: {audioPath}");
                }
            }

            Debug.Log("Mia Shooter validation passed: scene, gameplay, targets, and audio assets are ready.");
        }

        private static void ConfigureEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.08f, 0.12f, 0.16f);
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.0055f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.32f, 0.44f, 0.58f);
            RenderSettings.ambientEquatorColor = new Color(0.15f, 0.20f, 0.26f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.06f, 0.08f);
        }

        private static void BuildArena(
            Material indoorFloor,
            Material outdoorFloor,
            Material wall,
            Material ceiling,
            Material dark,
            Material pillar,
            Material hazard,
            Material containerGreen,
            Material containerOrange,
            Material containerBlue,
            Material barrier,
            Material cyanGlow,
            Material orangeGlow,
            Material ceilingLampGlow,
            Material warningAmberGlow)
        {
            GameObject environment = new GameObject("ENVIRONMENT");

            // =========================================================================
            // 1. INDOOR ZONE: TACTICAL HANGAR & FIRING RANGE (z = -28m to +20m)
            // =========================================================================
            // Indoor Floor (Length 48m, Width 32m, centered at z = -4m)
            CreateCube("Indoor Floor", new Vector3(0f, -0.5f, -4f), new Vector3(32f, 1f, 48f), indoorFloor, environment.transform);

            // Indoor Ceiling (at y = 10.5m)
            CreateCube("Indoor Ceiling", new Vector3(0f, 10.5f, -4f), new Vector3(32f, 0.6f, 48f), ceiling, environment.transform);

            // Indoor Back Wall (at z = -28m, height 11m, width 32m)
            CreateCube("Indoor Back Wall", new Vector3(0f, 5f, -28f), new Vector3(32f, 11f, 1f), wall, environment.transform);

            // Indoor Left Wall (at x = -16m, from z = -28m to +20m)
            CreateCube("Indoor Left Wall", new Vector3(-16f, 5f, -4f), new Vector3(1f, 11f, 48f), wall, environment.transform);

            // Indoor Right Wall (at x = +16m, from z = -28m to +20m)
            CreateCube("Indoor Right Wall", new Vector3(16f, 5f, -4f), new Vector3(1f, 11f, 48f), wall, environment.transform);

            // Back Wall Detail Panels & Armory Window
            CreateCube("Armory Observation Window", new Vector3(0f, 4.5f, -27.4f), new Vector3(12f, 3.2f, 0.2f), dark, environment.transform);
            CreateCube("Window Glass", new Vector3(0f, 4.5f, -27.35f), new Vector3(11.6f, 2.8f, 0.05f), ceilingLampGlow, environment.transform);
            CreateCube("Security Door Frame", new Vector3(0f, 1.25f, -27.4f), new Vector3(4.2f, 2.6f, 0.2f), dark, environment.transform);
            CreateCube("Security Blast Door", new Vector3(0f, 1.25f, -27.35f), new Vector3(3.6f, 2.4f, 0.1f), hazard, environment.transform);

            // Structural Pillars & Overhead Trusses along Left and Right Sides
            for (float z = -22f; z <= 14f; z += 9f)
            {
                // Left Pillars
                CreateCube($"Pillar L {z}", new Vector3(-15.2f, 5f, z), new Vector3(1.4f, 10.8f, 1.4f), pillar, environment.transform);
                CreateCube($"Pillar Base L {z}", new Vector3(-15.2f, 0.6f, z), new Vector3(1.8f, 1.2f, 1.8f), dark, environment.transform);
                // Right Pillars
                CreateCube($"Pillar R {z}", new Vector3(15.2f, 5f, z), new Vector3(1.4f, 10.8f, 1.4f), pillar, environment.transform);
                CreateCube($"Pillar Base R {z}", new Vector3(15.2f, 0.6f, z), new Vector3(1.8f, 1.2f, 1.8f), dark, environment.transform);

                // Overhead Steel Cross-Trusses
                CreateCube($"Ceiling Truss {z}", new Vector3(0f, 9.8f, z), new Vector3(30f, 0.45f, 0.55f), dark, environment.transform);

                // Hanging Industrial Fluorescent Fixtures
                CreateCube($"Lamp Housing L {z}", new Vector3(-6f, 9.3f, z), new Vector3(0.35f, 0.2f, 3.2f), dark, environment.transform);
                CreateCube($"Lamp Tube L {z}", new Vector3(-6f, 9.15f, z), new Vector3(0.18f, 0.08f, 2.8f), ceilingLampGlow, environment.transform);
                CreateCube($"Lamp Housing R {z}", new Vector3(6f, 9.3f, z), new Vector3(0.35f, 0.2f, 3.2f), dark, environment.transform);
                CreateCube($"Lamp Tube R {z}", new Vector3(6f, 9.15f, z), new Vector3(0.18f, 0.08f, 2.8f), ceilingLampGlow, environment.transform);
            }

            // Floor Glowing Rails (Indoor firing lane markings)
            for (float z = -24f; z <= 18f; z += 4f)
            {
                CreateCube($"Floor Rail L {z}", new Vector3(-6f, 0.02f, z), new Vector3(0.08f, 0.04f, 2.2f), cyanGlow, environment.transform);
                CreateCube($"Floor Rail R {z}", new Vector3(6f, 0.02f, z), new Vector3(0.08f, 0.04f, 2.2f), orangeGlow, environment.transform);
            }

            // Indoor Central Platform (elevated shooting deck)
            CreateCube("Central Platform", new Vector3(0f, 0.35f, 16f), new Vector3(9f, 0.7f, 3.5f), dark, environment.transform);

            // =========================================================================
            // 2. HANGAR BLAST GATE / TRANSITION ARCHWAY (z = +20m)
            // =========================================================================
            // Gate Left Pillar
            CreateCube("Gate Left Wall", new Vector3(-12.5f, 5f, 20f), new Vector3(7f, 11f, 1.2f), wall, environment.transform);
            CreateCube("Gate Left Frame", new Vector3(-8.8f, 4.2f, 20f), new Vector3(0.8f, 8.5f, 1.5f), dark, environment.transform);
            CreateCube("Gate Left Hazard", new Vector3(-8.8f, 4.2f, 19.35f), new Vector3(0.6f, 8.2f, 0.05f), hazard, environment.transform);

            // Gate Right Pillar
            CreateCube("Gate Right Wall", new Vector3(12.5f, 5f, 20f), new Vector3(7f, 11f, 1.2f), wall, environment.transform);
            CreateCube("Gate Right Frame", new Vector3(8.8f, 4.2f, 20f), new Vector3(0.8f, 8.5f, 1.5f), dark, environment.transform);
            CreateCube("Gate Right Hazard", new Vector3(8.8f, 4.2f, 19.35f), new Vector3(0.6f, 8.2f, 0.05f), hazard, environment.transform);

            // Gate Upper Header Beam / Lintel
            CreateCube("Gate Upper Beam", new Vector3(0f, 9.2f, 20f), new Vector3(18f, 2.6f, 1.4f), wall, environment.transform);
            CreateCube("Gate Header Hazard Strip", new Vector3(0f, 8.1f, 19.35f), new Vector3(17.4f, 0.45f, 0.05f), hazard, environment.transform);
            CreateCube("Warning Strobe Left", new Vector3(-8f, 8.5f, 19.2f), new Vector3(0.4f, 0.4f, 0.2f), warningAmberGlow, environment.transform);
            CreateCube("Warning Strobe Right", new Vector3(8f, 8.5f, 19.2f), new Vector3(0.4f, 0.4f, 0.2f), warningAmberGlow, environment.transform);

            // Transition Threshold on floor
            CreateCube("Gate Floor Threshold", new Vector3(0f, 0.02f, 20f), new Vector3(17.6f, 0.05f, 1.2f), hazard, environment.transform);

            // Arena Title (mounted above the hangar blast gate at z = 19.2f)
            CreateTitleText(environment.transform, new Vector3(0f, 7.3f, 19.2f));

            // =========================================================================
            // 3. OUTDOOR ZONE: TACTICAL PROVING GROUNDS (z = +20m to +95m, NO CEILING!)
            // =========================================================================
            // Outdoor Ground (Length 75m, Width 52m, centered at z = 57.5m)
            CreateCube("Outdoor Ground", new Vector3(0f, -0.5f, 57.5f), new Vector3(52f, 1f, 75f), outdoorFloor, environment.transform);

            // Left Outdoor Perimeter Security Wall (from z = 20m to 95m, at x = -26m)
            CreateCube("Outdoor Left Wall", new Vector3(-26f, 4f, 57.5f), new Vector3(1.2f, 9f, 75f), wall, environment.transform);
            CreateCube("Outdoor Left Cap", new Vector3(-26f, 8.6f, 57.5f), new Vector3(1.6f, 0.4f, 75f), dark, environment.transform);

            // Right Outdoor Perimeter Security Wall (from z = 20m to 95m, at x = +26m)
            CreateCube("Outdoor Right Wall", new Vector3(26f, 4f, 57.5f), new Vector3(1.2f, 9f, 75f), wall, environment.transform);
            CreateCube("Outdoor Right Cap", new Vector3(26f, 8.6f, 57.5f), new Vector3(1.6f, 0.4f, 75f), dark, environment.transform);

            // Outdoor Far Backstop Blast Wall (at z = +95m)
            CreateCube("Outdoor Backstop Wall", new Vector3(0f, 5.5f, 95f), new Vector3(53f, 12f, 2f), wall, environment.transform);
            CreateCube("Backstop Berm Lower", new Vector3(0f, 2f, 93.8f), new Vector3(52f, 4f, 1.5f), dark, environment.transform);

            // Connecting Wing Walls from Hangar to Perimeter at z = 20m
            CreateCube("Wing Wall Left", new Vector3(-21f, 5f, 20f), new Vector3(10f, 11f, 1.2f), wall, environment.transform);
            CreateCube("Wing Wall Right", new Vector3(21f, 5f, 20f), new Vector3(10f, 11f, 1.2f), wall, environment.transform);

            // Outdoor Distance Markers painted on ground
            CreateCube("Dist Marker 25M", new Vector3(0f, 0.015f, 25f), new Vector3(12f, 0.03f, 0.35f), ceilingLampGlow, environment.transform);
            CreateCube("Dist Marker 50M", new Vector3(0f, 0.015f, 50f), new Vector3(16f, 0.03f, 0.35f), ceilingLampGlow, environment.transform);
            CreateCube("Dist Marker 75M", new Vector3(0f, 0.015f, 75f), new Vector3(20f, 0.03f, 0.35f), ceilingLampGlow, environment.transform);

            // Outdoor Tactical Shipping Containers
            CreateCube("Container Green Left", new Vector3(-10f, 1.4f, 40f), new Vector3(2.6f, 2.8f, 6.2f), containerGreen, environment.transform);
            CreateCube("Container Green Stack", new Vector3(-10f, 4.2f, 40f), new Vector3(2.6f, 2.8f, 6.2f), dark, environment.transform);
            CreateCube("Container Orange Right", new Vector3(11f, 1.4f, 46f), new Vector3(2.6f, 2.8f, 6.2f), containerOrange, environment.transform);
            CreateCube("Container Blue FarLeft", new Vector3(-14f, 1.4f, 62f), new Vector3(2.6f, 2.8f, 6.2f), containerBlue, environment.transform);

            // Concrete Jersey Barriers
            CreateCube("Barrier L 28", new Vector3(-5f, 0.6f, 28f), new Vector3(4.2f, 1.2f, 0.6f), barrier, environment.transform);
            CreateCube("Barrier R 28", new Vector3(5f, 0.6f, 28f), new Vector3(4.2f, 1.2f, 0.6f), barrier, environment.transform);
            CreateCube("Barrier L 52", new Vector3(-8f, 0.6f, 52f), new Vector3(5.5f, 1.2f, 0.6f), barrier, environment.transform);
            CreateCube("Barrier R 52", new Vector3(8f, 0.6f, 52f), new Vector3(5.5f, 1.2f, 0.6f), barrier, environment.transform);

            // Corner Watchtowers
            BuildWatchtower("Watchtower Left", new Vector3(-23f, 0f, 90f), wall, dark, environment.transform);
            BuildWatchtower("Watchtower Right", new Vector3(23f, 0f, 90f), wall, dark, environment.transform);

            // =========================================================================
            // 4. ACOUSTIC ENVIRONMENT & REVERB ARCHITECTURE
            // =========================================================================
            // Indoor Reverb Zone (Hangar center: z = -4m)
            // Heavy cavernous acoustic reflections with 4.2s decay time
            GameObject indoorReverbObj = new GameObject("Indoor Reverb Zone (Hangar)");
            indoorReverbObj.transform.SetParent(environment.transform);
            indoorReverbObj.transform.position = new Vector3(0f, 4f, -4f);
            AudioReverbZone indoorReverb = indoorReverbObj.AddComponent<AudioReverbZone>();
            indoorReverb.reverbPreset = AudioReverbPreset.Hangar;
            indoorReverb.minDistance = 22f;
            indoorReverb.maxDistance = 26f;

            // Outdoor Reverb Zone (Outdoor field: z = +58m)
            // Dry open-air acoustics with minimal reflections
            GameObject outdoorReverbObj = new GameObject("Outdoor Reverb Zone (Open Air)");
            outdoorReverbObj.transform.SetParent(environment.transform);
            outdoorReverbObj.transform.position = new Vector3(0f, 4f, 58f);
            AudioReverbZone outdoorReverb = outdoorReverbObj.AddComponent<AudioReverbZone>();
            outdoorReverb.reverbPreset = AudioReverbPreset.Plain;
            outdoorReverb.minDistance = 28f;
            outdoorReverb.maxDistance = 48f;

            // --- Left Wall 3D Sound Emitter: Plasma Power Generator (Inside Hangar) ---
            GameObject leftGen = new GameObject("Plasma Power Generator");
            leftGen.transform.SetParent(environment.transform);
            leftGen.transform.position = new Vector3(-15.2f, 2.5f, 4f);
            CreateCube("Generator Base", new Vector3(-15.2f, 1.2f, 4f), new Vector3(1.1f, 2.4f, 2.2f), dark, environment.transform);
            CreateCylinder("Generator Core", new Vector3(-15.1f, 2.5f, 4f), new Vector3(0.75f, 0.55f, 0.75f), cyanGlow, environment.transform);
            CreateCube("Generator Hood", new Vector3(-15.2f, 3.4f, 4f), new Vector3(1.0f, 0.35f, 1.8f), dark, environment.transform);

            AudioSource leftAudio = leftGen.AddComponent<AudioSource>();
            leftAudio.clip = SpatialAudioUtility.GetPlasmaReactorClip();
            leftAudio.loop = true;
            leftAudio.playOnAwake = true;
            leftAudio.spatialBlend = 1.0f;
            leftAudio.rolloffMode = AudioRolloffMode.Logarithmic;
            leftAudio.minDistance = 2.5f;
            leftAudio.maxDistance = 22.0f;
            leftAudio.volume = 0.35f;

            // --- Right Wall 3D Sound Emitter: Quantum Data Relay (Inside Hangar) ---
            GameObject rightRelay = new GameObject("Quantum Data Relay");
            rightRelay.transform.SetParent(environment.transform);
            rightRelay.transform.position = new Vector3(15.2f, 2.5f, 8f);
            CreateCube("Relay Base", new Vector3(15.2f, 1.2f, 8f), new Vector3(1.1f, 2.4f, 2.2f), dark, environment.transform);
            CreateCube("Relay Screen", new Vector3(15.1f, 2.5f, 8f), new Vector3(0.12f, 0.85f, 1.4f), orangeGlow, environment.transform);
            CreateCube("Relay Hood", new Vector3(15.2f, 3.4f, 8f), new Vector3(1.0f, 0.35f, 1.8f), dark, environment.transform);

            AudioSource rightAudio = rightRelay.AddComponent<AudioSource>();
            rightAudio.clip = SpatialAudioUtility.GetQuantumRelayClip();
            rightAudio.loop = true;
            rightAudio.playOnAwake = true;
            rightAudio.spatialBlend = 1.0f;
            rightAudio.rolloffMode = AudioRolloffMode.Logarithmic;
            rightAudio.minDistance = 2.5f;
            rightAudio.maxDistance = 22.0f;
            rightAudio.volume = 0.30f;

            // --- Outdoor Open-Air Ambient Audio Emitter ---
            GameObject outdoorAtmosphere = new GameObject("Outdoor Wind Atmosphere");
            outdoorAtmosphere.transform.SetParent(environment.transform);
            outdoorAtmosphere.transform.position = new Vector3(0f, 6f, 55f);
            AudioSource outdoorAudio = outdoorAtmosphere.AddComponent<AudioSource>();
            outdoorAudio.clip = SpatialAudioUtility.GetOutdoorBreezeClip();
            outdoorAudio.loop = true;
            outdoorAudio.playOnAwake = true;
            outdoorAudio.spatialBlend = 1.0f;
            outdoorAudio.rolloffMode = AudioRolloffMode.Logarithmic;
            outdoorAudio.minDistance = 15f;
            outdoorAudio.maxDistance = 65f;
            outdoorAudio.volume = 0.35f;
        }

        private static void BuildWatchtower(string name, Vector3 position, Material wallMat, Material darkMat, Transform parent)
        {
            GameObject tower = new GameObject(name);
            tower.transform.SetParent(parent);
            tower.transform.position = position;

            float legOffset = 1.8f;
            CreateCube("Leg 1", new Vector3(-legOffset, 4.5f, -legOffset), new Vector3(0.4f, 9f, 0.4f), darkMat, tower.transform, true);
            CreateCube("Leg 2", new Vector3(legOffset, 4.5f, -legOffset), new Vector3(0.4f, 9f, 0.4f), darkMat, tower.transform, true);
            CreateCube("Leg 3", new Vector3(-legOffset, 4.5f, legOffset), new Vector3(0.4f, 9f, 0.4f), darkMat, tower.transform, true);
            CreateCube("Leg 4", new Vector3(legOffset, 4.5f, legOffset), new Vector3(0.4f, 9f, 0.4f), darkMat, tower.transform, true);

            CreateCube("Platform", new Vector3(0f, 9.1f, 0f), new Vector3(4.5f, 0.3f, 4.5f), darkMat, tower.transform, true);
            CreateCube("Cabin Roof", new Vector3(0f, 12f, 0f), new Vector3(4.8f, 0.25f, 4.8f), wallMat, tower.transform, true);
            CreateCube("Railing", new Vector3(0f, 9.7f, 0f), new Vector3(4.4f, 0.9f, 4.4f), darkMat, tower.transform, true);
        }

        private static void BuildLighting()
        {
            GameObject lights = new GameObject("LIGHTING");

            // Outdoor Golden Sunlight
            GameObject sunObject = new GameObject("Directional Sunlight");
            sunObject.transform.SetParent(lights.transform);
            sunObject.transform.rotation = Quaternion.Euler(46f, -32f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.94f, 0.86f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;

            // Indoor Hangar Ambient Fill Lights
            CreatePointLight("Indoor Hangar Fill", new Vector3(0f, 8f, -4f), new Color(0.45f, 0.65f, 0.85f), 4.5f, 26f, lights.transform);
            CreatePointLight("Indoor Gate Amber Fill", new Vector3(0f, 7.5f, 19f), new Color(1f, 0.65f, 0.15f), 4.0f, 18f, lights.transform);

            // Side Emitter Glows
            CreatePointLight("Cyan Generator Glow", new Vector3(-13.5f, 3f, 4f), new Color(0.05f, 0.75f, 1f), 6f, 14f, lights.transform);
            CreatePointLight("Orange Relay Glow", new Vector3(13.5f, 3f, 8f), new Color(1f, 0.35f, 0.05f), 5f, 14f, lights.transform);

            // Outdoor Field Lights
            CreatePointLight("Outdoor Yard Fill Mid", new Vector3(0f, 7f, 45f), new Color(0.85f, 0.92f, 1f), 3.5f, 35f, lights.transform);
            CreatePointLight("Outdoor Yard Fill Far", new Vector3(0f, 8f, 75f), new Color(0.85f, 0.92f, 1f), 4.0f, 40f, lights.transform);
        }

        private static void BuildTargets(Material cyan, Material orange, Material dark, AudioClip hit, AudioClip explosion)
        {
            GameObject targets = new GameObject("TARGETS");
            Vector3[] positions =
            {
                new Vector3(-6f, 1.6f, 6f),     // Target 1: Indoor Left (Cyan)
                new Vector3(0f, 1.6f, 11f),     // Target 2: Indoor Center Mover (Orange - verified in Test 4)
                new Vector3(6f, 1.6f, 8f),      // Target 3: Indoor Right (Cyan)
                new Vector3(0f, 2.2f, 16f),     // Target 4: Indoor Platform (Orange)
                new Vector3(0f, 2.0f, 32f),     // Target 5: Outdoor Mover (Cyan)
                new Vector3(-10f, 2.2f, 46f),   // Target 6: Outdoor Behind Left Container (Orange)
                new Vector3(12f, 2.2f, 55f),    // Target 7: Outdoor Right Bunker (Cyan)
                new Vector3(0f, 3.8f, 78f)      // Target 8: Outdoor Far Sniper Platform (Orange)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                Material accent = i % 2 == 0 ? cyan : orange;
                GameObject target = CreateTarget($"Target {i + 1}", positions[i], accent, dark, targets.transform);
                target.GetComponent<ShootableTarget>().Configure(hit, explosion);

                if (i == 1) // Target 2 (Indoor Mover)
                {
                    TargetMover mover = target.AddComponent<TargetMover>();
                    mover.Configure(new Vector3(2.4f, 0f, 0f), 1.15f);
                }
                else if (i == 4) // Target 5 (Outdoor Mover)
                {
                    TargetMover mover = target.AddComponent<TargetMover>();
                    mover.Configure(new Vector3(-4.0f, 0f, 0f), 0.95f);
                }
            }
        }

        private static GameObject CreateTarget(string name, Vector3 position, Material accent, Material dark, Transform parent)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent);
            root.transform.position = position;

            CreateCube("Base", new Vector3(0f, -1.15f, 0f), new Vector3(1.6f, 0.25f, 0.9f), dark, root.transform, true);
            CreateCube("Stem", new Vector3(0f, -0.55f, 0f), new Vector3(0.18f, 1.2f, 0.18f), dark, root.transform, true);

            CreateCylinder("Target Plate", Vector3.zero, new Vector3(1.15f, 0.12f, 1.15f), accent, root.transform);
            CreateCylinder("Bullseye", new Vector3(0f, 0f, -0.14f), new Vector3(0.42f, 0.08f, 0.42f), dark, root.transform);

            root.AddComponent<ShootableTarget>();
            return root;
        }

        private static void BuildPlayer(
            AudioClip gunshot,
            AudioClip impact,
            AudioClip footstep,
            AudioClip reload,
            AudioClip land,
            AudioClip grenadeExplosion,
            AudioClip smokeGrenade,
            AudioClip laser,
            Material gunSteel,
            Material gunPanel,
            Material gunCeramic,
            Material cyanGlow,
            Material orangeGlow,
            Material hotRedGlow,
            Material hotGoldGlow,
            Material opticGlass,
            Material darkMaterial)
        {
            GameObject player = new GameObject("PLAYER");
            player.transform.position = new Vector3(0f, 1.1f, -7f);
            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = 1.8f;
            characterController.radius = 0.35f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            GameObject cameraObject = new GameObject("Player Camera");
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 68f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 180f;
            cameraObject.AddComponent<AudioListener>();
            CameraShake shake = cameraObject.AddComponent<CameraShake>();

            // --- Modern Lowered Gun Transform (Sits low-ready on lower-right, handle hidden off-screen, barrel aligned straight forward) ---
            GameObject gun = new GameObject("Demo Blaster");
            gun.transform.SetParent(cameraObject.transform, false);
            gun.transform.localPosition = new Vector3(0.24f, -0.42f, 0.92f);
            gun.transform.localRotation = Quaternion.Euler(-0.95f, -0.57f, 0f);
            gun.transform.localScale = Vector3.one * 0.72f;

            // --- Futuristic Modular Chassis & Receiver ---
            CreateGunCube("Upper Ceramic Shell", new Vector3(0f, 0.05f, -0.06f), new Vector3(0.32f, 0.14f, 0.54f), Quaternion.identity, gunCeramic, gun.transform);
            CreateGunCube("Lower Receiver", new Vector3(0f, -0.065f, 0.02f), new Vector3(0.28f, 0.15f, 0.50f), Quaternion.identity, gunSteel, gun.transform);
            CreateGunCube("Receiver Bevel Top", new Vector3(0f, 0.13f, -0.04f), new Vector3(0.22f, 0.06f, 0.48f), Quaternion.identity, gunPanel, gun.transform);
            CreateGunCube("Rear Power Housing", new Vector3(0f, 0.025f, -0.36f), new Vector3(0.26f, 0.14f, 0.16f), Quaternion.identity, gunSteel, gun.transform);
            CreateGunCube("Rear Charge Strip", new Vector3(0f, 0.025f, -0.445f), new Vector3(0.14f, 0.035f, 0.012f), Quaternion.identity, orangeGlow, gun.transform);
            CreateGunCube("Left Swept Armor", new Vector3(-0.185f, 0.01f, -0.02f), new Vector3(0.065f, 0.18f, 0.40f), Quaternion.Euler(0f, -5f, 12f), gunSteel, gun.transform);
            CreateGunCube("Right Swept Armor", new Vector3(0.185f, 0.01f, -0.02f), new Vector3(0.065f, 0.18f, 0.40f), Quaternion.Euler(0f, 5f, -12f), gunSteel, gun.transform);

            // --- Modern Reflex Optic Sight ---
            CreateGunCube("Optic Rail Base", new Vector3(0f, 0.155f, -0.06f), new Vector3(0.09f, 0.025f, 0.32f), Quaternion.identity, gunSteel, gun.transform);
            CreateGunCube("Holo Sight Hood Left", new Vector3(-0.048f, 0.205f, -0.06f), new Vector3(0.012f, 0.085f, 0.14f), Quaternion.identity, gunPanel, gun.transform);
            CreateGunCube("Holo Sight Hood Right", new Vector3(0.048f, 0.205f, -0.06f), new Vector3(0.012f, 0.085f, 0.14f), Quaternion.identity, gunPanel, gun.transform);
            CreateGunCube("Holo Sight Hood Top", new Vector3(0f, 0.245f, -0.06f), new Vector3(0.108f, 0.012f, 0.14f), Quaternion.identity, gunPanel, gun.transform);
            CreateGunCube("Holo Lens Window", new Vector3(0f, 0.205f, -0.06f), new Vector3(0.082f, 0.07f, 0.008f), Quaternion.identity, opticGlass, gun.transform);
            CreateGunCube("Front Holo Sight", new Vector3(0f, 0.205f, -0.055f), new Vector3(0.016f, 0.016f, 0.004f), Quaternion.identity, hotRedGlow, gun.transform);
            CreateGunCube("Rear Sight", new Vector3(0f, 0.185f, -0.22f), new Vector3(0.08f, 0.045f, 0.035f), Quaternion.identity, gunSteel, gun.transform);

            // --- Laser Energy Progress Monitor (Tactical OLED & 5-Segment HUD on Upper-Left Corner) ---
            GameObject monitorMount = new GameObject("Laser Monitor Mount");
            monitorMount.transform.SetParent(gun.transform, false);
            monitorMount.transform.localPosition = new Vector3(-0.165f, 0.155f, -0.04f);
            monitorMount.transform.localRotation = Quaternion.Euler(18f, -30f, 2f);

            CreateGunCube("Laser Mount Bracket", new Vector3(0.05f, -0.035f, 0.025f), new Vector3(0.065f, 0.03f, 0.05f), Quaternion.identity, gunSteel, monitorMount.transform);
            CreateGunCube("Laser Monitor Casing", Vector3.zero, new Vector3(0.21f, 0.135f, 0.025f), Quaternion.identity, gunPanel, monitorMount.transform);
            CreateGunCube("Laser Monitor Screen", new Vector3(0f, 0f, -0.015f), new Vector3(0.19f, 0.118f, 0.004f), Quaternion.identity, darkMaterial, monitorMount.transform);

            GameObject readoutObj = new GameObject("Laser Monitor Readout");
            readoutObj.transform.SetParent(monitorMount.transform, false);
            readoutObj.transform.localPosition = new Vector3(0f, 0.036f, -0.020f);
            TextMesh readoutText = readoutObj.AddComponent<TextMesh>();
            readoutText.text = "LASER  0%";
            readoutText.anchor = TextAnchor.MiddleCenter;
            readoutText.alignment = TextAlignment.Center;
            readoutText.fontSize = 58;
            readoutText.characterSize = 0.0020f;
            readoutText.fontStyle = FontStyle.Bold;
            readoutText.color = new Color(1f, 0.45f, 0.08f);

            GameObject segmentsRoot = new GameObject("Laser Segments Root");
            segmentsRoot.transform.SetParent(monitorMount.transform, false);
            segmentsRoot.transform.localPosition = new Vector3(0f, -0.006f, -0.020f);
            for (int i = 0; i < 5; i++)
            {
                CreateGunCube($"Laser Segment {i + 1}", new Vector3(-0.064f + i * 0.032f, 0f, 0f), new Vector3(0.024f, 0.028f, 0.006f), Quaternion.identity, darkMaterial, segmentsRoot.transform);
            }

            CreateGunCube("Laser Gauge Trough", new Vector3(0f, -0.040f, -0.018f), new Vector3(0.165f, 0.015f, 0.005f), Quaternion.identity, darkMaterial, monitorMount.transform);
            CreateGunCube("Laser Gauge Fill", new Vector3(-0.078f, -0.040f, -0.022f), new Vector3(0.001f, 0.010f, 0.005f), Quaternion.identity, orangeGlow, monitorMount.transform);

            // --- Top Energy Rail Conduit ---
            GameObject topRail = new GameObject("Top Energy Rail");
            topRail.transform.SetParent(gun.transform, false);
            topRail.transform.localPosition = new Vector3(0f, 0.185f, 0.06f);
            CreateGunCube("Top Rail Trough", Vector3.zero, new Vector3(0.075f, 0.02f, 0.36f), Quaternion.identity, gunPanel, topRail.transform);
            CreateGunCube("Top Rail Fill", new Vector3(0f, 0.011f, -0.135f), new Vector3(0.05f, 0.012f, 0.001f), Quaternion.identity, orangeGlow, topRail.transform);

            // --- Core & Plasma Energy Conduit ---
            CreateGunSphere("Ion Core", new Vector3(0f, 0.045f, 0.22f), new Vector3(0.20f, 0.20f, 0.26f), hotRedGlow, gun.transform);
            CreateGunCylinder("Core Collar Rear", new Vector3(0f, 0.045f, 0.08f), new Vector3(0.18f, 0.03f, 0.18f), gunPanel, gun.transform);
            CreateGunCylinder("Core Collar Front", new Vector3(0f, 0.045f, 0.35f), new Vector3(0.18f, 0.03f, 0.18f), gunCeramic, gun.transform);
            CreateGunCylinder("Plasma Barrel", new Vector3(0f, 0.045f, 0.56f), new Vector3(0.085f, 0.24f, 0.085f), orangeGlow, gun.transform);
            CreateGunCylinder("Barrel Sleeve", new Vector3(0f, 0.045f, 0.58f), new Vector3(0.125f, 0.20f, 0.125f), gunPanel, gun.transform);

            CreateGunCylinder("Energy Coil 1", new Vector3(0f, 0.045f, 0.46f), new Vector3(0.155f, 0.018f, 0.155f), hotRedGlow, gun.transform);
            CreateGunCylinder("Energy Coil 2", new Vector3(0f, 0.045f, 0.58f), new Vector3(0.155f, 0.018f, 0.155f), orangeGlow, gun.transform);
            CreateGunCylinder("Energy Coil 3", new Vector3(0f, 0.045f, 0.70f), new Vector3(0.155f, 0.018f, 0.155f), hotGoldGlow, gun.transform);

            // --- Angular Compensator / Muzzle Assembly ---
            CreateGunCylinder("Muzzle Hub", new Vector3(0f, 0.045f, 0.80f), new Vector3(0.165f, 0.10f, 0.165f), gunSteel, gun.transform);
            CreateGunCylinder("Muzzle Halo", new Vector3(0f, 0.045f, 0.89f), new Vector3(0.185f, 0.024f, 0.185f), orangeGlow, gun.transform);
            CreateGunCube("Muzzle Prong Top", new Vector3(0f, 0.165f, 0.89f), new Vector3(0.065f, 0.065f, 0.26f), Quaternion.Euler(-6f, 0f, 0f), gunCeramic, gun.transform);
            CreateGunCube("Muzzle Prong Bottom", new Vector3(0f, -0.075f, 0.89f), new Vector3(0.065f, 0.065f, 0.26f), Quaternion.Euler(6f, 0f, 0f), gunPanel, gun.transform);
            CreateGunCube("Muzzle Prong Left", new Vector3(-0.13f, 0.045f, 0.89f), new Vector3(0.065f, 0.065f, 0.26f), Quaternion.Euler(0f, 6f, 0f), gunSteel, gun.transform);
            CreateGunCube("Muzzle Prong Right", new Vector3(0.13f, 0.045f, 0.89f), new Vector3(0.065f, 0.065f, 0.26f), Quaternion.Euler(0f, -6f, 0f), gunSteel, gun.transform);

            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    CreateGunCube($"Vent {(side < 0 ? "L" : "R")} {i + 1}",
                        new Vector3(side * 0.222f, 0.035f, -0.18f + i * 0.11f),
                        new Vector3(0.022f, 0.06f, 0.065f), Quaternion.Euler(0f, 0f, side * 8f), orangeGlow, gun.transform);
                }
            }

            CreateGunCube("Grip Frame", new Vector3(0f, -0.20f, -0.16f), new Vector3(0.18f, 0.14f, 0.18f), Quaternion.Euler(-14f, 0f, 0f), gunPanel, gun.transform);
            CreateGunCube("Grip Backstrap", new Vector3(0f, -0.22f, -0.23f), new Vector3(0.13f, 0.12f, 0.05f), Quaternion.Euler(-14f, 0f, 0f), gunCeramic, gun.transform);
            CreateGunCube("Trigger", new Vector3(0f, -0.12f, -0.01f), new Vector3(0.035f, 0.10f, 0.035f), Quaternion.Euler(-20f, 0f, 0f), orangeGlow, gun.transform);

            GameObject coreLightObject = new GameObject("Ion Core Light");
            coreLightObject.transform.SetParent(gun.transform, false);
            coreLightObject.transform.localPosition = new Vector3(0f, 0.045f, 0.22f);
            Light coreLight = coreLightObject.AddComponent<Light>();
            coreLight.type = LightType.Point;
            coreLight.color = new Color(1f, 0.36f, 0.05f);
            coreLight.intensity = 1.9f;
            coreLight.range = 2.4f;
            coreLight.shadows = LightShadows.None;

            GameObject magazine = new GameObject("Magazine");
            magazine.transform.SetParent(gun.transform, false);
            magazine.transform.localPosition = new Vector3(0f, -0.18f, 0.07f);
            CreateGunCube("Magazine Neck", new Vector3(0f, 0.06f, 0f), new Vector3(0.16f, 0.10f, 0.17f), Quaternion.Euler(6f, 0f, 0f), gunPanel, magazine.transform);
            CreateGunCube("Magazine Body", new Vector3(0f, -0.04f, -0.015f), new Vector3(0.21f, 0.14f, 0.21f), Quaternion.Euler(10f, 0f, 0f), gunSteel, magazine.transform);
            CreateGunCube("Magazine Energy Window", new Vector3(0f, -0.04f, -0.125f), new Vector3(0.12f, 0.10f, 0.025f), Quaternion.Euler(10f, 0f, 0f), orangeGlow, magazine.transform);
            CreateGunCube("Magazine Base", new Vector3(0f, -0.12f, -0.035f), new Vector3(0.23f, 0.04f, 0.23f), Quaternion.Euler(10f, 0f, 0f), gunCeramic, magazine.transform);

            GameObject muzzleObject = new GameObject("Muzzle");
            muzzleObject.transform.SetParent(gun.transform, false);
            muzzleObject.transform.localPosition = new Vector3(0f, 0.045f, 1.05f);

            AudioSource weaponAudio = player.AddComponent<AudioSource>();
            Configure3dAudio(weaponAudio, 0.65f, 1f, 32f);

            AudioSource footstepAudio = player.AddComponent<AudioSource>();
            footstepAudio.clip = footstep;
            Configure3dAudio(footstepAudio, 0.45f, 1f, 12f);

            FirstPersonController movement = player.AddComponent<FirstPersonController>();
            movement.Configure(camera, footstepAudio, land);
            WeaponController weapon = player.AddComponent<WeaponController>();
            weapon.Configure(camera, gun.transform, muzzleObject.transform, magazine.transform, weaponAudio, gunshot, impact, reload, shake, laser);
            GrenadeThrower grenadeThrower = player.AddComponent<GrenadeThrower>();
            grenadeThrower.Configure(camera, shake, grenadeExplosion, smokeGrenade);
        }

        private static void BuildGameSystems(AudioClip ambience)
        {
            GameObject systems = new GameObject("GAME SYSTEMS");
            systems.AddComponent<DemoGameManager>();
            systems.AddComponent<DemoHud>();
            systems.AddComponent<SpatialAudioProbe>();

            AudioSource ambientSource = systems.AddComponent<AudioSource>();
            ambientSource.clip = ambience;
            ambientSource.loop = true;
            ambientSource.playOnAwake = true;
            ambientSource.volume = 0.32f;
            ambientSource.spatialBlend = 0f;
        }

        private static void CreateTitleText(Transform parent, Vector3 position = default)
        {
            if (position == default)
            {
                position = new Vector3(0f, 7.3f, 19.2f);
            }
            GameObject title = new GameObject("Arena Title");
            title.transform.SetParent(parent);
            title.transform.position = position;
            title.transform.rotation = Quaternion.identity;
            TextMesh text = title.AddComponent<TextMesh>();
            text.text = "SOUND + VFX LAB";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.45f;
            text.fontSize = 64;
            text.color = new Color(0.2f, 0.9f, 1f);
        }

        private static GameObject CreateCube(
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            Transform parent,
            bool local = false)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            if (local)
            {
                cube.transform.localPosition = position;
            }
            else
            {
                cube.transform.position = position;
            }
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static void CreatePointLight(
            string name,
            Vector3 position,
            Color color,
            float intensity,
            float range,
            Transform parent)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent);
            lightObject.transform.position = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.Soft;
        }

        private static GameObject CreateCylinder(
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Transform parent)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cylinder.transform.localScale = localScale;
            cylinder.GetComponent<Renderer>().sharedMaterial = material;
            return cylinder;
        }

        private static GameObject CreateGunCube(
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material,
            Transform parent)
        {
            GameObject part = CreateCube(name, localPosition, localScale, material, parent, true);
            part.transform.localRotation = localRotation;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        private static GameObject CreateGunCylinder(
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Transform parent)
        {
            GameObject part = CreateCylinder(name, localPosition, localScale, material, parent);
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        private static GameObject CreateGunSphere(
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Transform parent)
        {
            return CreateGunPrimitive(PrimitiveType.Sphere, name, localPosition, localScale, Quaternion.identity, material, parent);
        }

        private static GameObject CreateGunPrimitive(
            PrimitiveType primitiveType,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material,
            Transform parent)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
            return part;
        }

        private static void Configure3dAudio(AudioSource source, float spatialBlend, float minDistance, float maxDistance)
        {
            source.playOnAwake = false;
            source.spatialBlend = spatialBlend;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness)
        {
            string path = $"Assets/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateEmissiveMaterial(string name, Color color, float intensity)
        {
            Material material = CreateMaterial(name, color, 0.3f, 0.8f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static AudioClip LoadAudio(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"Audio asset not found at {path}. Reimporting assets.");
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            return clip;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(string.IsNullOrEmpty(parent) ? "Assets" : parent, name);
        }
    }
}
