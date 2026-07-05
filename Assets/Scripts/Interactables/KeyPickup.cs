using ForgottenFort.Core;
using UnityEngine;

namespace ForgottenFort.Interactables
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class KeyPickup : MonoBehaviour
    {
        public string KeyId = "roshanai";
        public string DisplayName = "Roshanai Key";

        void Awake()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<Player.PlayerController>()) return;
            GameManager.Instance?.CollectKey(DisplayName);
            Destroy(gameObject);
        }
    }
}
