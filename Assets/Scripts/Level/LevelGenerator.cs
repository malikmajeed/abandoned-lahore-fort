using System.Collections.Generic;
using ForgottenFort.Core;
using ForgottenFort.Enemy;
using ForgottenFort.Interactables;
using ForgottenFort.Player;
using UnityEngine;

namespace ForgottenFort.Level
{
    [DefaultExecutionOrder(-50)]
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Environment Prefabs")]
        public GameObject floorPrefab;
        public GameObject wallPrefab;

        [Header("Character Prefabs")]
        public GameObject playerPrefab;
        public GameObject guardPrefab;

        [Header("Quest Prefabs")]
        public GameObject treasurePrefab;
        public GameObject keyPrefab;
        public GameObject mosaicPrefab;
        public GameObject doorPrefab;
        public GameObject doorOpenPrefab;
        public GameObject torchPrefab;
        public GameObject barrelPrefab;

        Transform tilesRoot;
        Transform objectsRoot;
        PlayerController spawnedPlayer;

        public PlayerController Player => spawnedPlayer;

        void Awake()
        {
            tilesRoot = new GameObject("Tiles").transform;
            tilesRoot.SetParent(transform);
            objectsRoot = new GameObject("Objects").transform;
            objectsRoot.SetParent(transform);

            ResolvePrefabs();
            FortLevelJsonLoader.EnsureLoaded();
            GenerateFromFortMap();
            SpawnCharacters();
        }

        void ResolvePrefabs()
        {
            var library = Resources.Load<LevelPrefabLibrary>("LevelPrefabLibrary");
            library?.ApplyTo(this);

            floorPrefab ??= PrefabFactory.FloorTemplate;
            wallPrefab ??= PrefabFactory.WallTemplate;
            playerPrefab ??= PrefabFactory.PlayerTemplate;
            guardPrefab ??= PrefabFactory.GuardTemplate;
            keyPrefab ??= PrefabFactory.KeyTemplate;
            treasurePrefab ??= PrefabFactory.TreasureTemplate;
            mosaicPrefab ??= PrefabFactory.MosaicTemplate;
            doorPrefab ??= PrefabFactory.DoorTemplate;
            doorOpenPrefab ??= PrefabFactory.DoorOpenTemplate;
            torchPrefab ??= PrefabFactory.TorchTemplate;
            barrelPrefab ??= PrefabFactory.BarrelTemplate;
        }

        static float SpriteWorldSize(Sprite sprite)
        {
            if (sprite == null) return 1f;
            return Mathf.Max(sprite.rect.width, sprite.rect.height) / sprite.pixelsPerUnit;
        }

