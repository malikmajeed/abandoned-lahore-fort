using System.Collections.Generic;
using ForgottenFort.Core;
using UnityEngine;

namespace ForgottenFort.Level
{
    /// <summary>
    /// Runtime fort map — loaded from level.json when available, otherwise uses embedded fallback rows.
    /// </summary>
    public static class FortLevelData
    {
        static readonly string[] FallbackMapRows =
        {
            "########################################################################################",
            "#............................#.............................#...........................#",
            "#.T.S........................#.............................#...........................#",
            "#............................#........#################....#....###################....#",
            "#.......K....................#........#...............#....#....#.................#....#",
            "#............................#........#...............#....#....#.................#....#",
            "#..........BBBB..............#........#...........G...#....#....#...........G.....#....#",
            "#..........BBBB..............#........#...............#....#....#.................#....#",
            "#............................#........D.......K.......#....#....D.......K.........#....#",
            "#.................G..........#........#...............#....#....#.................#....#",
            "#............................#........#...............#....#....#.................#....#",
            "#............................#........#...............#....#....#.................#....#",
            "#............................D........#...............#....D....#.................#....#",
            "#............................#........#################....#....###################....#",
            "#............................#.............................#...........................#",
            "#............................#.............................#...........................#",
            "#............................#....###################......#...........................#",
            "#............................#....#...M.M...........#......#...........................#",
            "#............................#....#....M....T.......#......#...........................#",
            "#............................#....#...M.M...........#......#...........................#",
            "#............................#....#.................#......#...........................#",
            "#............................#....###################......#...........................#",
            "#............................#.............................#...........................#",
            "#............................#.............................#...........................#",
            "###############D#############################D#############################D############",
            "#............................#.............................#...........................#",
            "#............................#.............................#...........................#",
            "#............................#.............................#...........................#",
            "#.........B..................#.............................#....#####################..#",
            "#..........B.................#.............................#....#...................#..#",
            "#.........B...G..............#.............................#....#...................#..#",
            "#............................#.............................#....#...................#..#",
            "#............................#..........BBBB...............#....#...................#..#",
            "#............................#.............................#....#...................#..#",
            "#............................#..................G..........#....#...................#..#",
            "#............................#.............................#....D.........X.........#..#",
            "#............................#.............................#....#...................#..#",
            "#............................#.............................#....#.............G.....#..#",
            "#............................#.............................#....#...................#..#",
            "#............................#.............................#....#...................#..#",
            "#............................#.............................#....#...................#..#",
            "#............................#.............................#....#...................#..#",
            "#............................#.............................#....#####################..#",
            "#............................#.............................#...........................#",
            "#............................#.............................#...........................#",
            "########################################################################################",
        };

        public static readonly Dictionary<char, Color> TileColors = new()
        {
            ['#'] = new Color(0.25f, 0.2f, 0.18f),
            ['.'] = new Color(0.45f, 0.42f, 0.38f),
            ['M'] = new Color(0.3f, 0.45f, 0.55f),
            ['S'] = new Color(0.5f, 0.45f, 0.35f),
        };

        static bool UsingJson => FortLevelJsonLoader.IsLoaded && FortLevelJsonLoader.Grid != null;

        public static int Width => UsingJson ? FortLevelJsonLoader.Data.gridWidth : FallbackMapRows[0].Length;
        public static int Height => UsingJson ? FortLevelJsonLoader.Data.gridHeight : FallbackMapRows.Length;

        public static readonly string[] DoorIdsInMapOrder =
        {
            "roshanai_inner", "akbari_inner", "roshanai_gate", "akbari_gate",
            "south_west", "south_mid", "south_east", "hazuri_vault",
        };

        public static Vector2Int StartPosition
        {
            get
            {
                if (UsingJson && FortLevelJsonLoader.Data.start != null)
                    return new Vector2Int(FortLevelJsonLoader.Data.start.x, FortLevelJsonLoader.Data.start.y);

                for (int y = 0; y < FallbackMapRows.Length; y++)
                for (int x = 0; x < FallbackMapRows[y].Length; x++)
                    if (FallbackMapRows[y][x] == 'S')
                        return new Vector2Int(x, y);
                return new Vector2Int(4, 2);
            }
        }

        public static char GetTile(int x, int y)
        {
            if (UsingJson)
            {
                if (y < 0 || y >= Height || x < 0 || x >= Width) return ' ';
                return FortLevelJsonLoader.Grid[y, x];
            }
            if (y < 0 || y >= FallbackMapRows.Length || x < 0 || x >= FallbackMapRows[y].Length)
                return '#';
            return FallbackMapRows[y][x];
        }

        public static bool IsWall(int x, int y) => GetTile(x, y) == '#';

        public static bool IsVoid(int x, int y) => GetTile(x, y) == ' ';

        public static bool IsWalkable(int x, int y)
        {
            char t = GetTile(x, y);
            return t != '#' && t != 'B' && t != ' ';
        }

        public static bool BlocksMovement(int x, int y)
        {
            char t = GetTile(x, y);
            return t == '#' || t == 'B' || t == ' ';
        }

        public static bool IsBarrelTile(int x, int y) => GetTile(x, y) == 'B';
        public static bool IsTorchTile(int x, int y) => GetTile(x, y) == 'T';
        public static bool IsDoorTile(int x, int y) => GetTile(x, y) == 'D';

        public static bool BlocksLineOfSight(int x, int y)
        {
            char t = GetTile(x, y);
            return t == '#' || t == 'B' || t == 'D';
        }

        public static int GridChebyshevDistance(Vector2Int a, Vector2Int b) =>
            Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        public static bool HasGridLineOfSight(Vector2Int from, Vector2Int to)
        {
            int x0 = from.x, y0 = from.y, x1 = to.x, y1 = to.y;
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                if (x0 == x1 && y0 == y1) return true;
                if (!(x0 == from.x && y0 == from.y) && BlocksLineOfSight(x0, y0))
                    return false;
                int e2 = err * 2;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        public static bool HasLineOfSight(Vector3 from, Vector3 to) =>
            HasGridLineOfSight(WorldToGrid(from), WorldToGrid(to));

        public static bool IsPlayerHiddenFrom(Vector2Int playerGrid)
        {
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                if (IsBarrelTile(playerGrid.x + dx, playerGrid.y + dy))
                    return true;
            }
            return false;
        }

        public static Vector3 GridToWorld(int x, int y)
        {
            float ox = -Width * GameConstants.TileSize * 0.5f;
            float oy = Height * GameConstants.TileSize * 0.5f;
            return new Vector3(
                ox + x * GameConstants.TileSize + GameConstants.TileSize * 0.5f,
                oy - y * GameConstants.TileSize - GameConstants.TileSize * 0.5f, 0);
        }

        public static Vector2Int WorldToGrid(Vector3 world)
        {
            float ox = -Width * GameConstants.TileSize * 0.5f;
            float oy = Height * GameConstants.TileSize * 0.5f;
            int x = Mathf.FloorToInt((world.x - ox) / GameConstants.TileSize);
            int y = Mathf.FloorToInt((oy - world.y) / GameConstants.TileSize);
            return new Vector2Int(x, y);
        }

        public static string GetDoorId(int doorIndex)
        {
            if (UsingJson && doorIndex >= 0 && doorIndex < FortLevelJsonLoader.DoorIdsInOrder.Count)
                return FortLevelJsonLoader.DoorIdsInOrder[doorIndex];
            return DoorIdsInMapOrder[doorIndex % DoorIdsInMapOrder.Length];
        }
    }
}
