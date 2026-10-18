using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace ClimbGames.Editor.UI
{
    public static class CreatePrefabMenu
    {
        private static readonly string ImagePrefabGUID = "d11bdc2091baf354da7b73261fb8ecdf";
        private static readonly string TextPrefabGUID = "7856f5214588b2c4fa023be15994d29d";
        private static readonly string RawImagePrefabGUID = "af72e630b8c3c2d449d46d7a3f89f6fb";
        private static readonly string TogglePrefabGUID = "5ac7a9d38f867c74dad6791a736ccd24";
        private static readonly string SliderPrefabGUID = "61a61991f43fe514f8b4babd264a49c5";
        private static readonly string ScrollbarPrefabGUID = "392f88c6a05685b4c83e9aa2593ed1ea";
        private static readonly string ScrollViewPrefabGUID = "fdc3253f4926aed459f4f76a53cf1d8c";
        private static readonly string ButtonPrefabGUID = "6bead26a5fb98a04cb187dc8d2ee734a";
        private static readonly string DropdownPrefabGUID = "7e90c0b25cd35ad48a3f40f9610957ab";
        private static readonly string InputFieldPrefabGUID = "bebe0e67455499d41b85ca59f6b88df8";



        [MenuItem("GameObject/ClimbGames/Image", false, 1)]
        public static void CreateImage(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, ImagePrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/Text (TMP)", false, 2)]
        public static void CreateText(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, TextPrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/RawImage", false, 3)]
        public static void CreateRawImage(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, RawImagePrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/Toggle (TMP)", false, 20)]
        public static void CreateToggle(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, TogglePrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/Slider", false, 20)]
        public static void CreateSlider(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, SliderPrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/Scrollbar", false, 20)]
        public static void CreateScrollbar(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, ScrollbarPrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/Scroll View", false, 20)]
        public static void CreateScrollView(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, ScrollViewPrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/Button (TMP)", false, 20)]
        public static void CreateButton(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, ButtonPrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/Dropdown (TMP)", false, 20)]
        public static void CreateDropdown(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, DropdownPrefabGUID);
        }

        [MenuItem("GameObject/ClimbGames/InputField (TMP)", false, 20)]
        public static void CreateInputField(MenuCommand menuCommand)
        {
            GameObject targetObject = menuCommand.context as GameObject;
            CreatePrefabInstance(targetObject, InputFieldPrefabGUID);
        }

        static Transform GetOrCreateParent(GameObject targetObject, out bool isCreated)
        {
            isCreated = false;
            if (targetObject == null || targetObject.GetComponentInParent<Canvas>() == null)
            {
                var canvas = GameObject.FindAnyObjectByType<Canvas>(FindObjectsInactive.Exclude);
                if (canvas == null)
                {
                    var go = new GameObject("Canvas");
                    canvas = go.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                    go.AddComponent<CanvasScaler>();
                    go.AddComponent<GraphicRaycaster>();

                    isCreated = true;
                }

                if (targetObject == null)
                    targetObject = canvas.gameObject;
            }

            return targetObject.transform;
        }

        static void CreatePrefabInstance(GameObject targetObject, string guid)
        {
            var _guid = new GUID(guid);

            GameObject prefab = AssetDatabase.LoadAssetByGUID<GameObject>(_guid);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            Transform parent = GetOrCreateParent(targetObject, out var isCreated);
            GameObjectUtility.SetParentAndAlign(instance, parent.gameObject);

            Undo.RegisterCreatedObjectUndo(isCreated ? parent.gameObject : instance, "Create " + instance.name);
            FocusAndRename(instance);
        }

        static void FocusAndRename(GameObject go)
        {
            Selection.activeObject = go;

            EditorApplication.delayCall += () =>
            {
                if (go != null)
                {
                    // 하이어라키 창에 포커스를 맞추고 이름 변경 단축키(F2 / Rename) 명령 실행
                    EditorWindow.FocusWindowIfItsOpen(typeof(SceneView)); // 또는 Hierarchy 창

                    // 유니티 내부 에디터벤치를 통해 하이어라키의 현재 선택된 이름 바꾸기 트리거
                    // (Alt + F2 또는 Rename 명령 전달)
                    System.Reflection.MethodInfo renameMethod = typeof(EditorWindow).Assembly
                        .GetType("UnityEditor.SceneHierarchyWindow")
                        .GetMethod("InitRenameMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

                    // 현재 열려있는 하이어라키 창 찾아서 이름 바꾸기 모드 실행
                    var hierarchyWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow"));
                    if (hierarchyWindow != null)
                    {
                        // 하이어라키 창에 포커스 강제 이동
                        hierarchyWindow.Focus();

                        // 에디터 이벤트로 Rename(F2) 단축키벤치를 강제로 보냄
                        // 키보드 이벤트(F2에 해당하는 씬/하이어라키 이름 변경 이벤트) 시뮬레이션
                        Event evt = EditorGUIUtility.CommandEvent("Rename");
                        if (evt != null)
                        {
                            EditorWindow.focusedWindow?.SendEvent(evt);
                        }
                    }
                }
            };
        }
    }
}