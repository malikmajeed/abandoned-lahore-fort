using ForgottenFort.Core;
using UnityEngine;

namespace ForgottenFort.Interactables
{
    public class TreasureChest : MonoBehaviour
    {
        public bool IsRoyalSeal;
        public bool RequiresPuzzle = true;
        public bool Collected;

        void Awake()
        {
            ConfigureCollider();
        }

        void ConfigureCollider()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.isTrigger = true;
                return;
            }

            var circle = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.45f;
        }

        void OnTriggerEnter2D(Collider2D other) => TryCollect(other);
        void OnTriggerStay2D(Collider2D other) => TryCollect(other);

        void TryCollect(Collider2D other)
        {
            if (Collected) return;
            if (!other.GetComponent<Player.PlayerController>()) return;
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

            if (RequiresPuzzle && !GameManager.Instance.PuzzleSolved)
                return;

            Collected = true;
            if (IsRoyalSeal)
                GameManager.Instance.WinGame();
        }
    }
}
