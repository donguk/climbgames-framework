using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace ClimbGames
{
    [UxmlElement]
    public partial class SelectPathField : VisualElement
    {
        private const string UxmlGUID = "540b38a4aaffbb74e9d798b18fbef6cb";

        private Label title;
        private Label path;
        private Button browse;

        public event Action<string> OnPathChanged;

        [UxmlAttribute]
        public string Title
        {
            get => title?.text;
            set { if (title != null) title.text = value; }
        }

        [UxmlAttribute]
        public string Path
        {
            get => path?.text;
            set
            {
                if (path != null)
                {
                    path.text = value;
                    OnPathChanged?.Invoke(value);
                }
            }
        }

        public SelectPathField()
        {
            string uxmlPath = AssetDatabase.GUIDToAssetPath(UxmlGUID);

            // UXML 로드 및 바인딩
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            visualTree.CloneTree(this);

            // 내부 요소 찾기
            title = this.Q<Label>("title");
            path = this.Q<Label>("path");
            browse = this.Q<Button>("browse");

            // 버튼 클릭 이벤트 등록
            if (browse != null)
            {
                browse.clicked += OnBrowseClicked;
            }
        }

        private void OnBrowseClicked()
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Path", Path, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                Path = selectedPath;
            }
        }
    }
}