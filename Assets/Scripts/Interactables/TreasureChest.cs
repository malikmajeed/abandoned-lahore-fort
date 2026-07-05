using ForgottenFort.Core;
using UnityEngine;

namespace ForgottenFort.Interactables
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class TreasureChest : MonoBehaviour
    {
        public bool IsRoyalSeal;

        void Awake()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<Player.PlayerController>()) return;
            if (IsRoyalSeal)
            {
                if (GameManager.Instance != null && GameManager.Instance.PuzzleSolved)
                    GameManager.Instance.WinGame();
            }
        }
    }
}
