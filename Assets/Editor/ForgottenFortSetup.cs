#if UNITY_EDITOR
using ForgottenFort.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ForgottenFort.Editor
{
    public static class ForgottenFortSetup
    {
        const string LibraryPath = "Assets/Resources/LevelPrefabLibrary.asset";
        const string GameplayScene = "Assets/Scenes/Gameplay.unity";

        [MenuItem("Forgotten Fort/Setup Project")]
        public static void SetupProject()
        {
            CreateScenes();
            ConfigureBuildSettings();
            ConfigureTextureImports();
            CreatePrefabLibrary();
            Debug.Log("The Forgotten Fort: Project setup complete!");
        }

        [MenuItem("Forgotten Fort/Wire My Prefabs To Gameplay")]
        public static void WireGameplayScene()
        {
            CreatePrefabLibrary();
            EditorSceneManager.OpenScene(GameplayScene);

            var level = Object.FindFirstObjectByType<LevelGenerator>();
            if (level == null)
            {
                var go = new GameObject("Level");
                level = go.AddComponent<LevelGenerator>();
            }

            var lib = AssetDatabase.LoadAssetAtPath<LevelPrefabLibrary>(LibraryPath);
            if (lib != null)
            {
                level.floorPrefab = lib.floorPrefab;
                level.wallPrefab = lib.wallPrefab;
                level.playerPrefab = lib.playerPrefab;
                level.guardPrefab = lib.guardPrefab;
                level.treasurePrefab = lib.treasurePrefab;
                level.keyPrefab = lib.keyPrefab;
                level.mosaicPrefab = lib.mosaicPrefab;
                level.doorPrefab = lib.doorPrefab;
                level.doorOpenPrefab = lib.doorOpenPrefab;
                level.torchPrefab = lib.torchPrefab;
                level.barrelPrefab = lib.barrelPrefab;
                EditorUtility.SetDirty(level);
            }

            EditorSceneManager.MarkSceneDirty(level.gameObject.scene);
            EditorSceneManager.SaveScene(level.gameObject.scene);
            Debug.Log("Gameplay scene saved with your prefabs on LevelGenerator. Press Play from Main Menu.");
        }

        [MenuItem("Forgotten Fort/Play From Main Menu")]
        public static void PlayFromMainMenu()
        {
            SetupProject();
            WireGameplayScene();
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Forgotten Fort/Create Prefab Library")]
        public static void CreatePrefabLibrary()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            var lib = AssetDatabase.LoadAssetAtPath<LevelPrefabLibrary>(LibraryPath);
            if (lib == null)
            {
                lib = ScriptableObject.CreateInstance<LevelPrefabLibrary>();
                AssetDatabase.CreateAsset(lib, LibraryPath);
            }

            lib.floorPrefab = LoadPrefab("Assets/Prefabs/ground.prefab");
            lib.wallPrefab = LoadPrefab("Assets/Prefabs/wall.prefab");
            lib.playerPrefab = LoadPrefab("Assets/Prefabs/hero.prefab");
            lib.guardPrefab = LoadPrefab("Assets/Prefabs/guard_01.prefab");
            lib.treasurePrefab = LoadPrefab("Assets/Prefabs/treasure.prefab");
            lib.keyPrefab = LoadPrefab("Assets/Prefabs/key prefab_0.prefab");
            lib.mosaicPrefab = LoadPrefab("Assets/Prefabs/mosaic pattern.prefab");
            lib.doorPrefab = LoadPrefab("Assets/Prefabs/door.prefab");
            lib.doorOpenPrefab = LoadPrefab("Assets/Prefabs/door_open.prefab");
            lib.torchPrefab = LoadPrefab("Assets/Prefabs/torch.prefab");
            lib.barrelPrefab = LoadPrefab("Assets/Prefabs/barrel.prefab");

            EditorUtility.SetDirty(lib);
            AssetDatabase.SaveAssets();
            Debug.Log("LevelPrefabLibrary updated at " + LibraryPath);
        }

        static GameObject LoadPrefab(string path) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(path);

        static void CreateScenes()
        {
            CreateEmptyScene("Assets/Scenes/MainMenu.unity");
            CreateEmptyScene(GameplayScene);
            CreateEmptyScene("Assets/Scenes/WinScreen.unity");
            CreateEmptyScene("Assets/Scenes/LoseScreen.unity");
        }

        static void CreateEmptyScene(string path)
        {
            if (System.IO.File.Exists(path)) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
        }

        static void ConfigureBuildSettings()
        {
            var scenes = new[]
            {
                "Assets/Scenes/MainMenu.unity",
                GameplayScene,
                "Assets/Scenes/WinScreen.unity",
                "Assets/Scenes/LoseScreen.unity",
            };
            var list = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            foreach (var s in scenes)
            {
                if (System.IO.File.Exists(s))
                    list.Add(new EditorBuildSettingsScene(s, true));
            }
            EditorBuildSettings.scenes = list.ToArray();
        }

        static void ConfigureTextureImports()
        {
            string[] folders = { "Assets/Sprites", "Assets/Resources/Sprites", "Assets/Prefabs" };
            foreach (var folder in folders)
            {
                if (!System.IO.Directory.Exists(folder)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) continue;
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 32;
                    importer.filterMode = FilterMode.Point;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
#endif
