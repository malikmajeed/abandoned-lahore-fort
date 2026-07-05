using ForgottenFort.Core;
using ForgottenFort.Level;
using UnityEngine;

namespace ForgottenFort.Interactables
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LockedDoor : MonoBehaviour
    {
        public string DoorId = "gate";
        public int DoorIndex;
        public GameObject OpenDoorPrefab;
        public Sprite LockedSprite;
        public Sprite OpenSprite;
        public bool IsLocked = true;

        SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (LockedSprite == null && sr != null)
                LockedSprite = sr.sprite;
            if (OpenSprite == null)
                OpenSprite = TileTextureFactory.LoadObjectSprite("Sprites/Objects/door_open");
        }

        void Start()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = true;
                if (col.size.x > 1.5f)
                    col.size = Vector2.one * 0.92f;
            }
            UpdateVisual();
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.GetComponent<Player.PlayerController>() != null)
                TryOpen();
        }

        public void TryOpen()
        {
            if (!IsLocked) return;
            if (GameManager.Instance == null || !GameManager.Instance.TryConsumeKeyForDoor())
                return;

            OpenPassage();
        }

        void OpenPassage()
        {
            IsLocked = false;
            var col = GetComponent<BoxCollider2D>();
            if (col != null) col.enabled = false;

            if (OpenDoorPrefab != null)
            {
                var opened = Instantiate(OpenDoorPrefab, transform.position, Quaternion.identity, transform.parent);
                opened.transform.localScale = transform.localScale;
                var openSr = opened.GetComponent<SpriteRenderer>();
                if (openSr != null)
                    openSr.sortingOrder = 14;
                gameObject.SetActive(false);
                return;
            }

            UpdateVisual();
        }

        void UpdateVisual()
        {
            if (sr == null) return;
            if (!IsLocked && OpenSprite != null)
                sr.sprite = OpenSprite;
            else if (LockedSprite != null)
                sr.sprite = LockedSprite;
        }
    }
}
