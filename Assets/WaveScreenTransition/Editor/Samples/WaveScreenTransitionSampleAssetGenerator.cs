using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Wave.ScreenTransition.Samples.Editor
{
    public static class WaveScreenTransitionSampleAssetGenerator
    {
        private const string SampleRoot = "Assets/WaveScreenTransition/Samples";
        private const string ResourcesRoot = SampleRoot + "/Resources";
        private const string ScenePath = SampleRoot + "/SampleScene.unity";

        [MenuItem("Tools/Wave Screen Transition/Create Sample Assets")]
        public static void CreateSampleAssets()
        {
            EnsureFolder(ResourcesRoot);
            CreatePrefab<SamplePage>(ResourcesRoot + "/SamplePage.prefab");
            CreatePrefab<SampleModal>(ResourcesRoot + "/SampleModal.prefab");
            CreateScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created Wave Screen Transition sample assets.");
        }

        private static void CreatePrefab<TComponent>(string path) where TComponent : Component
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return;

            var gameObject = new GameObject(Path.GetFileNameWithoutExtension(path), typeof(RectTransform));
            gameObject.AddComponent<TComponent>();
            PrefabUtility.SaveAsPrefabAsset(gameObject, path);
            Object.DestroyImmediate(gameObject);
        }

        private static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var rootObject = new GameObject("ScreenNavigatorRoot");
            rootObject.transform.SetParent(canvasObject.transform, false);
            var root = rootObject.AddComponent<ScreenNavigatorRoot>();

            var pageContainer = CreateContainer<PageContainer>("PageContainer", rootObject.transform);
            var modalContainer = CreateContainer<ModalContainer>("ModalContainer", rootObject.transform);
            var noReturnModalContainer = CreateContainer<ModalContainer>("NoReturnModalContainer", rootObject.transform);
            root.Configure(pageContainer, modalContainer, noReturnModalContainer);

            var entryObject = new GameObject("MainEntry");
            entryObject.transform.SetParent(canvasObject.transform, false);
            var entry = entryObject.AddComponent<MainEntry>();
            var entrySerializedObject = new SerializedObject(entry);
            entrySerializedObject.FindProperty("_screenNavigatorRoot").objectReferenceValue = root;
            entrySerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static TContainer CreateContainer<TContainer>(string name, Transform parent)
            where TContainer : Component
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);

            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            return gameObject.AddComponent<TContainer>();
        }

        private static void EnsureFolder(string assetPath)
        {
            var absolutePath = Path.GetFullPath(assetPath);
            Directory.CreateDirectory(absolutePath);
        }
    }
}
