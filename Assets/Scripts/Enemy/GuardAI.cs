using ForgottenFort.Core;
using ForgottenFort.Level;
using ForgottenFort.Player;
using UnityEngine;

namespace ForgottenFort.Enemy
{
    public enum GuardState
    {
        Patrol,
        Suspicious,
        Search,
        Chase
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class GuardAI : MonoBehaviour
    {
        public Vector3[] PatrolWorldPoints;
        public float viewAngle = 360f;

        GuardState state = GuardState.Patrol;
        int patrolIndex;
        Vector3 lastKnownPlayerPos;
        float searchTimer;
        float hitCooldown;
        float loseSightTimer;
        float lastChaseDistance;
        float giveUpTimer;
        SpriteRenderer spriteRenderer;
        Transform player;
        string facing = "down";

        Sprite suspiciousLeft, suspiciousRight, chaseSprite;
        readonly System.Collections.Generic.Dictionary<string, Sprite> idleSprites = new();
        bool usePrefabArt;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 18;
            usePrefabArt = spriteRenderer.sprite != null;
            if (!usePrefabArt)
                LoadSprites();
        }

        void Start()
        {
            player = FindFirstObjectByType<PlayerController>()?.transform;
            if (PatrolWorldPoints == null || PatrolWorldPoints.Length == 0)
                PatrolWorldPoints = new[] { transform.position };
        }

        void LoadSprites()
        {
            foreach (var d in new[] { "down", "up", "left", "right" })
                idleSprites[d] = LoadSprite($"guard_idle_{d}");
            suspiciousLeft = LoadSprite("guard_suspicious_left");
            suspiciousRight = LoadSprite("guard_suspicious_right");
            chaseSprite = LoadSprite("guard_chase");
        }

        static Sprite LoadSprite(string name) =>
            TileTextureFactory.LoadCharacterSprite($"Sprites/Characters/{name}");

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
                return;
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerController>()?.transform;
                if (player == null) return;
            }

            if (hitCooldown > 0f)
                hitCooldown -= Time.deltaTime;

            if (CanDetectPlayer())
            {
                lastKnownPlayerPos = player.position;
                if (state != GuardState.Chase)
                    EnterChase();
                loseSightTimer = 0f;
                giveUpTimer = 0f;
            }

