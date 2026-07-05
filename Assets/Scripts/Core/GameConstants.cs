namespace ForgottenFort.Core
{
    public static class GameConstants
    {
        public const float TileSize = 1f;
        public const int ViewportWidthTiles = 18;
        public const int ViewportHeightTiles = 10;
        public const int TotalKeysRequired = 3;
        public const int MosaicFragmentsRequired = 3;
        public const int MaxHealth = 100;
        public const int GuardHitDamage = 15;
        public const float PlayerInvulnerabilitySeconds = 1.25f;
        public const float GameTimeLimitSeconds = 600f;
        public const float GuardViewDistance = 6f;
        public const float GuardHearingDistance = 3.5f;
        public const int GuardDetectionGridRadius = 4;
        public const float GuardMinStandoff = 0.72f;
        public const float GuardCatchDistance = 0.55f;
        public const float GuardGiveUpDelay = 1.1f;
        public const float GuardSprintCatchPenalty = 0.45f;
        public const float GuardSearchDuration = 3f;
        public const float GuardSuspiciousSpeed = 2.5f;
        public const float GuardPatrolSpeed = 1.5f;
        public const float GuardChaseSpeed = 3.5f;
        public const float PlayerMoveSpeed = 7.5f;
        public const float PlayerSprintMultiplier = 1.45f;
        public const float SprintNoiseInterval = 0.22f;
        public const float WalkNoiseInterval = 0.4f;
        public const float TorchDetectionBonus = 1.35f;

        public static readonly string[] KeyNames =
        {
            "Roshanai Key",
            "Akbari Key",
            "Hazuri Key"
        };
    }
}
