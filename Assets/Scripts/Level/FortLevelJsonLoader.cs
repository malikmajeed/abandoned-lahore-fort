using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ForgottenFort.Level
{
    [Serializable]
    public class LevelJsonData
    {
        public int tileSize;
        public int gridWidth;
        public int gridHeight;
        public LevelStartJson start;
        public LevelRoomJson[] rooms;
        public LevelCorridorJson[] corridors;
        public LevelPointJson[] mazeBlocks;
        public LevelDoorJson[] doors;
        public LevelKeyJson[] keys;
        public LevelChestJson[] chests;
        public LevelGuardJson[] guards;
        public LevelBarrelJson[] decor_barrels;
        public LevelPuzzleJson puzzle;
        public LevelExitJson[] exits;
    }

    [Serializable] public class LevelStartJson { public int x, y; public string room; }
    [Serializable] public class LevelRoomJson { public string id, name; public int x, y, w, h; public bool guard; }
    [Serializable] public class LevelCorridorJson { public string id; public int[] pathX, pathY; }
    [Serializable] public class LevelPointJson { public int x, y; }
    [Serializable] public class LevelDoorJson { public string id; public int x, y; public string orientation, keyId, leadsFrom, leadsTo; public bool locked; }
    [Serializable] public class LevelKeyJson { public string id, unlocks, room; public int x, y; }
    [Serializable] public class LevelChestJson { public string id, room, loot; public int x, y; public bool requiresPuzzle; }
    [Serializable] public class LevelGuardJson { public string id, room; public int x, y; public Vector2Int[] patrol; }
    [Serializable] public class LevelBarrelJson { public int x, y; public string room; }
    [Serializable] public class LevelPuzzleJson { public string id, room, type, unlocksChest; public int x, y; }
    [Serializable] public class LevelExitJson { public string id, leadsTo; public int x, y; }

    /// <summary>
    /// Builds the playable grid from Assets/Resources/level.json using computed 1-tile walls.
    /// Floor = room interiors + door tiles. Wall = orthogonal neighbors of floor not in floor.
    /// </summary>
    public static class FortLevelJsonLoader
    {
        public static LevelJsonData Data { get; private set; }
        public static char[,] Grid { get; private set; }
        public static bool IsLoaded { get; private set; }
        public static int TileSizePixels { get; private set; } = 64;

        public static readonly HashSet<Vector2Int> FloorTiles = new();
        public static readonly HashSet<Vector2Int> WallTiles = new();
        public static readonly List<LevelGuardJson> Guards = new();
        public static readonly List<string> DoorIdsInOrder = new();
        public static readonly List<LevelDoorJson> DoorsInOrder = new();

        static readonly Vector2Int[] OrthoDirs =
        {
            Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down
        };

        public static void ResetForReload()
        {
            IsLoaded = false;
            Data = null;
            Grid = null;
            FloorTiles.Clear();
            WallTiles.Clear();
            Guards.Clear();
            DoorIdsInOrder.Clear();
            DoorsInOrder.Clear();
        }

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

            TileSizePixels = Data.tileSize > 0 ? Data.tileSize : 64;
            AttachGuardPatrols(asset.text, Data.guards);
            ComputeFloorAndWalls(Data);
            Grid = BuildGrid(Data);
            IsLoaded = true;

            Debug.Log($"FortLevelJsonLoader: loaded {Data.gridWidth}x{Data.gridHeight} " +
                      $"({FloorTiles.Count} floor, {WallTiles.Count} wall, {DoorsInOrder.Count} doors, {Guards.Count} guards).");
        }

        public static LevelDoorJson GetDoor(int doorIndex)
        {
            if (doorIndex >= 0 && doorIndex < DoorsInOrder.Count)
                return DoorsInOrder[doorIndex];
            return null;
        }

        static void AttachGuardPatrols(string json, LevelGuardJson[] guards)
        {
            if (guards == null) return;

            var patrolMatches = Regex.Matches(
                json,
                "\"patrol\"\\s*:\\s*\\[((?:\\s*\\[\\s*\\d+\\s*,\\s*\\d+\\s*\\]\\s*,?\\s*)+)\\]",
                RegexOptions.Singleline);

            int i = 0;
            foreach (Match block in patrolMatches)
            {
                if (i >= guards.Length) break;
                var points = new List<Vector2Int>();
                foreach (Match pt in Regex.Matches(block.Groups[1].Value, "\\[\\s*(\\d+)\\s*,\\s*(\\d+)\\s*\\]"))
                    points.Add(new Vector2Int(int.Parse(pt.Groups[1].Value), int.Parse(pt.Groups[2].Value)));
                guards[i].patrol = points.ToArray();
                i++;
            }
        }

        static void ComputeFloorAndWalls(LevelJsonData data)
        {
            FloorTiles.Clear();
            WallTiles.Clear();

            if (data.rooms != null)
            {
                foreach (var room in data.rooms)
                {
                    for (int y = room.y; y < room.y + room.h; y++)
                    for (int x = room.x; x < room.x + room.w; x++)
                        FloorTiles.Add(new Vector2Int(x, y));
                }
            }

            if (data.doors != null)
            {
                foreach (var door in data.doors)
                    FloorTiles.Add(new Vector2Int(door.x, door.y));
            }

            if (data.corridors != null)
            {
                foreach (var corridor in data.corridors)
                {
                    if (corridor.pathX == null || corridor.pathY == null) continue;
                    for (int i = 0; i < corridor.pathX.Length && i < corridor.pathY.Length; i++)
                        FloorTiles.Add(new Vector2Int(corridor.pathX[i], corridor.pathY[i]));
                }
            }

            if (data.mazeBlocks != null)
            {
                foreach (var block in data.mazeBlocks)
                    FloorTiles.Remove(new Vector2Int(block.x, block.y));
            }

            foreach (var floor in FloorTiles)
            {
                foreach (var dir in OrthoDirs)
                {
                    var neighbor = floor + dir;
                    if (!FloorTiles.Contains(neighbor))
                        WallTiles.Add(neighbor);
                }
            }
        }

        static char[,] BuildGrid(LevelJsonData data)
        {
            int w = data.gridWidth, h = data.gridHeight;
            var g = new char[h, w];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                g[y, x] = ' ';

            foreach (var wall in WallTiles)
                if (InBounds(g, w, h, wall.x, wall.y))
                    g[wall.y, wall.x] = '#';

            foreach (var floor in FloorTiles)
                if (InBounds(g, w, h, floor.x, floor.y))
                    g[floor.y, floor.x] = '.';

            DoorIdsInOrder.Clear();
            DoorsInOrder.Clear();
            if (data.doors != null)
            {
                foreach (var door in data.doors)
                {
                    PlaceChar(g, w, h, door.x, door.y, 'D');
                    DoorIdsInOrder.Add(door.id);
                    DoorsInOrder.Add(door);
                }
            }

            if (data.start != null)
                PlaceChar(g, w, h, data.start.x, data.start.y, 'S');

            if (data.keys != null)
                foreach (var key in data.keys)
                    PlaceChar(g, w, h, key.x, key.y, 'K');

            if (data.decor_barrels != null)
                foreach (var barrel in data.decor_barrels)
                    PlaceChar(g, w, h, barrel.x, barrel.y, 'B');

            if (data.chests != null)
                foreach (var chest in data.chests)
                    PlaceChar(g, w, h, chest.x, chest.y, chest.requiresPuzzle ? 'X' : 'C');

            if (data.puzzle != null)
                PlaceChar(g, w, h, data.puzzle.x, data.puzzle.y, 'M');

            if (data.exits != null)
                foreach (var exit in data.exits)
                    PlaceChar(g, w, h, exit.x, exit.y, 'E');

            Guards.Clear();
            if (data.guards != null)
                Guards.AddRange(data.guards);

            return g;
        }

        static void PlaceChar(char[,] g, int w, int h, int x, int y, char c)
        {
            if (!InBounds(g, w, h, x, y)) return;
            if (g[y, x] == '#') return;
            g[y, x] = c;
        }

        static bool InBounds(char[,] g, int w, int h, int x, int y) =>
            x >= 0 && x < w && y >= 0 && y < h;
    }
}
