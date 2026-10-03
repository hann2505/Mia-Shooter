using System;
using UnityEngine;

namespace MiaShooter
{
    public sealed class DemoGameManager : MonoBehaviour
    {
        public static DemoGameManager Instance { get; private set; }
        public static event Action TargetDestroyed;
        public static event Action<int, Vector3, Color> TargetEliminated;

        public int Score { get; private set; }
        public int TargetsDestroyed { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterDestroyedTarget(int scoreValue, Vector3 targetPosition = default, Color targetColor = default)
        {
            Score += scoreValue;
            TargetsDestroyed++;
            TargetDestroyed?.Invoke();
            TargetEliminated?.Invoke(scoreValue, targetPosition, targetColor);
        }
    }
}
