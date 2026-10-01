using UnityEngine;

namespace MiaShooter
{
    public sealed class DemoGameManager : MonoBehaviour
    {
        public static DemoGameManager Instance { get; private set; }

        public int Score { get; private set; }
        public int TargetsDestroyed { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void RegisterDestroyedTarget(int scoreValue)
        {
            Score += scoreValue;
            TargetsDestroyed++;
        }
    }
}
