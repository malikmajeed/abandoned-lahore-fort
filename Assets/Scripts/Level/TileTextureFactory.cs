using UnityEngine;

namespace ForgottenFort.Level
{
    /// <summary>
    /// Procedural pixel-art floor and wall tiles matching the dungeon reference style.
    /// </summary>
    public static class TileTextureFactory
    {
        const int Size = 32;
        const float Ppu = 32f;

        static Sprite _floorA, _floorB, _wall, _wallTop, _torchGlow;

        public static Sprite Floor(int x, int y) => ((x + y) & 1) == 0 ? FloorA() : FloorB();
        public static Sprite WallBlock() => _wall ??= BuildWall();
        public static Sprite WallTopFace() => _wallTop ??= BuildWallTop();
        public static Sprite TorchGlow() => _torchGlow ??= BuildGlow(new Color(1f, 0.55f, 0.15f, 0.35f));

        static Sprite FloorA()
        {
            if (_floorA != null) return _floorA;
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int py = 0; py < Size; py++)
            for (int px = 0; px < Size; px++)
            {
                bool mortar = px % 8 == 0 || py % 8 == 0;
                var c = mortar ? new Color(0.18f, 0.17f, 0.16f) : new Color(0.32f, 0.31f, 0.30f);
                if ((px + py * 3) % 11 == 0) c *= 0.92f;
                tex.SetPixel(px, py, c);
            }
            tex.Apply();
            _floorA = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Ppu);
            return _floorA;
        }

        static Sprite FloorB()
        {
            if (_floorB != null) return _floorB;
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int py = 0; py < Size; py++)
            for (int px = 0; px < Size; px++)
            {
                bool mortar = (px + 4) % 8 == 0 || (py + 4) % 8 == 0;
                var c = mortar ? new Color(0.17f, 0.16f, 0.15f) : new Color(0.30f, 0.29f, 0.28f);
                if ((px * 2 + py) % 13 == 0) c *= 0.9f;
                tex.SetPixel(px, py, c);
            }
            tex.Apply();
            _floorB = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Ppu);
            return _floorB;
        }

        static Sprite BuildWall()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int py = 0; py < Size; py++)
            for (int px = 0; px < Size; px++)
            {
                int brickW = 8, brickH = 6;
                int row = py / brickH;
                int offset = (row & 1) * 4;
                int bx = (px + offset) % brickW;
                int by = py % brickH;
                bool edge = bx == 0 || by == 0 || bx == brickW - 1 || by == brickH - 1;
                var c = edge ? new Color(0.12f, 0.10f, 0.09f) : new Color(0.22f, 0.19f, 0.17f);
                tex.SetPixel(px, py, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Ppu);
        }

        static Sprite BuildWallTop()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            for (int py = 0; py < Size; py++)
            for (int px = 0; px < Size; px++)
            {
                var c = new Color(0.38f, 0.36f, 0.34f);
                if (px % 6 == 0 || py % 6 == 0) c *= 0.85f;
                tex.SetPixel(px, py, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Ppu);
        }

        static Sprite BuildGlow(Color color)
        {
            int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float cx = s * 0.5f, cy = s * 0.5f;
            for (int py = 0; py < s; py++)
            for (int px = 0; px < s; px++)
            {
                float d = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy)) / (s * 0.5f);
                float a = Mathf.Clamp01(1f - d);
                a *= a;
                tex.SetPixel(px, py, new Color(color.r, color.g, color.b, color.a * a));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 32f);
        }

        public static Sprite LoadCharacterSprite(string resourcePath)
        {
            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.25f), 48f);
        }

        public static Sprite LoadObjectSprite(string resourcePath)
        {
            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
