using System;
using System.Collections.Generic;
using ForgottenFort.Core;
using UnityEngine;

namespace ForgottenFort.Level
{
    [Serializable]
    public class LevelJsonData
    {
        public int gridWidth;
        public int gridHeight;
        public LevelStartJson start;
        public LevelRoomJson[] rooms;
        public LevelCorridorJson[] corridors;
        public LevelDoorJson[] doors;
        public LevelKeyJson[] keys;
        public LevelChestJson[] chests;
        public LevelGuardJson[] guards;
        public LevelBarrelJson[] decor_barrels;
        public LevelMazeWallJson[] mazeWalls;
        public LevelPuzzleJson puzzle;
    }

    [Serializable] public class LevelStartJson { public int x, y; public string room; }
    [Serializable] public class LevelRoomJson { public string id, name; public int x, y, w, h; }
    [Serializable] public class LevelCorridorJson { public string from, to; public int[] pathX, pathY; }
    [Serializable] public class LevelDoorJson { public string id; public int x, y; public string orientation, keyId, leadsFrom, leadsTo; public bool locked; }
    [Serializable] public class LevelKeyJson { public string id, unlocks, room; public int x, y; }
    [Serializable] public class LevelChestJson { public string id, room, loot; public int x, y; public bool requiresPuzzle; }
    [Serializable] public class LevelGuardJson { public string id, room; public int x, y; public int[] patrolX, patrolY; }
    [Serializable] public class LevelBarrelJson { public int x, y; public string room; }
    [Serializable] public class LevelMazeWallJson { public int x1, y1, x2, y2, gapX; public int gapY; }
    [Serializable] public class LevelPuzzleJson { public string id, room, type, unlocksChest; public int x, y; }

    /// <summary>
    /// Builds the playable grid from Assets/Resources/level.json.
    /// </summary>
    public static class FortLevelJsonLoader
    {
        public static LevelJsonData Data { get; private set; }
        public static char[,] Grid { get; private set; }
        public static bool IsLoaded { get; private set; }

        public static readonly List<LevelGuardJson> Guards = new();
        public static readonly List<string> DoorIdsInOrder = new();

        public static void EnsureLoaded()
        {
            if (IsLoaded) return;
            var asset = Resources.Load<TextAsset>("level");
            if (asset == null)
            {
                Debug.LogWarning("FortLevelJsonLoader: level.json not found — using built-in map.");
                return;
            }

            Data = JsonUtility.FromJson<LevelJsonData>(asset.text);
            if (Data == null || Data.gridWidth <= 0 || Data.gridHeight <= 0)
            {
                Debug.LogError("FortLevelJsonLoader: failed to parse level.json");
                return;
            }

            Grid = BuildGrid(Data);
            IsLoaded = true;
            Debug.Log($"FortLevelJsonLoader: loaded {Data.gridWidth}x{Data.gridHeight} maze " +
                      $"({Data.rooms?.Length ?? 0} rooms, {Data.doors?.Length ?? 0} doors, {Guards.Count} guards).");
        }

        static char[,] BuildGrid(LevelJsonData data)
        {
            int w = data.gridWidth, h = data.gridHeight;
            var g = new char[h, w];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                g[y, x] = '#';

            if (data.rooms != null)
                foreach (var room in data.rooms)
                    CarveRect(g, w, h, room.x, room.y, room.w, room.h);

            if (data.corridors != null)
                foreach (var corridor in data.corridors)
                {
                    if (corridor.pathX == null || corridor.pathY == null) continue;
                    for (int i = 0; i < corridor.pathX.Length && i < corridor.pathY.Length; i++)
                        CarveBrush(g, w, h, corridor.pathX[i], corridor.pathY[i]);
                }

            if (data.mazeWalls != null)
                foreach (var wall in data.mazeWalls)
                    ApplyMazeWall(g, w, h, wall);

            PlaceChar(g, w, h, data.start.x, data.start.y, 'S');

            DoorIdsInOrder.Clear();
            if (data.doors != null)
                foreach (var door in data.doors)
                {
                    PlaceChar(g, w, h, door.x, door.y, 'D');
                    DoorIdsInOrder.Add(door.id);
                }

            if (data.keys != null)
                foreach (var key in data.keys)
                    PlaceChar(g, w, h, key.x, key.y, 'K');

            if (data.decor_barrels != null)
                foreach (var barrel in data.decor_barrels)
                    PlaceChar(g, w, h, barrel.x, barrel.y, 'B');

            if (data.chests != null)
                foreach (var chest in data.chests)
                {
                    char mark = chest.requiresPuzzle || chest.loot == "final_treasure" ? 'X' : 'C';
                    PlaceChar(g, w, h, chest.x, chest.y, mark);
                }

            if (data.puzzle != null)
            {
                PlaceChar(g, w, h, data.puzzle.x, data.puzzle.y, 'M');
                PlaceChar(g, w, h, data.puzzle.x + 1, data.puzzle.y, 'M');
                PlaceChar(g, w, h, data.puzzle.x, data.puzzle.y + 1, 'M');
            }

            Guards.Clear();
            if (data.guards != null)
                Guards.AddRange(data.guards);

            return g;
        }

        static void CarveRect(char[,] g, int w, int h, int x, int y, int rw, int rh)
        {
            for (int dy = 0; dy < rh; dy++)
            for (int dx = 0; dx < rw; dx++)
                SetWalkable(g, w, h, x + dx, y + dy);
        }

        static void CarveBrush(char[,] g, int w, int h, int cx, int cy)
        {
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                SetWalkable(g, w, h, cx + dx, cy + dy);
        }

        static void ApplyMazeWall(char[,] g, int w, int h, LevelMazeWallJson wall)
        {
            int x1 = Mathf.Min(wall.x1, wall.x2);
            int x2 = Mathf.Max(wall.x1, wall.x2);
            int y1 = Mathf.Min(wall.y1, wall.y2);
            int y2 = Mathf.Max(wall.y1, wall.y2);

            for (int y = y1; y <= y2; y++)
            for (int x = x1; x <= x2; x++)
            {
                if (wall.gapY > 0 && x == wall.gapX && y == wall.gapY) continue;
                if (wall.gapY <= 0 && y1 == y2 && x == wall.gapX) continue;
                if (InBounds(g, w, h, x, y) && g[y, x] == '.')
                    g[y, x] = '#';
            }
        }

        static void PlaceChar(char[,] g, int w, int h, int x, int y, char c)
        {
            if (!InBounds(g, w, h, x, y)) return;
            if (g[y, x] == '#') return;
            g[y, x] = c;
        }

        static void SetWalkable(char[,] g, int w, int h, int x, int y)
        {
            if (InBounds(g, w, h, x, y))
                g[y, x] = '.';
        }

        static bool InBounds(char[,] g, int w, int h, int x, int y) =>
            x >= 0 && x < w && y >= 0 && y < h;
    }
}
