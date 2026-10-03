using System;
using System.Collections;
using UnityEngine;

namespace MiaShooter
{
    [DisallowMultipleComponent]
    public sealed class GrenadeThrowAnimation : MonoBehaviour
    {
        [Header("Animation Timings")]
        [SerializeField] private float equipDuration = 0.28f;
        [SerializeField] private float windupDuration = 0.18f;
        [SerializeField] private float throwDuration = 0.22f;
        [SerializeField] private float followThroughDuration = 0.16f;
        [SerializeField] private float recoverDuration = 0.22f;
        [SerializeField, Range(0.1f, 0.9f)] private float releasePoint = 0.54f;

        // --- Camera-Relative Natural Poses for Wrist/Hand ---
        public static readonly Pose RestPose = new Pose(
            new Vector3(-0.28f, -0.52f, 0.36f), Quaternion.Euler(40f, 18f, -14f));

        public static readonly Pose HoldPose = new Pose(
            new Vector3(-0.18f, -0.19f, 0.50f), Quaternion.Euler(14f, 28f, -16f));

        public static readonly Pose WindupPose = new Pose(
            new Vector3(-0.24f, -0.09f, 0.40f), Quaternion.Euler(28f, 16f, -24f));

        public static readonly Pose PeakThrowPose = new Pose(
            new Vector3(-0.11f, -0.02f, 0.58f), Quaternion.Euler(-10f, 12f, 16f));

        public static readonly Pose ReleasePose = new Pose(
            new Vector3(-0.06f, -0.06f, 0.68f), Quaternion.Euler(-20f, 6f, 22f));

        public static readonly Pose FollowThroughPose = new Pose(
            new Vector3(-0.06f, -0.22f, 0.60f), Quaternion.Euler(-8f, 12f, 14f));

        private const float L1 = 0.38f; // Upper arm length
        private const float L2 = 0.38f; // Forearm length
        private static readonly Vector3 ShoulderLocalPos = new Vector3(-0.32f, -0.38f, 0.10f);

        private GameObject armSystemRoot;
        private Transform upperArmBone;
        private Transform elbowBone;
        private Transform forearmBone;
        private Transform wristBone;
        private Transform handRoot;
        private Transform grenadeMount;
        private GameObject heldGrenade;
        private GrenadeType equippedType;
        private CameraShake cameraShake;

        private FingerJoint[] fingers;

        public bool IsPlaying { get; private set; }
        public bool IsEquipped { get; private set; }
        public bool IsReadyToThrow => IsEquipped && !IsPlaying;
        public GrenadeType EquippedType => equippedType;
        public Transform HandTransform => handRoot != null ? handRoot : null;

        private sealed class FingerJoint
        {
            public Transform rootPivot;
            public Transform distalPivot;
            public Quaternion gripRootRot;
            public Quaternion gripDistalRot;
            public Quaternion openRootRot;
            public Quaternion openDistalRot;
            public Quaternion relaxRootRot;
            public Quaternion relaxDistalRot;

            public void Apply(float openProgress, float relaxWeight)
            {
                Quaternion activeRoot = Quaternion.Slerp(gripRootRot, openRootRot, openProgress);
                Quaternion activeDistal = Quaternion.Slerp(gripDistalRot, openDistalRot, openProgress);

                Quaternion finalRoot = Quaternion.Slerp(activeRoot, relaxRootRot, relaxWeight);
                Quaternion finalDistal = Quaternion.Slerp(activeDistal, relaxDistalRot, relaxWeight);

                if (rootPivot != null)
                {
                    rootPivot.localRotation = finalRoot;
                }

                if (distalPivot != null)
                {
                    distalPivot.localRotation = finalDistal;
                }
            }
        }

        private void Awake()
        {
            cameraShake = GetComponent<CameraShake>() ?? GetComponentInParent<CameraShake>();
            EnsureBuilt();
        }

        public void EnsureBuilt()
        {
            if (armSystemRoot == null || handRoot == null)
            {
                BuildArmSystem();
            }
        }

        private void Start()
        {
            if (cameraShake == null)
            {
                cameraShake = GetComponent<CameraShake>() ?? GetComponentInParent<CameraShake>();
            }
        }

