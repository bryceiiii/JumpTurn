using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JumpTurn.Editor
{
    public static class JumpTurnDemoSetup
    {
        private const string ScenePath = "Assets/JumpTurn/Scenes/JumpTurnDemo.unity";

        [MenuItem("Tools/JumpTurn/Rebuild Demo Scene")]
        public static void RebuildDemoScene()
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            ConfigureTextureImports();
            ConfigureFontImport();
            ConfigurePlayerSettings();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 11.25f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.035f, 0.055f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();

            var bootstrapObject = new GameObject("JumpTurnBootstrap");
            var bootstrap = bootstrapObject.AddComponent<JumpTurnBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("worldCamera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/JumpTurn/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[JumpTurn] Demo scene rebuilt at " + ScenePath);
        }

        private static void ConfigureTextureImports()
        {
            string[] folders = { "Assets/JumpTurn/Resources/Kenney1Bit", "Assets/JumpTurn/Resources/Ghost" };
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", folders);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = path.EndsWith("/Tilemap.png") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "JumpTurn";
            PlayerSettings.productName = "JumpTurn";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            QualitySettings.antiAliasing = 0;
        }

        private static void ConfigureFontImport()
        {
            const string fontPath = "Assets/JumpTurn/Resources/UI/ZCOOLQingKeHuangYou-Regular.ttf";
            if (AssetImporter.GetAtPath(fontPath) is TrueTypeFontImporter importer)
            {
                importer.includeFontData = true;
                importer.fontTextureCase = FontTextureCase.Dynamic;
                importer.fontSize = 16;
                importer.SaveAndReimport();
            }
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(scene => scene.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