            switch (state)
            {
                case GuardState.Patrol:
                    Patrol();
                    break;
                case GuardState.Suspicious:
                    MoveToward(lastKnownPlayerPos, GameConstants.GuardSuspiciousSpeed);
                    if (Vector3.Distance(transform.position, lastKnownPlayerPos) < 0.35f)
                    {
                        state = GuardState.Search;
                        searchTimer = GameConstants.GuardSearchDuration;
                    }
                    break;
                case GuardState.Search:
                    searchTimer -= Time.deltaTime;
                    if (searchTimer <= 0)
                    {
                        state = GuardState.Patrol;
                        patrolIndex = 0;
                    }
                    break;
                case GuardState.Chase:
                    UpdateChase();
                    break;
            }
        }

        void UpdateChase()
        {
            if (!CanDetectPlayer())
            {
                loseSightTimer += Time.deltaTime;
                giveUpTimer += Time.deltaTime;
                if (giveUpTimer >= GameConstants.GuardGiveUpDelay)
                {
                    state = GuardState.Search;
                    searchTimer = GameConstants.GuardSearchDuration;
                    return;
                }

                MoveToward(lastKnownPlayerPos, GameConstants.GuardSuspiciousSpeed * 0.8f);
                return;
            }

            giveUpTimer = 0f;
            float dist = Vector3.Distance(transform.position, player.position);

            if (dist <= GameConstants.GuardCatchDistance)
            {
                CatchPlayer();
                return;
            }

            var pc = player.GetComponent<PlayerController>();
            bool playerSprinting = pc != null && pc.IsSprinting;
            float speed = GameConstants.GuardChaseSpeed;

            if (playerSprinting && dist > lastChaseDistance + 0.05f)
                speed *= GameConstants.GuardSprintCatchPenalty;
            else if (dist > GameConstants.GuardViewDistance)
                speed *= 0.65f;

            lastChaseDistance = dist;
            MoveToward(player.position, speed, respectPlayerSpace: true);
        }

        void Patrol()
        {
            if (PatrolWorldPoints.Length < 2) return;
            Vector3 target = PatrolWorldPoints[patrolIndex];
            MoveToward(target, GameConstants.GuardPatrolSpeed);
            if (Vector3.Distance(transform.position, target) < 0.25f)
                patrolIndex = (patrolIndex + 1) % PatrolWorldPoints.Length;
        }

        void MoveToward(Vector3 target, float speed, bool respectPlayerSpace = false)
        {
            Vector3 dir = (target - transform.position).normalized;
            Vector3 newPos = transform.position + dir * speed * Time.deltaTime;
            var grid = FortLevelData.WorldToGrid(newPos);

            if (!FortLevelData.IsWalkable(grid.x, grid.y))
                return;

            if (respectPlayerSpace && player != null)
            {
                var playerGrid = FortLevelData.WorldToGrid(player.position);
                if (grid == playerGrid)
                    return;

                float dist = Vector3.Distance(transform.position, player.position);
                if (dist <= GameConstants.GuardMinStandoff && dist > GameConstants.GuardCatchDistance)
                    return;
            }

            transform.position = newPos;
            UpdateFacing(dir);
        }

        void UpdateFacing(Vector3 dir)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                facing = dir.x > 0 ? "right" : "left";
            else
                facing = dir.y > 0 ? "up" : "down";

            if (usePrefabArt)
            {
                spriteRenderer.flipX = facing == "left";
                return;
            }

            if (state != GuardState.Chase && idleSprites.TryGetValue(facing, out var s) && s != null)
                spriteRenderer.sprite = s;
        }

        bool CanDetectPlayer()
        {
            if (player == null) return false;

            var guardGrid = FortLevelData.WorldToGrid(transform.position);
            var playerGrid = FortLevelData.WorldToGrid(player.position);

            if (FortLevelData.IsPlayerHiddenFrom(playerGrid))
                return false;

            if (FortLevelData.GridChebyshevDistance(guardGrid, playerGrid) > GameConstants.GuardDetectionGridRadius)
                return false;

            return FortLevelData.HasGridLineOfSight(guardGrid, playerGrid);
        }

        public void OnHeardNoise(Vector3 noisePos, bool loud = false)
        {
            if (state == GuardState.Chase) return;
            float hearing = GameConstants.GuardHearingDistance * (loud ? 1.5f : 1f);
            if (Vector3.Distance(transform.position, noisePos) <= hearing)
            {
                lastKnownPlayerPos = noisePos;
                state = GuardState.Suspicious;
            }
        }

        void EnterChase()
        {
            state = GuardState.Chase;
            lastChaseDistance = Vector3.Distance(transform.position, player.position);
            SoundManager.Instance?.PlayGuardAlert();
            if (!usePrefabArt && chaseSprite != null)
                spriteRenderer.sprite = chaseSprite;
        }

        void CatchPlayer()
        {
            if (hitCooldown > 0f) return;

            var playerController = player.GetComponent<PlayerController>();
            if (playerController == null || playerController.IsInvulnerable) return;

            playerController.OnGuardHit(transform.position);
            SoundManager.Instance?.PlayHurt();
            hitCooldown = GameConstants.PlayerInvulnerabilitySeconds + 0.75f;

            if (GameManager.Instance != null && GameManager.Instance.Health > 0)
            {
                state = GuardState.Search;
                searchTimer = 2.5f;
                PushBackFromPlayer();
            }
        }

        void PushBackFromPlayer()
        {
            Vector3 away = (transform.position - player.position).normalized;
            if (away.sqrMagnitude < 0.01f) away = Vector3.up;
            var newPos = transform.position + away * 0.9f;
            var grid = FortLevelData.WorldToGrid(newPos);
            if (FortLevelData.IsWalkable(grid.x, grid.y))
                transform.position = newPos;
        }
    }
}
