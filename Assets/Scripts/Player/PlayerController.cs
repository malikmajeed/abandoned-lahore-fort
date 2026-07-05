using System.Collections.Generic;
using ForgottenFort.Core;
using ForgottenFort.Level;
using UnityEngine;

namespace ForgottenFort.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = GameConstants.PlayerMoveSpeed;
        public bool makeNoiseOnMove = true;

        SpriteRenderer spriteRenderer;
        Rigidbody2D rb;
        Vector2 moveInput;
        Vector2Int currentGrid;
        bool isMoving;
        bool isSprinting;

        public bool IsSprinting => isSprinting;
        float footstepTimer;
        int walkFrame;
        float animTimer;
        float invulnTimer;

        readonly Dictionary<string, Sprite[]> walkSprites = new();
        Sprite[] idleSprites = new Sprite[4];
        string facing = "down";
        bool usePrefabArt;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0;
            }
            spriteRenderer.sortingOrder = 10;
            moveSpeed = GameConstants.PlayerMoveSpeed;
            usePrefabArt = spriteRenderer.sprite != null;
            if (!usePrefabArt)
                LoadSprites();
        }

        void Start()
        {
            var start = FortLevelData.StartPosition;
            currentGrid = start;
            var pos = FortLevelData.GridToWorld(start.x, start.y);
            if (rb != null) rb.position = pos;
            transform.position = pos;
        }

        void LoadSprites()
        {
            string[] dirs = { "down", "up", "left", "right" };
            for (int i = 0; i < 4; i++)
            {
                idleSprites[i] = LoadCharSprite($"player_idle_{dirs[i]}");
                var frames = new Sprite[4];
                for (int f = 0; f < 4; f++)
                    frames[f] = LoadCharSprite($"player_walk_{dirs[i]}_{f}");
                walkSprites[dirs[i]] = frames;
            }
            if (idleSprites[0] != null) spriteRenderer.sprite = idleSprites[0];
        }

        static Sprite LoadCharSprite(string name)
        {
            return TileTextureFactory.LoadCharacterSprite($"Sprites/Characters/{name}");
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
                return;

            UpdateInvulnerability();
            ReadInput();
            Move();
            Animate();
        }

        void UpdateInvulnerability()
        {
            if (invulnTimer <= 0f) return;
            invulnTimer -= Time.deltaTime;
            if (spriteRenderer == null) return;
            if (invulnTimer > 0f)
                spriteRenderer.color = new Color(1f, 1f, 1f, Mathf.PingPong(Time.time * 12f, 1f) > 0.5f ? 0.45f : 1f);
            else
                spriteRenderer.color = Color.white;
        }

        public bool IsInvulnerable => invulnTimer > 0f;

        public void OnGuardHit(Vector3 guardPosition)
        {
            if (IsInvulnerable) return;
            if (GameManager.Instance == null) return;

            GameManager.Instance.TakeDamage();
            invulnTimer = GameConstants.PlayerInvulnerabilitySeconds;

            Vector2 away = ((Vector2)transform.position - (Vector2)guardPosition).normalized;
            if (away.sqrMagnitude < 0.01f) away = Vector2.up;
            Vector3 knockback = transform.position + (Vector3)(away * 0.8f);
            if (CanMoveTo(knockback))
            {
                if (rb != null) rb.position = knockback;
                else transform.position = knockback;
            }
        }

        void ReadInput()
        {
            moveInput = Vector2.zero;
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
                moveInput.y = 1;
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
                moveInput.y = -1;
            else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                moveInput.x = -1;
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                moveInput.x = 1;

            isSprinting = moveInput != Vector2.zero &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                GameManager.Instance?.TogglePause();
        }

        float CurrentMoveSpeed =>
            moveSpeed * (isSprinting ? GameConstants.PlayerSprintMultiplier : 1f);

        void Move()
        {
            if (moveInput == Vector2.zero)
            {
                isMoving = false;
                return;
            }

            isMoving = true;
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
                facing = moveInput.x > 0 ? "right" : "left";
            else
                facing = moveInput.y > 0 ? "up" : "down";

            Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0).normalized * CurrentMoveSpeed * Time.deltaTime;
            Vector3 newPos = transform.position + delta;

            TryOpenNearbyDoors(newPos);

            if (CanMoveTo(newPos))
            {
                if (rb != null) rb.MovePosition(newPos);
                else transform.position = newPos;
                currentGrid = FortLevelData.WorldToGrid(newPos);

                if (makeNoiseOnMove)
                {
                    float interval = isSprinting
                        ? GameConstants.SprintNoiseInterval
                        : GameConstants.WalkNoiseInterval;
                    footstepTimer += Time.deltaTime;
                    if (footstepTimer > interval)
                    {
                        footstepTimer = 0;
                        NotifySound(transform.position, isSprinting);
                    }
                }
            }
        }

        static void TryOpenNearbyDoors(Vector3 pos)
        {
            var hits = Physics2D.OverlapCircleAll(pos, 0.55f);
            foreach (var hit in hits)
            {
                var door = hit.GetComponent<Interactables.LockedDoor>();
                door?.TryOpen();
            }
        }

        bool CanMoveTo(Vector3 pos)
        {
            float r = 0.25f;
            Vector2Int[] checks =
            {
                FortLevelData.WorldToGrid(pos + new Vector3(-r, -r)),
                FortLevelData.WorldToGrid(pos + new Vector3(r, -r)),
                FortLevelData.WorldToGrid(pos + new Vector3(-r, r)),
                FortLevelData.WorldToGrid(pos + new Vector3(r, r)),
            };
            foreach (var g in checks)
                if (FortLevelData.BlocksMovement(g.x, g.y))
                    return false;

            // Check locked doors
            var hits = Physics2D.OverlapCircleAll(pos, 0.2f);
            foreach (var hit in hits)
            {
                var door = hit.GetComponent<Interactables.LockedDoor>();
                if (door != null && door.IsLocked)
                    return false;
            }
            return true;
        }

        void Animate()
        {
            if (usePrefabArt)
            {
                spriteRenderer.flipX = facing == "left";
                return;
            }

            if (!walkSprites.ContainsKey(facing)) return;
            if (isMoving)
            {
                animTimer += Time.deltaTime;
                if (animTimer > 0.12f)
                {
                    animTimer = 0;
                    walkFrame = (walkFrame + 1) % 4;
                }
                var frames = walkSprites[facing];
                if (frames[walkFrame] != null)
                    spriteRenderer.sprite = frames[walkFrame];
            }
            else
            {
                walkFrame = 0;
                int idx = facing switch { "up" => 1, "left" => 2, "right" => 3, _ => 0 };
                if (idleSprites[idx] != null)
                    spriteRenderer.sprite = idleSprites[idx];
            }
        }

        public static void NotifySound(Vector3 position, bool loud = false)
        {
            var guards = FindObjectsByType<Enemy.GuardAI>(FindObjectsSortMode.None);
            foreach (var g in guards)
                g.OnHeardNoise(position, loud);
        }

        public Vector2Int GridPosition => FortLevelData.WorldToGrid(transform.position);
    }
}
