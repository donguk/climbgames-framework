using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace ClimbGames.Editor
{
    public class BuildWindow : EditorWindow
    {
        // bin 파일 드롭다운 관련
        private List<string> _binFilePaths = new List<string>();
        private List<string> _binDropdownOptions = new List<string>();
        private int _selectedBinIndex = 0;

        // env 파일
        private List<string> _envFilePaths = new List<string>();
        private List<string> _envDropdownOptions = new List<string>();
        private int _selectedEnvIndex = 0;

        private bool _uploadToHfs = true;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/ClimbGames/Build Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildWindow>("ClimbGames Build");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshBinFileList();
            RefreshEnvFileList();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUILayout.Space(10);

            DrawSettings();
            EditorGUILayout.Space(20);

            DrawAddressables();
            EditorGUILayout.Space(20);

            DrawEditorEnvironment();
            EditorGUILayout.Space(20);

            EditorGUILayout.EndScrollView();
        }

        void DrawSettings()
        {
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Root Path", BuildSettings.RootPath);
                if (GUILayout.Button("Browse", GUILayout.Width(70)))
                {
                    // 폴더 선택 창 오픈
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Root Directory", BuildSettings.RootPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        BuildSettings.RootPath = selectedPath;
                        GUI.FocusControl(null); // 입력 포커스 해제

                        RefreshBinFileList();
                        RefreshEnvFileList();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            BuildSettings.BuildType = (BuildType)EditorGUILayout.EnumPopup("Build Type", BuildSettings.BuildType);
            BuildSettings.BundleVersion = EditorGUILayout.TextField("Bundle Version", BuildSettings.BundleVersion);
            BuildSettings.VersionCode = EditorGUILayout.IntField("Version Code", BuildSettings.VersionCode);
            BuildSettings.BuildNumber = EditorGUILayout.IntField("Build Number", BuildSettings.BuildNumber);
            EditorGUILayout.Space(5);

            var targetGroup = BuildSettings.TargetGroup;
            switch (targetGroup)
            {
                case BuildTargetGroup.Android:
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("KeystoreName", BuildSettings.KeystoreName);
                        if (GUILayout.Button("Browse", GUILayout.Width(70)))
                        {
                            string selectedPath = EditorUtility.OpenFilePanel("Select Keystore File", Application.dataPath, "keystore,jks");
                            if (!string.IsNullOrEmpty(selectedPath))
                            {
                                // 절대 경로를 유니티 상대 경로(Assets/...)로 변환 시도
                                if (selectedPath.StartsWith(Application.dataPath))
                                {
                                    BuildSettings.KeystoreName = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                                }
                                else
                                {
                                    BuildSettings.KeystoreName = selectedPath;
                                }
                            }
                        }

                        GUIContent clearIcon = EditorGUIUtility.IconContent("TreeEditor.Trash");
                        clearIcon.tooltip = "Clear Keystore Settings";
                        if (GUILayout.Button(clearIcon, GUILayout.Width(28), GUILayout.Height(19)))
                        {
                            BuildSettings.KeystoreName = string.Empty;
                            BuildSettings.KeystorePass = string.Empty;
                            BuildSettings.KeyaliasName = string.Empty;
                            BuildSettings.KeyaliasPass = string.Empty;
                            GUI.FocusControl(null); // 입력 필드 포커스 해제
                        }
                        EditorGUILayout.EndHorizontal();

                        BuildSettings.KeystorePass = EditorGUILayout.TextField("KeystorePass", BuildSettings.KeystorePass);
                        BuildSettings.KeyaliasName = EditorGUILayout.TextField("KeyaliasName", BuildSettings.KeyaliasName);
                        BuildSettings.KeyaliasPass = EditorGUILayout.TextField("KeyaliasPass", BuildSettings.KeyaliasPass);
                        BuildSettings.BuildAppBundle = EditorGUILayout.Toggle("Build App Bundle", BuildSettings.BuildAppBundle);

                        break;
                    }
            }
            BuildSettings.DevelopmentBuild = EditorGUILayout.Toggle("Development Build", BuildSettings.DevelopmentBuild);

            EditorGUILayout.Space(5);
            if (GUILayout.Button($"Build {targetGroup}", GUILayout.Height(35)))
            {
                EditorApplication.delayCall += () =>
                {
                    ExecuteAppBuild();
                };
            }
        }

        void DrawAddressables()
        {
            EditorGUILayout.LabelField("Addressables", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            BuildSettings.PatchUrl = EditorGUILayout.TextField("Patch Url", BuildSettings.PatchUrl);

            // contentState.bin 파일 선택 드롭다운
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Content State (.bin)");
            if (_binDropdownOptions.Count > 0)
                _selectedBinIndex = EditorGUILayout.Popup(_selectedBinIndex, _binDropdownOptions.ToArray());

            GUIContent refreshIcon = EditorGUIUtility.IconContent("Refresh");
            refreshIcon.tooltip = "Refresh Bin List"; // 툴팁 표시
            if (GUILayout.Button(refreshIcon, GUILayout.Width(26), GUILayout.Height(19)))
                RefreshBinFileList();
            EditorGUILayout.EndHorizontal();

            _uploadToHfs = EditorGUILayout.Toggle("Upload To HFS", _uploadToHfs);
            EditorGUILayout.Space(5);

            bool isEmptyBin = _binDropdownOptions.Count == 0 || _binDropdownOptions[_selectedBinIndex] == "empty";
            // [New Build] 버튼
            if (GUILayout.Button("New Content", GUILayout.Height(35)))
                ExecuteNewContent();

            // [Update Content] 버튼 (bin이 'empty'면 비활성화)
            EditorGUI.BeginDisabledGroup(isEmptyBin);
            if (GUILayout.Button("Update Content", GUILayout.Height(35)))
                ExecuteUpdateContent();
            EditorGUI.EndDisabledGroup();
        }

        void DrawEditorEnvironment()
        {
            EditorGUILayout.LabelField("Editor Environment", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Env State (.zip)");
            if (_envDropdownOptions.Count > 0)
                _selectedEnvIndex = EditorGUILayout.Popup(_selectedEnvIndex, _envDropdownOptions.ToArray());

            GUIContent refreshIcon = EditorGUIUtility.IconContent("Refresh");
            refreshIcon.tooltip = "Refresh Editor Environment List"; // 툴팁 표시
            if (GUILayout.Button(refreshIcon, GUILayout.Width(26), GUILayout.Height(19)))
                RefreshEnvFileList();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(5);

            bool isEmptyEnv = _envDropdownOptions.Count == 0 || _envDropdownOptions[_selectedEnvIndex] == "empty";
            EditorGUI.BeginDisabledGroup(isEmptyEnv);
            if (GUILayout.Button("Apply Editor Environment", GUILayout.Height(35)))
                ApplyEditorEnvironment();
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// bin 백업 루트 폴더에서 파일 목록을 스캔하여 드롭다운 아이템을 채움
        /// </summary>
        private void RefreshBinFileList()
        {
            _binFilePaths.Clear();
            _binDropdownOptions.Clear();

            string rootPath = Path.Combine(BuildSettings.AddressablesPath, "ContentState");
            if (Directory.Exists(rootPath))
            {
                var files = Directory.GetFiles(rootPath, "addressables_content_state.bin", SearchOption.AllDirectories)
                                     .Select(p => p.Replace("\\", "/"))
                                     .ToList();

                // 최신 수정일 기준 정렬
                files = files.OrderByDescending(f =>
                {
                    string folderName = Path.GetFileName(Path.GetDirectoryName(f));
                    if (System.Version.TryParse(folderName, out var version))
                        return version;
                    return new System.Version(0, 0, 0);
                }).ToList();

                foreach (var filePath in files)
                {
                    _binFilePaths.Add(filePath);
                    string folderName = Path.GetFileName(Path.GetDirectoryName(filePath));
                    _binDropdownOptions.Add(folderName);
                }
            }

            if (_binDropdownOptions.Count > 0)
            {
                // 최상단을 'latest'로 표기 및 선택
                _binDropdownOptions[0] += $" (latest)";
                _selectedBinIndex = 0;
            }
            else
            {
                _binDropdownOptions.Add("empty");
                _selectedBinIndex = 0;
            }
        }

        private void RefreshEnvFileList()
        {
            _envFilePaths.Clear();
            _envDropdownOptions.Clear();

            string rootPath = Path.Combine(BuildSettings.AddressablesPath, "ContentState");
            if (Directory.Exists(rootPath))
            {
                var files = Directory.GetFiles(rootPath, "EditorEnv_*.zip", SearchOption.AllDirectories)
                                     .Select(p => p.Replace("\\", "/"))
                                     .ToList();

                // 최신 수정일 기준 정렬
                files = files.OrderByDescending(f =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(f);
                    string versionStr = fileName.Replace("EditorEnv_", "");
                    versionStr = versionStr.Replace('_', '.');

                    if (System.Version.TryParse(versionStr, out var version))
                        return version;
                    return new System.Version(0, 0, 0);
                }).ToList();

                foreach (var filePath in files)
                {
                    _envFilePaths.Add(filePath);
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    _envDropdownOptions.Add(fileName);
                }
            }

            if (_envDropdownOptions.Count > 0)
            {
                _selectedEnvIndex = 0;
            }
            else
            {
                _envDropdownOptions.Add("empty");
                _selectedEnvIndex = 0;
            }
        }

        void ExecuteAppBuild()
        {
            if (!EditorUtility.DisplayDialog("App Build", "Are you sure you want to start the build?", "Yes", "No"))
                return;

            using (new AddressableAssetSettingsScope(AddressableAssetSettingsDefaultObject.Settings))
            {
                BuildSettings.ApplySettings();
                ProjectBuilder.BuildPlayerContent();

                var targetGroup = BuildSettings.TargetGroup;
                switch (targetGroup)
                {
                    case BuildTargetGroup.Android:
                        {
                            ProjectBuilder.BuildAndroid();
                            break;
                        }
                    default: EditorUtility.DisplayDialog("Feature Not Implemented", $"{targetGroup} build support is not implemented yet.", "OK"); break;
                }
            }

            if (_uploadToHfs)
                UploadToHfs().Forget();

            RefreshBinFileList();
            RefreshEnvFileList();
        }

        /// <summary>
        /// New Build 실행 및 파일 백업
        /// </summary>
        private void ExecuteNewContent()
        {
            if (!EditorUtility.DisplayDialog("New Build", "Are you sure you want to proceed with a new full bundle build?", "Yes", "No"))
                return;

            using (new AddressableAssetSettingsScope(AddressableAssetSettingsDefaultObject.Settings))
            {
                BuildSettings.ApplySettings();
                ProjectBuilder.BuildPlayerContent();
            }

            if (_uploadToHfs)
                UploadToHfs().Forget();

            RefreshBinFileList();
            RefreshEnvFileList();
        }

        /// <summary>
        /// Update Content 실행
        /// </summary>
        private void ExecuteUpdateContent()
        {
            if (_selectedBinIndex < 0 || _selectedBinIndex >= _binFilePaths.Count)
                return;

            string selectedBinPath = _binFilePaths[_selectedBinIndex];
            if (File.Exists(selectedBinPath) == false)
            {
                Debug.LogError($"The selected .bin file could not be found: {selectedBinPath}");
                return;
            }

            if (!EditorUtility.DisplayDialog("Update Content", $"Proceed with the patch build based on the selected file?\n{selectedBinPath}", "Yes", "No"))
                return;

            using (new AddressableAssetSettingsScope(AddressableAssetSettingsDefaultObject.Settings))
            {
                BuildSettings.ApplySettings();
                ProjectBuilder.BuildContentUpdate(selectedBinPath);
            }

            if (_uploadToHfs)
                UploadToHfs().Forget();

            RefreshEnvFileList();
        }

        void ApplyEditorEnvironment()
        {
            if (_selectedEnvIndex < 0 || _selectedEnvIndex >= _envFilePaths.Count)
                return;

            string selectedEnvPath = _envFilePaths[_selectedEnvIndex];
            if (File.Exists(selectedEnvPath) == false)
            {
                Debug.LogError($"The selected .zip file could not be found: {selectedEnvPath}");
                return;
            }

            try
            {
                string targetFolderPath = $"Library/com.unity.addressables/aa/{BuildSettings.TargetPlatform}";
                if (Directory.Exists(targetFolderPath))
                    Directory.Delete(targetFolderPath, true);
                Directory.CreateDirectory(targetFolderPath);

                string cacheFolderPath = $"{Application.persistentDataPath}/com.unity.addressables";
                if (Directory.Exists(cacheFolderPath))
                    Directory.Delete(cacheFolderPath, true);

                // 압축 해제 (overwriteFiles: true -> 기존 파일 존재 시 덮어쓰기)
                ZipFile.ExtractToDirectory(selectedEnvPath, targetFolderPath, overwriteFiles: true);
                Debug.Log($"[AssetBuildWindow] Successfully extracted to: {targetFolderPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[AssetBuildWindow] Failed to extract zip: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to extract zip file.\n{e.Message}", "OK");
            }
        }

        async UniTask UploadToHfs()
        {
            var progress = Cysharp.Threading.Tasks.Progress.Create<FileUploadInfo>(info =>
            {
                string text = $"Uploading({info.currentIndex + 1}/{info.totalCount}) {info.fileName}... ({info.progress * 100:F0}%)";
                EditorUtility.DisplayProgressBar("HFS Upload", text, info.progress);
            });

            await ProjectBuilder.UploadToHfs(progress);
            EditorUtility.ClearProgressBar();
        }
    }
}