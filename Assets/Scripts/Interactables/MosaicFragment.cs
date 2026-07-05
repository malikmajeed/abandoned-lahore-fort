using ForgottenFort.Core;
using UnityEngine;

namespace ForgottenFort.Interactables
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class MosaicFragment : MonoBehaviour
    {
        void Awake()
        {
            var col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<Player.PlayerController>()) return;
            GameManager.Instance?.CollectMosaicFragment();
            GameManager.Instance?.SolvePuzzle();
            Destroy(gameObject);
        }
    }
}