        private void Update()
        {
            // Organic idle breathing sway while holding grenade ready to throw
            if (IsReadyToThrow && armSystemRoot != null && armSystemRoot.activeSelf)
            {
                float t = Time.time;
                float swayY = Mathf.Sin(t * 2.0f) * 0.0035f;
                float swayX = Mathf.Cos(t * 1.0f) * 0.0022f;
                float swayRotZ = Mathf.Sin(t * 2.0f) * 0.75f;
                float swayRotX = Mathf.Cos(t * 1.6f) * 0.5f;

                Vector3 currentPos = HoldPose.position + new Vector3(swayX, swayY, 0f);
                Quaternion currentRot = HoldPose.rotation * Quaternion.Euler(swayRotX, 0f, swayRotZ);

                SetPose(new Pose(currentPos, currentRot));
            }
        }

        public bool Equip(GrenadeType grenadeType)
        {
            EnsureBuilt();
            if (IsPlaying)
            {
                return false;
            }

            SetHeldGrenade(grenadeType);
            if (IsEquipped)
            {
                return true;
            }

            StartCoroutine(EquipRoutine());
            return true;
        }

        public bool Unequip(Action completed)
        {
            if (!IsEquipped || IsPlaying)
            {
                return false;
            }

            StartCoroutine(UnequipRoutine(completed));
            return true;
        }

        public bool Throw(Action<Vector3> releaseGrenade, Action completed)
        {
            if (!IsReadyToThrow || releaseGrenade == null)
            {
                return false;
            }

            StartCoroutine(ThrowRoutine(releaseGrenade, completed));
            return true;
        }

        private IEnumerator EquipRoutine()
        {
            IsEquipped = true;
            IsPlaying = true;
            armSystemRoot.SetActive(true);
            SetPose(RestPose);
            SetFingers(0f, 0.4f);

            float elapsed = 0f;
            while (elapsed < equipDuration)
            {
                float p = Mathf.Clamp01(elapsed / equipDuration);
                // Elastic ease-out with subtle overshoot settle
                float easedP = EaseOutBack(p);
                SetPose(LerpPose(RestPose, HoldPose, easedP));

                // Fingers tighten securely into grip around grenade
                float relax = Mathf.Lerp(0.4f, 0f, p);
                SetFingers(0f, relax);

                elapsed += Time.deltaTime;
                yield return null;
            }

            SetPose(HoldPose);
            SetFingers(0f, 0f);
            IsPlaying = false;
        }

        private IEnumerator UnequipRoutine(Action completed)
        {
            IsPlaying = true;
            float elapsed = 0f;
            Pose currentPose = new Pose(handRoot.localPosition, handRoot.localRotation);

            while (elapsed < recoverDuration)
            {
                float p = Mathf.Clamp01(elapsed / recoverDuration);
                float smoothP = Smooth(p);
                SetPose(LerpPose(currentPose, RestPose, smoothP));
                SetFingers(0.1f, Mathf.Lerp(0f, 0.6f, p));

                elapsed += Time.deltaTime;
                yield return null;
            }

            FinishAndHide(completed);
        }

