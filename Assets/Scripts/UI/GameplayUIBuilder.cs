using ForgottenFort.Core;
using ForgottenFort.Level;
using ForgottenFort.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ForgottenFort.Core
{
    public static class GameplayUIBuilder
    {
        public static GameplayHUD Build(Transform canvasRoot)
        {
            var canvasGo = canvasRoot.gameObject;

            // Top HUD bar
            var topBar = MakePanel(canvasGo.transform, "TopBar",
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                new Vector2(0, 0), new Vector2(0, 72),
                new Color(0.05f, 0.04f, 0.035f, 0.9f));

            var hudGo = new GameObject("HUD");
            hudGo.transform.SetParent(topBar, false);
            var hudRect = hudGo.AddComponent<RectTransform>();
            Stretch(hudRect);
            var hud = hudGo.AddComponent<GameplayHUD>();

            // Hearts + health
            hud.heartImages = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                hud.heartImages[i] = MakeImage(hudGo.transform, $"Heart{i}", new Vector2(20 + i * 34, -16),
                    new Vector2(28, 28), LoadUiSprite("heart_full"),
                    new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));
            }

            hud.healthText = MakeText(hudGo.transform, "Health", new Vector2(200, -32), "100/100", 22, Color.white,
                TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(90, 36));

            // Keys — icon + counter bubble
            hud.keyIcon = MakeImage(hudGo.transform, "KeyIcon", new Vector2(292, -14), new Vector2(26, 26),
                LoadUiSprite("icon_key"), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1));

            var keyBubble = MakeImage(hudGo.transform, "KeyBubble", new Vector2(322, -14), new Vector2(28, 28),
                null, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f));
            keyBubble.color = new Color(0.75f, 0.15f, 0.12f, 0.95f);

            hud.keyBubbleText = MakeText(hudGo.transform, "KeyCount", new Vector2(322, -14), "0", 20, Color.white,
                TextAnchor.MiddleCenter, new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector2(28, 28));

            hud.keyText = MakeText(hudGo.transform, "KeysLabel", new Vector2(356, -32), "KEYS", 18,
                new Color(1f, 0.92f, 0.55f), TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(60, 36));

            hud.mosaicText = MakeText(hudGo.transform, "Mosaic", new Vector2(430, -32), "MOSAIC 0/3", 22,
                new Color(0.7f, 0.85f, 1f), TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(80, 40));

            hud.timerText = MakeText(hudGo.transform, "Timer", new Vector2(-20, -32), "TIME 10:00", 26, Color.white,
                TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(240, 40));

            // Minimap
            var minimap = MakePanel(canvasGo.transform, "Minimap",
                new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-12, 12), new Vector2(200, 230),
                new Color(0.82f, 0.72f, 0.52f, 0.92f));

            var mapImg = new GameObject("MapImage");
            mapImg.transform.SetParent(minimap, false);
            var mapRect = mapImg.AddComponent<RectTransform>();
            Stretch(mapRect, 6);
            var raw = mapImg.AddComponent<RawImage>();
            raw.texture = Resources.Load<Texture2D>("Sprites/Map/fort_map");
            raw.color = new Color(1, 1, 1, 0.88f);

            var viewport = MakeImage(minimap, "Viewport", Vector2.zero, new Vector2(50, 38),
                null, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            viewport.color = new Color(1, 1, 1, 0.12f);
            viewport.enabled = false;

            var dot = MakeImage(minimap, "PlayerDot", Vector2.zero, new Vector2(10, 10),
                null, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            dot.color = new Color(0.9f, 0.1f, 0.1f);

            var mm = minimap.gameObject.AddComponent<MinimapController>();
            mm.minimapPanel = minimap.GetComponent<RectTransform>();
            mm.mapRawImage = raw;
            mm.playerDot = dot;
            mm.viewportRect = viewport;

            // Pause overlay
            var pause = MakePanel(canvasGo.transform, "PausePanel",
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.72f));
            Stretch(pause.GetComponent<RectTransform>());
            MakeText(pause, "PauseLabel", Vector2.zero, "PAUSED\nPress P or ESC", 44, Color.white,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, 200));
            pause.gameObject.SetActive(false);
            hud.pausePanel = pause.gameObject;

            return hud;
        }

        static RectTransform MakePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return rt;
        }

        static Image MakeImage(Transform parent, string name, Vector2 pos, Vector2 size, Sprite sprite,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        static Text MakeText(Transform parent, string name, Vector2 pos, string content, int size, Color color,
            TextAnchor align, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = align;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.raycastTarget = false;
            return txt;
        }

        static void Stretch(RectTransform rt, float margin = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        static Sprite LoadUiSprite(string name)
        {
            var tex = Resources.Load<Texture2D>($"Sprites/UI/{name}");
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
