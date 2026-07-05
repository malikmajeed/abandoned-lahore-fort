using ForgottenFort.Core;
using ForgottenFort.Interactables;
using ForgottenFort.Enemy;
using ForgottenFort.Player;
using UnityEngine;

namespace ForgottenFort.Level
{
    /// <summary>
    /// Builds hidden template objects used when Inspector prefab slots are empty.
    /// </summary>
    public static class PrefabFactory
    {
        static Transform _root;
        static GameObject _floor, _wall, _wallTop, _player, _guard, _key, _treasure, _door, _doorOpen, _mosaic, _torch, _barrel;

        static Transform Root
        {
            get
            {
                if (_root != null) return _root;
                var go = new GameObject("[PrefabTemplates]");
                go.hideFlags = HideFlags.HideInHierarchy;
                Object.DontDestroyOnLoad(go);
                _root = go.transform;
                return _root;
            }
        }

        public static GameObject FloorTemplate => _floor ??= MakeSpritePrefab("FloorTemplate", TileTextureFactory.Floor(0, 0), 0, 1f);
        public static GameObject WallTemplate => _wall ??= MakeSpritePrefab("WallTemplate", TileTextureFactory.WallBlock(), 5, 1f);
        public static GameObject WallTopTemplate => _wallTop ??= MakeSpritePrefab("WallTopTemplate", TileTextureFactory.WallTopFace(), 6, 1f);

        public static GameObject PlayerTemplate =>
            _player ??= MakeCharacterPrefab("PlayerTemplate", TileTextureFactory.LoadCharacterSprite("Sprites/Characters/player_idle_down"), true);

        public static GameObject GuardTemplate =>
            _guard ??= MakeCharacterPrefab("GuardTemplate", TileTextureFactory.LoadCharacterSprite("Sprites/Characters/guard_idle_down"), false);

        public static GameObject KeyTemplate => _key ??= MakePickupPrefab("KeyTemplate", TileTextureFactory.LoadObjectSprite("Sprites/Objects/key_roshanai"), typeof(KeyPickup), 8, 0.35f);
        public static GameObject TreasureTemplate => _treasure ??= MakePickupPrefab("TreasureTemplate", TileTextureFactory.LoadObjectSprite("Sprites/Objects/chest_royal"), typeof(TreasureChest), 8, 0.4f);
        public static GameObject DoorTemplate => _door ??= MakeDoorPrefab();
        public static GameObject DoorOpenTemplate =>
            _doorOpen ??= MakeSpritePrefab("DoorOpenTemplate", TileTextureFactory.LoadObjectSprite("Sprites/Objects/door_open"), 14, 1f);
        public static GameObject MosaicTemplate => _mosaic ??= MakePickupPrefab("MosaicTemplate", TileTextureFactory.LoadObjectSprite("Sprites/Objects/mosaic_0"), typeof(MosaicFragment), 8, 0.35f);
        public static GameObject TorchTemplate => _torch ??= MakeTorchPrefab();
        public static GameObject BarrelTemplate => _barrel ??= MakeSpritePrefab("BarrelTemplate", TileTextureFactory.LoadObjectSprite("Sprites/Objects/barrel_0"), 3, 0.85f);

        static GameObject MakeSpritePrefab(string name, Sprite sprite, int order, float scale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root);
            go.SetActive(false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        static GameObject MakeCharacterPrefab(string name, Sprite sprite, bool isPlayer)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root);
            go.SetActive(false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * 0.85f;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.28f;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            if (isPlayer) go.AddComponent<PlayerController>();
            else go.AddComponent<GuardAI>();
            return go;
        }

        static GameObject MakePickupPrefab(string name, Sprite sprite, System.Type pickupType, int order, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(Root);
            go.SetActive(false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            go.transform.localScale = Vector3.one * 0.9f;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;
            go.AddComponent(pickupType);
            return go;
        }

        static GameObject MakeDoorPrefab()
        {
            var go = new GameObject("DoorTemplate");
            go.transform.SetParent(Root);
            go.SetActive(false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = TileTextureFactory.LoadObjectSprite("Sprites/Objects/door_locked_gold");
            sr.sortingOrder = 7;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one * 0.85f;
            var door = go.AddComponent<LockedDoor>();
            door.LockedSprite = sr.sprite;
            door.OpenSprite = TileTextureFactory.LoadObjectSprite("Sprites/Objects/door_open");
            return go;
        }

        static GameObject MakeTorchPrefab()
        {
            var go = new GameObject("TorchTemplate");
            go.transform.SetParent(Root);
            go.SetActive(false);
            var glow = new GameObject("Glow");
            glow.transform.SetParent(go.transform, false);
            var gsr = glow.AddComponent<SpriteRenderer>();
            gsr.sprite = TileTextureFactory.TorchGlow();
            gsr.sortingOrder = 1;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = TileTextureFactory.LoadObjectSprite("Sprites/Objects/torch_0");
            sr.sortingOrder = 4;
            go.transform.localScale = Vector3.one * 0.85f;
            go.AddComponent<TorchFlicker>();
            return go;
        }

        public static GameObject Spawn(GameObject template, Vector3 position, Transform parent)
        {
            var instance = Object.Instantiate(template, position, Quaternion.identity, parent);
            instance.SetActive(true);
            return instance;
        }
    }
}
