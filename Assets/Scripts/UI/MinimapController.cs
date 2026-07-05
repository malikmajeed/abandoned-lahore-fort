using ForgottenFort.Core;
using ForgottenFort.Level;
using ForgottenFort.Player;
using UnityEngine;
using UnityEngine.UI;

namespace ForgottenFort.UI
{
    /// <summary>
    /// Parchment-style minimap showing full fort layout with player position dot.
    /// Only reveals explored areas; viewport rect shows current camera view.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        public RectTransform minimapPanel;
        public Image mapImage;
        public Image playerDot;
        public Image viewportRect;
        public RawImage mapRawImage;

        Texture2D exploredMask;
        bool[,] explored;
        Transform player;
        Camera mainCam;

        void Start()
        {
            player = FindFirstObjectByType<PlayerController>()?.transform;
            mainCam = Camera.main;
            explored = new bool[FortLevelData.Width, FortLevelData.Height];

            var mapTex = Resources.Load<Texture2D>("Sprites/Map/fort_map");
            if (mapTex != null && mapRawImage != null)
            {
                mapRawImage.texture = mapTex;
            }
            else if (mapImage != null)
            {
                mapImage.color = new Color(0.85f, 0.75f, 0.55f);
            }

            if (playerDot != null)
            {
                playerDot.color = new Color(0.9f, 0.15f, 0.1f);
            }
            if (viewportRect != null)
            {
                viewportRect.color = new Color(1f, 1f, 1f, 0.15f);
            }
        }

        void Update()
        {
            if (player == null || minimapPanel == null) return;
            UpdatePlayerDot();
            UpdateViewportRect();
            RevealAroundPlayer();
        }

        void UpdatePlayerDot()
        {
            var grid = FortLevelData.WorldToGrid(player.position);
            float nx = (float)grid.x / FortLevelData.Width;
            float ny = 1f - (float)grid.y / FortLevelData.Height;

            if (playerDot != null)
            {
                var panel = minimapPanel.rect;
                playerDot.rectTransform.anchoredPosition = new Vector2(
                    (nx - 0.5f) * panel.width,
                    (ny - 0.5f) * panel.height);
            }
        }

        void UpdateViewportRect()
        {
            if (viewportRect == null || mainCam == null) return;
            float camH = mainCam.orthographicSize * 2f;
            float camW = camH * mainCam.aspect;
            float mapW = FortLevelData.Width * GameConstants.TileSize;
            float mapH = FortLevelData.Height * GameConstants.TileSize;

            var panel = minimapPanel.rect;
            viewportRect.rectTransform.sizeDelta = new Vector2(
                panel.width * (camW / mapW),
                panel.height * (camH / mapH));

            var grid = FortLevelData.WorldToGrid(mainCam.transform.position);
            float nx = (float)grid.x / FortLevelData.Width;
            float ny = 1f - (float)grid.y / FortLevelData.Height;
            viewportRect.rectTransform.anchoredPosition = new Vector2(
                (nx - 0.5f) * panel.width,
                (ny - 0.5f) * panel.height);
        }

        void RevealAroundPlayer()
        {
            var grid = FortLevelData.WorldToGrid(player.position);
            int radius = 4;
            for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++)
            {
                int x = grid.x + dx, y = grid.y + dy;
                if (x >= 0 && x < FortLevelData.Width && y >= 0 && y < FortLevelData.Height)
                    explored[x, y] = true;
            }
        }
    }
}
