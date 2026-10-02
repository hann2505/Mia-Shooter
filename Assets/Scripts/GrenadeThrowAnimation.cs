using System;
using System.Collections;
using UnityEngine;

namespace MiaShooter
{
    [DisallowMultipleComponent]
    public sealed class GrenadeThrowAnimation : MonoBehaviour
    {
        [SerializeField] private float equipDuration = 0.22f;
        [SerializeField] private float windupDuration = 0.16f;
        [SerializeField] private float throwDuration = 0.25f;
        [SerializeField] private float recoverDuration = 0.22f;
        [SerializeField, Range(0.1f, 0.9f)] private float releasePoint = 0.58f;

        private static readonly Pose RestPose = new Pose(
            new Vector3(-0.48f, -0.72f, 0.68f), Quaternion.Euler(28f, -22f, 18f));
        private static readonly Pose HoldPose = new Pose(
            new Vector3(-0.31f, -0.24f, 0.72f), Quaternion.Euler(16f, -24f, -12f));
        private static readonly Pose WindupPose = new Pose(
            new Vector3(-0.38f, -0.32f, 0.62f), Quaternion.Euler(18f, -32f, -24f));
        private static readonly Pose ReleasePose = new Pose(
            new Vector3(-0.08f, -0.12f, 1.08f), Quaternion.Euler(-32f, 4f, 42f));

        private GameObject handRoot;
        private GameObject heldGrenade;
        private GrenadeType equippedType;

        public bool IsPlaying { get; private set; }
        public bool IsEquipped { get; private set; }
        public bool IsReadyToThrow => IsEquipped && !IsPlaying;
        public GrenadeType EquippedType => equippedType;
        public Transform HandTransform => handRoot != null ? handRoot.transform : null;

        private void Awake()
        {
            BuildHand();
        }

        public bool Equip(GrenadeType grenadeType)
        {
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
            handRoot.SetActive(true);
            SetPose(RestPose);
            yield return AnimatePose(RestPose, HoldPose, equipDuration);
            IsPlaying = false;
        }

        private IEnumerator UnequipRoutine(Action completed)
        {
            IsPlaying = true;
            yield return AnimatePose(HoldPose, RestPose, recoverDuration);
            FinishAndHide(completed);
        }

        private IEnumerator ThrowRoutine(Action<Vector3> releaseGrenade, Action completed)
        {
            IsPlaying = true;
            yield return AnimatePose(HoldPose, WindupPose, windupDuration);

            bool released = false;
            float elapsed = 0f;
            while (elapsed < throwDuration)
            {
                float progress = Mathf.Clamp01(elapsed / throwDuration);
                SetPose(LerpPose(WindupPose, ReleasePose, Smooth(progress)));
                if (!released && progress >= releasePoint)
                {
                    released = true;
                    ReleaseHeldGrenade(releaseGrenade);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            SetPose(ReleasePose);
            if (!released)
            {
                ReleaseHeldGrenade(releaseGrenade);
            }

            yield return AnimatePose(ReleasePose, RestPose, recoverDuration);
            FinishAndHide(completed);
        }

        private IEnumerator AnimatePose(Pose from, Pose to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float progress = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
                SetPose(LerpPose(from, to, Smooth(progress)));
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetPose(to);
        }

        private void FinishAndHide(Action completed)
        {
            handRoot.SetActive(false);
            IsEquipped = false;
            IsPlaying = false;
            completed?.Invoke();
        }

        private void ReleaseHeldGrenade(Action<Vector3> releaseGrenade)
        {
            releaseGrenade(heldGrenade.transform.position);
            heldGrenade.SetActive(false);
        }

        private void SetHeldGrenade(GrenadeType grenadeType)
        {
            equippedType = grenadeType;
            if (heldGrenade != null)
            {
                Destroy(heldGrenade);
            }

            heldGrenade = GrenadeVisualFactory.Create(
                grenadeType == GrenadeType.Smoke ? "Held Smoke Grenade" : "Held Fragmentation Grenade",
                grenadeType);
            GrenadeVisualFactory.DisableCollider(heldGrenade);
            heldGrenade.transform.SetParent(handRoot.transform, false);
            heldGrenade.transform.localPosition = new Vector3(0f, 0.12f, 0.32f);
            heldGrenade.transform.localRotation = Quaternion.Euler(8f, 0f, -12f);
        }

        private void BuildHand()
        {
            handRoot = new GameObject("Grenade Throw Hand");
            handRoot.transform.SetParent(transform, false);

            Material armor = GrenadeVisualFactory.CreateMaterial(new Color(0.055f, 0.08f, 0.11f), 0.72f, 0.58f);
            Material glove = GrenadeVisualFactory.CreateMaterial(new Color(0.16f, 0.19f, 0.2f), 0.35f, 0.42f);
            Material accent = GrenadeVisualFactory.CreateEmissiveMaterial(new Color(0.05f, 0.7f, 1f), 2.4f);

            CreateHandPart(PrimitiveType.Capsule, "Left Forearm", new Vector3(-0.17f, -0.17f, -0.12f),
                new Vector3(0.16f, 0.36f, 0.16f), Quaternion.Euler(58f, 0f, -18f), armor);
            CreateHandPart(PrimitiveType.Cube, "Left Palm", new Vector3(0f, 0f, 0.08f),
                new Vector3(0.24f, 0.15f, 0.3f), Quaternion.Euler(-8f, 0f, -6f), glove);
            CreateHandPart(PrimitiveType.Cube, "Glove Light", new Vector3(-0.12f, 0.02f, 0.06f),
                new Vector3(0.035f, 0.06f, 0.18f), Quaternion.identity, accent);

            for (int i = 0; i < 3; i++)
            {
                CreateHandPart(PrimitiveType.Capsule, $"Left Finger {i + 1}",
                    new Vector3(-0.075f + i * 0.075f, 0.035f, 0.25f),
                    new Vector3(0.045f, 0.12f, 0.045f), Quaternion.Euler(72f, 0f, 0f), glove);
            }

            SetPose(RestPose);
            handRoot.SetActive(false);
        }

        private void CreateHandPart(
            PrimitiveType primitiveType,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitiveType);
            part.name = name;
            part.transform.SetParent(handRoot.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.transform.localRotation = localRotation;
            part.GetComponent<Renderer>().material = material;
            GrenadeVisualFactory.DisableCollider(part);
        }

        private void SetPose(Pose pose)
        {
            handRoot.transform.localPosition = pose.position;
            handRoot.transform.localRotation = pose.rotation;
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
    }
}