        private IEnumerator ThrowRoutine(Action<Vector3> releaseGrenade, Action completed)
        {
            IsPlaying = true;
            Pose startHoldPose = new Pose(handRoot.localPosition, handRoot.localRotation);

            // 1. Windup: Pull back, cock wrist, fingers tighten
            float elapsed = 0f;
            while (elapsed < windupDuration)
            {
                float p = Mathf.Clamp01(elapsed / windupDuration);
                float smoothP = Smooth(p);
                SetPose(LerpPose(startHoldPose, WindupPose, smoothP));
                SetFingers(0f, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetPose(WindupPose);

            // 2. The Heave & Forward Whip
            bool released = false;
            elapsed = 0f;
            while (elapsed < throwDuration)
            {
                float progress = Mathf.Clamp01(elapsed / throwDuration);
                Pose currentThrowPose;

                if (progress < releasePoint)
                {
                    float subP = progress / releasePoint;
                    float whipP = Mathf.Pow(subP, 1.85f);
                    currentThrowPose = LerpPose(WindupPose, PeakThrowPose, whipP);
                    SetFingers(0f, 0f);
                }
                else
                {
                    float subP = (progress - releasePoint) / (1f - releasePoint);
                    float snapP = 1f - Mathf.Pow(1f - subP, 2.2f);
                    currentThrowPose = LerpPose(PeakThrowPose, ReleasePose, snapP);

                    // Dynamic finger opening: spring open immediately on release!
                    float openP = Mathf.Clamp01(subP / 0.35f);
                    SetFingers(openP, 0f);
                }

                SetPose(currentThrowPose);

                if (!released && progress >= releasePoint)
                {
                    released = true;
                    ReleaseHeldGrenade(releaseGrenade);
                    cameraShake?.Play(0.12f, 0.075f);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            SetPose(ReleasePose);
            if (!released)
            {
                ReleaseHeldGrenade(releaseGrenade);
                cameraShake?.Play(0.12f, 0.075f);
            }
            SetFingers(1f, 0f);

            // 3. Follow-Through: Momentum dissipation & finger relaxation
            elapsed = 0f;
            while (elapsed < followThroughDuration)
            {
                float p = Mathf.Clamp01(elapsed / followThroughDuration);
                float smoothP = Smooth(p);
                SetPose(LerpPose(ReleasePose, FollowThroughPose, smoothP));

                // Hand relaxes from full flick open to comfortable natural curve
                float fingerOpen = Mathf.Lerp(1f, 0.35f, p);
                float relax = Mathf.Lerp(0f, 0.55f, p);
                SetFingers(fingerOpen, relax);

                elapsed += Time.deltaTime;
                yield return null;
            }
            SetPose(FollowThroughPose);

            // 4. Recovery: Lower arm smoothly out of view
            elapsed = 0f;
            while (elapsed < recoverDuration)
            {
                float p = Mathf.Clamp01(elapsed / recoverDuration);
                float smoothP = Smooth(p);
                SetPose(LerpPose(FollowThroughPose, RestPose, smoothP));
                SetFingers(0.15f, 0.8f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            FinishAndHide(completed);
        }

        private void FinishAndHide(Action completed)
        {
            armSystemRoot.SetActive(false);
            IsEquipped = false;
            IsPlaying = false;
            completed?.Invoke();
        }

        private void ReleaseHeldGrenade(Action<Vector3> releaseGrenade)
        {
            if (heldGrenade != null)
            {
                releaseGrenade(heldGrenade.transform.position);
                heldGrenade.SetActive(false);
            }
        }

        private void SetHeldGrenade(GrenadeType grenadeType)
        {
            EnsureBuilt();
            equippedType = grenadeType;
            if (heldGrenade != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(heldGrenade);
                }
                else
                {
                    DestroyImmediate(heldGrenade);
                }
            }

            heldGrenade = GrenadeVisualFactory.Create(
                grenadeType == GrenadeType.Smoke ? "Held Smoke Grenade" : "Held Fragmentation Grenade",
                grenadeType);
            GrenadeVisualFactory.DisableCollider(heldGrenade);
            heldGrenade.transform.SetParent(grenadeMount != null ? grenadeMount : handRoot, false);
            heldGrenade.transform.localPosition = Vector3.zero;
            heldGrenade.transform.localRotation = Quaternion.identity;
        }

        public void SetHeldGrenadeForPreview(GrenadeType grenadeType)
        {
            EnsureBuilt();
            SetHeldGrenade(grenadeType);
            if (heldGrenade != null)
            {
                heldGrenade.SetActive(true);
            }
        }

        public void SetHeldGrenadeActive(bool active)
        {
            if (heldGrenade != null)
            {
                heldGrenade.SetActive(active);
            }
        }

        private void SetFingers(float openProgress, float relaxWeight)
        {
            if (fingers == null)
            {
                return;
            }

            for (int i = 0; i < fingers.Length; i++)
            {
                fingers[i]?.Apply(openProgress, relaxWeight);
            }
        }

        public void SetPoseForPreview(Pose pose, float openProgress, float relaxWeight)
        {
            EnsureBuilt();
            if (armSystemRoot != null && !armSystemRoot.activeSelf)
            {
                armSystemRoot.SetActive(true);
            }

            SetPose(pose);
            SetFingers(openProgress, relaxWeight);
        }

        private void SetPose(Pose pose)
        {
            if (handRoot == null || armSystemRoot == null)
            {
                return;
            }

            Vector3 targetWristPos = pose.position;
            Quaternion wristRot = pose.rotation;

            // Compute distance from shoulder and clamp within physical reach (L1 + L2)
            Vector3 toWrist = targetWristPos - ShoulderLocalPos;
            float targetDist = toWrist.magnitude;
            float maxReach = L1 + L2 - 0.003f;
            float clampedDist = Mathf.Clamp(targetDist, 0.08f, maxReach);
            Vector3 toWristDir = targetDist > 0.001f ? toWrist / targetDist : Vector3.forward;
            Vector3 effectiveWristPos = ShoulderLocalPos + toWristDir * clampedDist;

            // 2-Bone Analytical IK
            float cosElbow = (L1 * L1 + clampedDist * clampedDist - L2 * L2) / (2f * L1 * clampedDist);
            float elbowAngle = Mathf.Acos(Mathf.Clamp(cosElbow, -1f, 1f));

            // Elbow naturally flares outward to the left and downward
            Vector3 bendPlaneRef = new Vector3(-0.6f, -0.8f, 0.1f);
            Vector3 bendAxis = Vector3.Cross(toWristDir, bendPlaneRef).normalized;
            if (bendAxis.sqrMagnitude < 0.01f)
            {
                bendAxis = Vector3.right;
            }

            Vector3 elbowDir = Quaternion.AngleAxis(elbowAngle * Mathf.Rad2Deg, bendAxis) * toWristDir;
            Vector3 elbowPos = ShoulderLocalPos + elbowDir * L1;

            // Orient Upper Arm
            if (upperArmBone != null)
            {
                upperArmBone.localPosition = ShoulderLocalPos;
                Vector3 upperFwd = (elbowPos - ShoulderLocalPos).normalized;
                upperArmBone.localRotation = Quaternion.LookRotation(upperFwd, Vector3.up);
            }

            // Orient Elbow and Forearm
            if (elbowBone != null)
            {
                elbowBone.localPosition = elbowPos;
            }

            if (forearmBone != null)
            {
                forearmBone.localPosition = elbowPos;
                Vector3 forearmFwd = (effectiveWristPos - elbowPos).normalized;
                forearmBone.localRotation = Quaternion.LookRotation(forearmFwd, Vector3.up);
            }

            // Position & Orient Wrist and Hand EXACTLY at effectiveWristPos
            if (wristBone != null)
            {
                wristBone.localPosition = effectiveWristPos;
                wristBone.localRotation = wristRot;
            }

            handRoot.localPosition = effectiveWristPos;
            handRoot.localRotation = wristRot;
        }

        private static Pose LerpPose(Pose from, Pose to, float progress)
        {
            return new Pose(
                Vector3.LerpUnclamped(from.position, to.position, progress),
                Quaternion.SlerpUnclamped(from.rotation, to.rotation, progress));
        }

        private static float Smooth(float value)
        {
            return value * value * (3f - 2f * value);
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.35f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        // =========================================================================
        // HIGH-DETAIL FIRST PERSON TACTICAL ARM & COMBAT GLOVE VISUAL BUILDER
        // =========================================================================

        private void BuildArmSystem()
        {
            Transform existing = transform.Find("Grenade Throw Arm System");
            if (existing != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(existing.gameObject);
                }
                else
                {
                    DestroyImmediate(existing.gameObject);
                }
            }

            armSystemRoot = new GameObject("Grenade Throw Arm System");
            armSystemRoot.transform.SetParent(transform, false);

            // Materials
            Material suitMaterial = GrenadeVisualFactory.CreateMaterial(new Color(0.12f, 0.14f, 0.17f), 0.05f, 0.22f);
            Material suitTrimMaterial = GrenadeVisualFactory.CreateMaterial(new Color(0.08f, 0.09f, 0.11f), 0.10f, 0.32f);
            Material armorMaterial = GrenadeVisualFactory.CreateMaterial(new Color(0.09f, 0.11f, 0.14f), 0.65f, 0.60f);
            Material metalPlateMaterial = GrenadeVisualFactory.CreateMaterial(new Color(0.32f, 0.35f, 0.38f), 0.85f, 0.75f);
            Material gloveBodyMaterial = GrenadeVisualFactory.CreateMaterial(new Color(0.16f, 0.18f, 0.22f), 0.14f, 0.38f);
            Material gloveGripMaterial = GrenadeVisualFactory.CreateMaterial(new Color(0.06f, 0.07f, 0.09f), 0.02f, 0.15f);
            Material cyanGlow = GrenadeVisualFactory.CreateEmissiveMaterial(new Color(0.05f, 0.88f, 1f), 3.0f);
            Material amberGlow = GrenadeVisualFactory.CreateEmissiveMaterial(new Color(1f, 0.65f, 0.08f), 2.5f);

            // 1. Upper Arm Bone (Originates at shoulder, aims towards elbow)
            GameObject upperArmObj = new GameObject("Upper Arm Bone");
            upperArmObj.transform.SetParent(armSystemRoot.transform, false);
            upperArmBone = upperArmObj.transform;

            // Base anchor extending off-screen to bottom-left to prevent visual gaps
            CreatePart(PrimitiveType.Capsule, "Upper Arm Base Anchor", upperArmBone,
                new Vector3(0f, 0f, -0.06f), new Vector3(0.142f, 0.12f, 0.142f),
                Quaternion.Euler(90f, 0f, 0f), suitMaterial);

            CreatePart(PrimitiveType.Capsule, "Upper Arm Sleeve Core", upperArmBone,
                new Vector3(0f, 0f, L1 * 0.5f), new Vector3(0.130f, L1 * 0.5f, 0.130f),
                Quaternion.Euler(90f, 0f, 0f), suitMaterial);
            CreatePart(PrimitiveType.Cylinder, "Shoulder Seam Cuff", upperArmBone,
                new Vector3(0f, 0f, 0.02f), new Vector3(0.138f, 0.025f, 0.138f),
                Quaternion.Euler(90f, 0f, 0f), suitTrimMaterial);
            CreatePart(PrimitiveType.Cylinder, "Bicep Tactical Strap", upperArmBone,
                new Vector3(0f, 0f, L1 * 0.65f), new Vector3(0.135f, 0.028f, 0.135f),
                Quaternion.Euler(90f, 0f, 0f), suitTrimMaterial);
            CreatePart(PrimitiveType.Cube, "Bicep Strap Buckle", upperArmBone,
                new Vector3(-0.062f, 0f, L1 * 0.65f), new Vector3(0.022f, 0.035f, 0.032f),
                Quaternion.identity, metalPlateMaterial);

            // 2. Elbow Bone (Joint pivot connecting upper arm and forearm)
            GameObject elbowObj = new GameObject("Elbow Joint Bone");
            elbowObj.transform.SetParent(armSystemRoot.transform, false);
            elbowBone = elbowObj.transform;

            CreatePart(PrimitiveType.Sphere, "Elbow Pivot Core", elbowBone,
                Vector3.zero, Vector3.one * 0.118f, Quaternion.identity, suitTrimMaterial);
            CreatePart(PrimitiveType.Cube, "Tactical Elbow Guard", elbowBone,
                new Vector3(-0.028f, -0.025f, -0.020f), new Vector3(0.104f, 0.082f, 0.054f),
                Quaternion.Euler(15f, -10f, 0f), armorMaterial);
            CreatePart(PrimitiveType.Cube, "Elbow Guard Accent", elbowBone,
                new Vector3(-0.028f, -0.025f, -0.048f), new Vector3(0.042f, 0.050f, 0.010f),
                Quaternion.identity, metalPlateMaterial);

            // 3. Forearm Bone (Extends from elbow towards wrist)
            GameObject forearmObj = new GameObject("Forearm Bone");
            forearmObj.transform.SetParent(armSystemRoot.transform, false);
            forearmBone = forearmObj.transform;

            CreatePart(PrimitiveType.Capsule, "Forearm Undersuit", forearmBone,
                new Vector3(0f, 0f, L2 * 0.5f), new Vector3(0.114f, L2 * 0.5f, 0.114f),
                Quaternion.Euler(90f, 0f, 0f), suitMaterial);

            // Forearm Exo-Gauntlet Armor Shell (mounted on top of forearm)
            CreatePart(PrimitiveType.Cube, "Gauntlet Armor Shell", forearmBone,
                new Vector3(0f, 0.050f, L2 * 0.52f), new Vector3(0.104f, 0.038f, L2 * 0.72f),
                Quaternion.identity, armorMaterial);
            CreatePart(PrimitiveType.Cube, "Gauntlet Left Bevel", forearmBone,
                new Vector3(-0.048f, 0.042f, L2 * 0.52f), new Vector3(0.020f, 0.030f, L2 * 0.68f),
                Quaternion.Euler(0f, 0f, 22f), armorMaterial);
            CreatePart(PrimitiveType.Cube, "Gauntlet Right Bevel", forearmBone,
                new Vector3(0.048f, 0.042f, L2 * 0.52f), new Vector3(0.020f, 0.030f, L2 * 0.68f),
                Quaternion.Euler(0f, 0f, -22f), armorMaterial);

            // Biometric Power Rail & Telemetry Indicators
            CreatePart(PrimitiveType.Cube, "Biometric Power Rail", forearmBone,
                new Vector3(0f, 0.071f, L2 * 0.52f), new Vector3(0.016f, 0.007f, L2 * 0.62f),
                Quaternion.identity, cyanGlow);
            CreatePart(PrimitiveType.Cube, "Gauntlet Status Node", forearmBone,
                new Vector3(-0.030f, 0.071f, L2 * 0.72f), new Vector3(0.022f, 0.008f, 0.022f),
                Quaternion.identity, amberGlow);
            CreatePart(PrimitiveType.Cylinder, "Gauntlet Bolt Top", forearmBone,
                new Vector3(0.032f, 0.068f, L2 * 0.72f), new Vector3(0.014f, 0.006f, 0.014f),
                Quaternion.identity, metalPlateMaterial);
            CreatePart(PrimitiveType.Cylinder, "Gauntlet Bolt Btm", forearmBone,
                new Vector3(0.032f, 0.068f, L2 * 0.32f), new Vector3(0.014f, 0.006f, 0.014f),
                Quaternion.identity, metalPlateMaterial);

            // 4. Wrist Bone
            GameObject wristObj = new GameObject("Wrist Bone");
            wristObj.transform.SetParent(armSystemRoot.transform, false);
            wristBone = wristObj.transform;

            CreatePart(PrimitiveType.Cylinder, "Wrist Collar", wristBone,
                new Vector3(0f, 0f, -0.018f), new Vector3(0.098f, 0.032f, 0.098f),
                Quaternion.Euler(90f, 0f, 0f), suitTrimMaterial);
            CreatePart(PrimitiveType.Cube, "Wrist Buckle Clasp", wristBone,
                new Vector3(0f, 0.045f, -0.018f), new Vector3(0.030f, 0.020f, 0.032f),
                Quaternion.identity, metalPlateMaterial);

            // 5. Hand Root (Combat Glove Palm, Knuckles, Fingers, and Grenade Cradle)
            GameObject handObj = new GameObject("Combat Glove Hand");
            handObj.transform.SetParent(armSystemRoot.transform, false);
            handRoot = handObj.transform;

            // Ergonomic Palm
            CreatePart(PrimitiveType.Cube, "Palm Main Body", handRoot,
                new Vector3(0f, 0f, 0.055f), new Vector3(0.108f, 0.042f, 0.115f),
                Quaternion.identity, gloveBodyMaterial);
            CreatePart(PrimitiveType.Cube, "Palm Heel Cushion", handRoot,
                new Vector3(-0.038f, 0.012f, 0.018f), new Vector3(0.040f, 0.026f, 0.055f),
                Quaternion.Euler(0f, -12f, 0f), gloveGripMaterial);
            CreatePart(PrimitiveType.Cube, "Inner Palm Grip", handRoot,
                new Vector3(0.004f, 0.020f, 0.058f), new Vector3(0.088f, 0.007f, 0.082f),
                Quaternion.identity, gloveGripMaterial);

            // Backhand Knuckle Armor Plate
            CreatePart(PrimitiveType.Cube, "Backhand Knuckle Plate", handRoot,
                new Vector3(0f, -0.020f, 0.060f), new Vector3(0.106f, 0.016f, 0.092f),
                Quaternion.identity, armorMaterial);
            CreatePart(PrimitiveType.Cube, "Backhand Cyber Trace", handRoot,
                new Vector3(-0.012f, -0.027f, 0.060f), new Vector3(0.009f, 0.006f, 0.076f),
                Quaternion.identity, cyanGlow);

            // 4x Knuckle Armor Studs across metacarpals
            for (int k = 0; k < 4; k++)
            {
                float kx = -0.036f + k * 0.024f;
                CreatePart(PrimitiveType.Cube, $"Knuckle Stud {k + 1}", handRoot,
                    new Vector3(kx, -0.025f, 0.108f), new Vector3(0.018f, 0.012f, 0.018f),
                    Quaternion.identity, metalPlateMaterial);
            }

            // Dedicated Grenade Mount Socket (Nestled naturally inside palm cradle)
            GameObject socketObj = new GameObject("Grenade Mount Socket");
            socketObj.transform.SetParent(handRoot, false);
            socketObj.transform.localPosition = new Vector3(-0.002f, 0.050f, 0.065f);
            socketObj.transform.localRotation = Quaternion.Euler(14f, 6f, -12f);
            grenadeMount = socketObj.transform;

            // 6. Articulated 5-Finger System
            fingers = new FingerJoint[5];

            // --- THUMB ---
            fingers[0] = BuildFinger(
                handRoot,
                "Thumb",
                new Vector3(-0.046f, 0.012f, 0.035f),
                proximalLength: 0.048f,
                proximalDiameter: 0.030f,
                distalLength: 0.040f,
                distalDiameter: 0.026f,
                gripRootRot: Quaternion.Euler(-32f, 38f, -22f),
                gripDistalRot: Quaternion.Euler(-34f, 0f, 0f),
                openRootRot: Quaternion.Euler(14f, -26f, 12f),
                openDistalRot: Quaternion.Euler(8f, 0f, 0f),
                relaxRootRot: Quaternion.Euler(-14f, 14f, -8f),
                relaxDistalRot: Quaternion.Euler(-10f, 0f, 0f),
                gloveBodyMaterial, gloveGripMaterial, armorMaterial);

            // --- INDEX FINGER ---
            fingers[1] = BuildFinger(
                handRoot,
                "Index Finger",
                new Vector3(-0.036f, 0.008f, 0.110f),
                proximalLength: 0.058f,
                proximalDiameter: 0.027f,
                distalLength: 0.045f,
                distalDiameter: 0.023f,
                gripRootRot: Quaternion.Euler(-55f, 4f, 4f),
                gripDistalRot: Quaternion.Euler(-48f, 0f, 0f),
                openRootRot: Quaternion.Euler(18f, -6f, 8f),
                openDistalRot: Quaternion.Euler(10f, 0f, 0f),
                relaxRootRot: Quaternion.Euler(-20f, 0f, 3f),
                relaxDistalRot: Quaternion.Euler(-14f, 0f, 0f),
                gloveBodyMaterial, gloveGripMaterial, armorMaterial);

            // --- MIDDLE FINGER ---
            fingers[2] = BuildFinger(
                handRoot,
                "Middle Finger",
                new Vector3(-0.012f, 0.010f, 0.116f),
                proximalLength: 0.064f,
                proximalDiameter: 0.028f,
                distalLength: 0.048f,
                distalDiameter: 0.024f,
                gripRootRot: Quaternion.Euler(-58f, 0f, 0f),
                gripDistalRot: Quaternion.Euler(-50f, 0f, 0f),
                openRootRot: Quaternion.Euler(20f, 0f, 0f),
                openDistalRot: Quaternion.Euler(12f, 0f, 0f),
                relaxRootRot: Quaternion.Euler(-22f, 0f, 0f),
                relaxDistalRot: Quaternion.Euler(-16f, 0f, 0f),
                gloveBodyMaterial, gloveGripMaterial, armorMaterial);

            // --- RING FINGER ---
            fingers[3] = BuildFinger(
                handRoot,
                "Ring Finger",
                new Vector3(0.012f, 0.008f, 0.112f),
                proximalLength: 0.060f,
                proximalDiameter: 0.027f,
                distalLength: 0.046f,
                distalDiameter: 0.023f,
                gripRootRot: Quaternion.Euler(-56f, -3f, -4f),
                gripDistalRot: Quaternion.Euler(-48f, 0f, 0f),
                openRootRot: Quaternion.Euler(18f, 4f, -6f),
                openDistalRot: Quaternion.Euler(10f, 0f, 0f),
                relaxRootRot: Quaternion.Euler(-20f, 0f, -3f),
                relaxDistalRot: Quaternion.Euler(-15f, 0f, 0f),
                gloveBodyMaterial, gloveGripMaterial, armorMaterial);

            // --- PINKY FINGER ---
            fingers[4] = BuildFinger(
                handRoot,
                "Pinky Finger",
                new Vector3(0.034f, 0.005f, 0.104f),
                proximalLength: 0.048f,
                proximalDiameter: 0.024f,
                distalLength: 0.038f,
                distalDiameter: 0.020f,
                gripRootRot: Quaternion.Euler(-52f, -6f, -10f),
                gripDistalRot: Quaternion.Euler(-42f, 0f, 0f),
                openRootRot: Quaternion.Euler(16f, 8f, -12f),
                openDistalRot: Quaternion.Euler(8f, 0f, 0f),
                relaxRootRot: Quaternion.Euler(-18f, 2f, -5f),
                relaxDistalRot: Quaternion.Euler(-12f, 0f, 0f),
                gloveBodyMaterial, gloveGripMaterial, armorMaterial);

            SetPose(RestPose);
            SetFingers(0f, 0f);
            armSystemRoot.SetActive(false);
        }

        private static FingerJoint BuildFinger(
            Transform parent,
            string name,
            Vector3 rootPos,
            float proximalLength,
            float proximalDiameter,
            float distalLength,
            float distalDiameter,
            Quaternion gripRootRot,
            Quaternion gripDistalRot,
            Quaternion openRootRot,
            Quaternion openDistalRot,
            Quaternion relaxRootRot,
            Quaternion relaxDistalRot,
            Material bodyMat,
            Material gripMat,
            Material armorMat)
        {
            // 1. Root knuckle pivot
            GameObject rootPivotObj = new GameObject($"{name} Root Pivot");
            rootPivotObj.transform.SetParent(parent, false);
            rootPivotObj.transform.localPosition = rootPos;
            rootPivotObj.transform.localRotation = gripRootRot;

            // Proximal phalanx segment (pointing along +Z)
            CreatePart(PrimitiveType.Capsule, $"{name} Proximal", rootPivotObj.transform,
                new Vector3(0f, 0f, proximalLength * 0.5f),
                new Vector3(proximalDiameter, proximalLength * 0.5f, proximalDiameter),
                Quaternion.Euler(90f, 0f, 0f), bodyMat);

            // Knuckle joint protective armor pad on dorsal side (-Y)
            CreatePart(PrimitiveType.Cube, $"{name} Knuckle Plate", rootPivotObj.transform,
                new Vector3(0f, -proximalDiameter * 0.45f, proximalLength * 0.45f),
                new Vector3(proximalDiameter * 0.90f, proximalDiameter * 0.35f, proximalLength * 0.65f),
                Quaternion.identity, armorMat);

            // 2. Distal knuckle pivot
            GameObject distalPivotObj = new GameObject($"{name} Distal Pivot");
            distalPivotObj.transform.SetParent(rootPivotObj.transform, false);
            distalPivotObj.transform.localPosition = new Vector3(0f, 0f, proximalLength);
            distalPivotObj.transform.localRotation = gripDistalRot;

            // Distal phalanx segment
            CreatePart(PrimitiveType.Capsule, $"{name} Distal", distalPivotObj.transform,
                new Vector3(0f, 0f, distalLength * 0.5f),
                new Vector3(distalDiameter, distalLength * 0.5f, distalDiameter),
                Quaternion.Euler(90f, 0f, 0f), bodyMat);

            // High-friction fingertip grip pad on palmar side (+Y)
            CreatePart(PrimitiveType.Cube, $"{name} Tip Grip Pad", distalPivotObj.transform,
                new Vector3(0f, distalDiameter * 0.38f, distalLength * 0.55f),
                new Vector3(distalDiameter * 0.85f, distalDiameter * 0.22f, distalLength * 0.60f),
                Quaternion.identity, gripMat);

            // Fingertip protective nail guard on dorsal side (-Y)
            CreatePart(PrimitiveType.Cube, $"{name} Tip Guard", distalPivotObj.transform,
                new Vector3(0f, -distalDiameter * 0.38f, distalLength * 0.50f),
                new Vector3(distalDiameter * 0.85f, distalDiameter * 0.20f, distalLength * 0.55f),
                Quaternion.identity, armorMat);

            return new FingerJoint
            {
                rootPivot = rootPivotObj.transform,
                distalPivot = distalPivotObj.transform,
                gripRootRot = gripRootRot,
                gripDistalRot = gripDistalRot,
                openRootRot = openRootRot,
                openDistalRot = openDistalRot,
                relaxRootRot = relaxRootRot,
                relaxDistalRot = relaxDistalRot
            };
        }

        private static GameObject CreatePart(
            PrimitiveType primitiveType,
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.transform.localRotation = localRotation;
            part.GetComponent<Renderer>().material = material;
            GrenadeVisualFactory.DisableCollider(part);
            return part;
        }
    }
}
