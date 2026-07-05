using ForgottenFort.Core;
using ForgottenFort.Level;
using ForgottenFort.Player;
using ForgottenFort.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ForgottenFort.Core
{
    /// <summary>
    /// Runtime scene bootstrapper — builds gameplay world and UI when scenes load.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        static bool subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RegisterSceneHook()
        {
            if (subscribed) return;
            subscribed = true;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            BootstrapActiveScene();
        }

        static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BootstrapScene(scene.name);
        }

        static void BootstrapActiveScene()
        {
            BootstrapScene(SceneManager.GetActiveScene().name);
        }

        static void BootstrapScene(string sceneName)
        {
            var go = new GameObject("Bootstrapper");
            var boot = go.AddComponent<GameBootstrapper>();
            boot.SetupScene(sceneName);
            Destroy(go);
        }

        void SetupScene(string sceneName)
        {
            if (sceneName == "SampleScene") sceneName = "MainMenu";
            switch (sceneName)
            {
                case "Gameplay":
                    BuildGameplay();
                    break;
                case "MainMenu":
                    BuildMainMenu();
                    break;
                case "WinScreen":
                    BuildEndScreen(true);
                    break;
                case "LoseScreen":
                    BuildEndScreen(false);
                    break;
            }
        }

        void BuildGameplay()
        {
            EnsureEventSystem();

            if (FindFirstObjectByType<GameManager>() == null)
            {
                var gmGo = new GameObject("GameManager");
                gmGo.AddComponent<GameManager>();
                gmGo.AddComponent<AudioSource>();
                gmGo.AddComponent<SoundManager>();
            }

            if (FindFirstObjectByType<LevelGenerator>() == null)
            {
                var levelGo = new GameObject("Level");
                levelGo.AddComponent<LevelGenerator>();
            }

            if (FindFirstObjectByType<GameplayHUD>() == null)
            {
                var canvasGo = new GameObject("Canvas");
                canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                canvasGo.AddComponent<GraphicRaycaster>();
                GameplayUIBuilder.Build(canvasGo.transform);
            }

            Camera cam;
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<CameraController>();
            }
            else
            {
                cam = Camera.main;
                if (cam.GetComponent<CameraController>() == null)
                    cam.gameObject.AddComponent<CameraController>();
            }
            cam.orthographic = true;
            cam.orthographicSize = GameConstants.ViewportHeightTiles * GameConstants.TileSize * 0.5f;
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.04f);

            DimSceneLighting();
            Physics2D.queriesHitTriggers = true;

            var start = FortLevelData.StartPosition;
            cam.transform.position = FortLevelData.GridToWorld(start.x, start.y) + Vector3.back * 10f;
        }

        static void DimSceneLighting()
        {
            foreach (var light in FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None))
                light.intensity *= 0.25f;
        }

        void BuildMainMenu()
        {
            if (FindFirstObjectByType<MainMenuUI>() != null) return;
            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var menuScaler = canvasGo.AddComponent<CanvasScaler>();
            menuScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            menuScaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var bg = canvasGo.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.12f, 0.1f);
            bg.raycastTarget = false;

            var menu = canvasGo.AddComponent<MainMenuUI>();

            CreateUIText(canvasGo.transform, "Title", new Vector2(0, 220), "THE FORGOTTEN FORT", 64, TextAnchor.MiddleCenter);
            CreateUIText(canvasGo.transform, "Subtitle", new Vector2(0, 140), "A 2D Maze Adventure", 28, TextAnchor.MiddleCenter);

            menu.playButton = CreateButton(canvasGo.transform, "Play", new Vector2(0, 20), "PLAY", null);
            menu.instructionsButton = CreateButton(canvasGo.transform, "Instructions", new Vector2(0, -70), "INSTRUCTIONS", null);
            menu.exitButton = CreateButton(canvasGo.transform, "Exit", new Vector2(0, -160), "EXIT", null);

            var instrGo = new GameObject("InstructionsPanel");
            instrGo.transform.SetParent(canvasGo.transform, false);
            StretchFull(instrGo.AddComponent<RectTransform>());
            var instrBg = instrGo.AddComponent<Image>();
            instrBg.color = new Color(0.05f, 0.04f, 0.03f, 0.95f);
            instrBg.raycastTarget = true;
            string instr = "HOW TO PLAY\n\nARROW KEYS / WASD — Move\nHOLD SHIFT — Sprint (faster, louder)\n\nCollect 3 Keys to open gates\nFind 3 Mosaic Fragments for the vault\nHide behind BARRELS from guards\nAvoid lit TORCHES — guards spot you easier\nReach the Royal Seal in the vault to win!";
            var instrText = CreateUIText(instrGo.transform, "Text", new Vector2(0, 40), instr, 24, TextAnchor.MiddleCenter);
            instrText.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 420);
            CreateButton(instrGo.transform, "Close", new Vector2(0, -260), "BACK", () => instrGo.SetActive(false));
            instrGo.SetActive(false);
            menu.instructionsPanel = instrGo;
        }

        void BuildEndScreen(bool win)
        {
            if (FindFirstObjectByType<EndScreenUI>() != null) return;
            EnsureEventSystem();

            var canvasGo = new GameObject("Canvas");
            canvasGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.AddComponent<Image>().color = win ? new Color(0.1f, 0.08f, 0.05f) : new Color(0.2f, 0.05f, 0.05f);

            var end = canvasGo.AddComponent<EndScreenUI>();
            end.isWinScreen = win;
            end.messageText = CreateUIText(canvasGo.transform, "Message", new Vector2(0, 80), "", 36, TextAnchor.MiddleCenter);
            end.mainMenuButton = CreateButton(canvasGo.transform, "MainMenu", new Vector2(0, -120), "MAIN MENU", () => GameManager.LoadMainMenu());
        }

        static void EnsureEventSystem()
        {
            var existing = FindFirstObjectByType<EventSystem>();
            if (existing != null)
            {
#if ENABLE_INPUT_SYSTEM
                if (existing.GetComponent<InputSystemUIInputModule>() == null)
                {
                    var old = existing.GetComponent<StandaloneInputModule>();
                    if (old != null) Object.Destroy(old);
                    existing.gameObject.AddComponent<InputSystemUIInputModule>();
                }
#endif
                return;
            }
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        static Image CreateUIImage(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go.AddComponent<Image>();
        }

        static Text CreateUIText(Transform parent, string name, Vector2 pos, string content, int size, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(800, 200);
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = size;
            txt.alignment = anchor;
            txt.color = new Color(0.9f, 0.85f, 0.7f);
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.raycastTarget = false;
            return txt;
        }

        static Button CreateButton(Transform parent, string name, Vector2 pos, string label, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280, 60);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.45f, 0.38f, 0.28f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(onClick);
            CreateUIText(go.transform, "Label", Vector2.zero, label, 28, TextAnchor.MiddleCenter);
            return btn;
        }

        static void StretchFull(RectTransform rt, float margin = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }
    }
}