        static void FitToTile(GameObject go, float tileFraction = 1f)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr?.sprite == null) return;
            float size = SpriteWorldSize(sr.sprite);
            if (size <= 0.001f) return;
            go.transform.localScale = Vector3.one * (GameConstants.TileSize * tileFraction / size);
        }

        static void FitCharacter(GameObject go, int sortingOrder)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr?.sprite == null) return;
            float size = SpriteWorldSize(sr.sprite);
            float scale = 0.85f / Mathf.Max(size, 0.001f);
            go.transform.localScale = Vector3.one * Mathf.Clamp(scale, 0.15f, 2.5f);
            sr.sortingOrder = sortingOrder;
        }

        static void SetSort(GameObject go, int order)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = order;
        }

        static void EnsurePlayer(GameObject go)
        {
            FitCharacter(go, 20);
            if (go.GetComponent<PlayerController>() == null)
                go.AddComponent<PlayerController>();
            var rb = go.GetComponent<Rigidbody2D>() ?? go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            var col = go.GetComponent<CircleCollider2D>() ?? go.AddComponent<CircleCollider2D>();
            col.radius = 0.28f;
        }

        static void EnsureWall(GameObject go)
        {
            FitToTile(go, 1f);
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = false;
                col.size = Vector2.one * 0.98f;
            }
        }

        static void EnsureBarrel(GameObject go)
        {
            FitToTile(go, 0.92f);
            SetSort(go, 9);
            var col = go.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                col.isTrigger = false;
                col.size = Vector2.one * 0.78f;
            }
            if (go.GetComponent<BarrelHideout>() == null)
                go.AddComponent<BarrelHideout>();
        }

        static void EnsureGuard(GameObject go)
        {
            FitCharacter(go, 18);
            if (go.GetComponent<GuardAI>() == null)
                go.AddComponent<GuardAI>();
            var col = go.GetComponent<CircleCollider2D>() ?? go.AddComponent<CircleCollider2D>();
            col.isTrigger = false;
            col.radius = 0.26f;
            var rb = go.GetComponent<Rigidbody2D>() ?? go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }

        static void EnsureKey(GameObject go)
        {
            FitToTile(go, 0.7f);
            SetSort(go, 12);
            if (go.GetComponent<KeyPickup>() == null)
                go.AddComponent<KeyPickup>();
            EnsureTrigger(go, 0.35f);
        }

        static void EnsureChest(GameObject go, bool royal)
        {
            FitToTile(go, 0.85f);
            SetSort(go, 12);
            var chest = go.GetComponent<TreasureChest>() ?? go.AddComponent<TreasureChest>();
            chest.IsRoyalSeal = royal;
            EnsureTrigger(go, 0.4f);
        }

        void EnsureDoor(GameObject go, string doorId, int doorIndex)
        {
            FitToTile(go, 1f);
            SetSort(go, 14);
            var door = go.GetComponent<LockedDoor>() ?? go.AddComponent<LockedDoor>();
            door.DoorId = doorId;
            door.DoorIndex = doorIndex;
            door.OpenDoorPrefab = doorOpenPrefab;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && door.LockedSprite == null)
                door.LockedSprite = sr.sprite;
            var col = go.GetComponent<BoxCollider2D>() ?? go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one * 0.92f;
        }

        static void EnsureTrigger(GameObject go, float radius)
        {
            var col = go.GetComponent<CircleCollider2D>() ?? go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;
        }

        void GenerateFromFortMap()
        {
            int keyIndex = 0;
            int mosaicIndex = 0;
            int doorIndex = 0;

            for (int y = 0; y < FortLevelData.Height; y++)
            {
                for (int x = 0; x < FortLevelData.Width; x++)
                {
                    char tile = FortLevelData.GetTile(x, y);
                    Vector3 pos = FortLevelData.GridToWorld(x, y);

                    if (tile == '#')
                    {
                        var wall = SpawnTile(wallPrefab, pos, tilesRoot);
                        EnsureWall(wall);
                        SetSort(wall, 5);
                        continue;
                    }

                    var floor = SpawnTile(floorPrefab, pos, tilesRoot);
                    FitToTile(floor, 1f);
                    SetSort(floor, 0);

                    switch (tile)
                    {
                        case 'T':
                            PlaceTorch(pos);
                            break;
                        case 'B':
                            var barrel = PrefabFactory.Spawn(barrelPrefab, pos, objectsRoot);
                            EnsureBarrel(barrel);
                            break;
                        case 'K':
                            SpawnKey(pos, KeyIdForIndex(keyIndex), keyIndex);
                            keyIndex++;
                            break;
                        case 'D':
                            SpawnDoor(pos, FortLevelData.GetDoorId(doorIndex), doorIndex);
                            doorIndex++;
                            break;
                        case 'M':
                            if (mosaicIndex < GameConstants.MosaicFragmentsRequired)
                            {
                                var mosaic = PrefabFactory.Spawn(mosaicPrefab, pos, objectsRoot);
                                FitToTile(mosaic, 0.65f);
                                SetSort(mosaic, 12);
                                if (mosaic.GetComponent<MosaicFragment>() == null)
                                    mosaic.AddComponent<MosaicFragment>();
                                EnsureTrigger(mosaic, 0.35f);
                                mosaicIndex++;
                            }
                            break;
                        case 'X':
                            SpawnChest(pos, true);
                            break;
                    }
                }
            }
        }

        static string KeyIdForIndex(int index)
        {
            if (FortLevelJsonLoader.IsLoaded && FortLevelJsonLoader.Data?.keys != null &&
                index >= 0 && index < FortLevelJsonLoader.Data.keys.Length)
                return FortLevelJsonLoader.Data.keys[index].id;

            string[] keyIds = { "roshanai", "akbari", "hazuri" };
            return keyIds[index % keyIds.Length];
        }

        static string KeyDisplayName(int index)
        {
            if (FortLevelJsonLoader.IsLoaded && FortLevelJsonLoader.Data?.keys != null &&
                index >= 0 && index < FortLevelJsonLoader.Data.keys.Length)
                return FortLevelJsonLoader.Data.keys[index].id;

            return index < GameConstants.KeyNames.Length
                ? GameConstants.KeyNames[index]
                : $"Key {index + 1}";
        }

        static GameObject SpawnTile(GameObject prefab, Vector3 pos, Transform parent) =>
            PrefabFactory.Spawn(prefab, pos, parent);

        void PlaceTorch(Vector3 pos)
        {
            var glow = new GameObject("TorchGlow");
            glow.transform.SetParent(objectsRoot);
            glow.transform.position = pos;
            var gsr = glow.AddComponent<SpriteRenderer>();
            gsr.sprite = TileTextureFactory.TorchGlow();
            gsr.sortingOrder = 2;

            var torch = PrefabFactory.Spawn(torchPrefab, pos + new Vector3(0, 0.05f, 0), objectsRoot);
            FitToTile(torch, 0.75f);
            SetSort(torch, 9);
            if (torch.GetComponent<TorchFlicker>() == null)
                torch.AddComponent<TorchFlicker>();
        }

        void SpawnKey(Vector3 pos, string keyId, int index)
        {
            var go = PrefabFactory.Spawn(keyPrefab, pos, objectsRoot);
            EnsureKey(go);
            var key = go.GetComponent<KeyPickup>();
            key.KeyId = keyId;
            key.DisplayName = KeyDisplayName(index);
        }

        void SpawnDoor(Vector3 pos, string doorId, int doorIndex)
        {
            var go = PrefabFactory.Spawn(doorPrefab, pos, objectsRoot);
            EnsureDoor(go, doorId, doorIndex);
        }

        void SpawnChest(Vector3 pos, bool isRoyal)
        {
            var go = PrefabFactory.Spawn(treasurePrefab, pos, objectsRoot);
            EnsureChest(go, isRoyal);
        }

        void SpawnCharacters()
        {
            var start = FortLevelData.StartPosition;
            var playerGo = PrefabFactory.Spawn(playerPrefab, FortLevelData.GridToWorld(start.x, start.y), objectsRoot);
            EnsurePlayer(playerGo);
            spawnedPlayer = playerGo.GetComponent<PlayerController>();

            if (FortLevelJsonLoader.IsLoaded && FortLevelJsonLoader.Guards.Count > 0)
            {
                foreach (var g in FortLevelJsonLoader.Guards)
                {
                    var guardGo = PrefabFactory.Spawn(guardPrefab, FortLevelData.GridToWorld(g.x, g.y), objectsRoot);
                    EnsureGuard(guardGo);
                    var guard = guardGo.GetComponent<GuardAI>();
                    if (guard == null || g.patrolX == null || g.patrolY == null) continue;
                    var points = new List<Vector3>();
                    for (int i = 0; i < g.patrolX.Length && i < g.patrolY.Length; i++)
                        points.Add(FortLevelData.GridToWorld(g.patrolX[i], g.patrolY[i]));
                    if (points.Count == 0)
                        points.Add(FortLevelData.GridToWorld(g.x, g.y));
                    guard.PatrolWorldPoints = points.ToArray();
                }
                return;
            }

            var guardSpawns = new List<Vector2Int>();
            for (int y = 0; y < FortLevelData.Height; y++)
            for (int x = 0; x < FortLevelData.Width; x++)
                if (FortLevelData.GetTile(x, y) == 'G')
                    guardSpawns.Add(new Vector2Int(x, y));

            foreach (var spawn in guardSpawns)
            {
                var path = BuildGuardPatrol(spawn);
                var guardGo = PrefabFactory.Spawn(guardPrefab, FortLevelData.GridToWorld(path[0].x, path[0].y), objectsRoot);
                EnsureGuard(guardGo);
                var guard = guardGo.GetComponent<GuardAI>();
                if (guard != null)
                    guard.PatrolWorldPoints = System.Array.ConvertAll(path, p => FortLevelData.GridToWorld(p.x, p.y));
            }
        }

        static Vector2Int[] BuildGuardPatrol(Vector2Int center)
        {
            var points = new List<Vector2Int> { center };
            var offsets = new[]
            {
                new Vector2Int(3, 0), new Vector2Int(0, 2),
                new Vector2Int(-3, 0), new Vector2Int(0, -2),
            };
            foreach (var offset in offsets)
            {
                var p = center + offset;
                if (FortLevelData.IsWalkable(p.x, p.y))
                    points.Add(p);
            }
            if (points.Count < 2)
            {
                var fallback = center + new Vector2Int(2, 0);
                if (FortLevelData.IsWalkable(fallback.x, fallback.y))
                    points.Add(fallback);
            }
            return points.ToArray();
        }
    }
}
