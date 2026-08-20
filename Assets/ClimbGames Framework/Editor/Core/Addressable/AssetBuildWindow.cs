using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClimbGames.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace ClimbGames
{
    public class AssetBuildWindow : EditorWindow
    {
        private string _bundleVersion = "0.1.0";
        private int _patchNumber = 0;

        // bin 파일 드롭다운 관련
        private List<string> _binFilePaths = new List<string>();
        private List<string> _binDropdownOptions = new List<string>();
        private int _selectedBinIndex = 0;

        // 루트 저장 경로 정의
        private const string BinBackupRootFolder = "com.unity.addressables";

        [MenuItem("Tools/ClimbGames/Asset Build Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetBuildWindow>("Asset Build");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            _bundleVersion = Application.version;
            RefreshBinFileList();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Addressables Build Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 1. 컨트롤 요소 (버전 및 패치 번호)
            _bundleVersion = EditorGUILayout.TextField("Bundle Version", _bundleVersion);
            _patchNumber = EditorGUILayout.IntField("Patch Number", _patchNumber);

            EditorGUILayout.Space();

            // 2. contentState.bin 파일 선택 드롭다운
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Content State (.bin)");

            if (_binDropdownOptions.Count > 0)
            {
                _selectedBinIndex = EditorGUILayout.Popup(_selectedBinIndex, _binDropdownOptions.ToArray());
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Refresh Bin List", GUILayout.Width(120)))
            {
                RefreshBinFileList();
            }

            EditorGUILayout.Space(20);

            // 3. 빌드 버튼영역
            bool isEmptyBin = _binDropdownOptions.Count == 0 || _binDropdownOptions[_selectedBinIndex] == "empty";

            // [New Build] 버튼
            if (GUILayout.Button("New Build", GUILayout.Height(35)))
            {
                ExecuteNewBuild();
            }

            EditorGUILayout.Space(5);

            // [Update Content] 버튼 (bin이 'empty'면 비활성화)
            EditorGUI.BeginDisabledGroup(isEmptyBin);
            if (GUILayout.Button("Update Content", GUILayout.Height(35)))
            {
                ExecuteUpdateContent();
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// bin 백업 루트 폴더에서 파일 목록을 스캔하여 드롭다운 아이템을 채움
        /// </summary>
        private void RefreshBinFileList()
        {
            _binFilePaths.Clear();
            _binDropdownOptions.Clear();

            string contentStateFolder = Path.Combine(AssetBuilder.BuildRoot, "ContentState");
            if (Directory.Exists(contentStateFolder))
            {
                var files = Directory.GetFiles(contentStateFolder, "addressables_content_state.bin", SearchOption.AllDirectories)
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
                _binDropdownOptions.Insert(0, $"latest ({_binDropdownOptions[0]})");
                _binFilePaths.Insert(0, _binFilePaths[0]); // latest는 가장 최신 파일 경로 매핑
                _selectedBinIndex = 0;
            }
            else
            {
                _binDropdownOptions.Add("empty");
                _selectedBinIndex = 0;
            }
        }

        /// <summary>
        /// New Build 실행 및 파일 백업
        /// </summary>
        private void ExecuteNewBuild()
        {
            if (!EditorUtility.DisplayDialog("New Build", "새로운 전체 번들 빌드를 진행하시겠습니까?", "Yes", "No"))
                return;

            AssetBuilder.BuildNewContent(_bundleVersion, _patchNumber);
            RefreshBinFileList();
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
                Debug.LogError($"선택한 .bin 파일을 찾을 수 없습니다: {selectedBinPath}");
                return;
            }

            if (!EditorUtility.DisplayDialog("Update Content", $"다음 기준 파일로 패치 빌드를 진행하시겠습니까?\n{selectedBinPath}", "Yes", "No"))
                return;

            AssetBuilder.BuildContentUpdate(selectedBinPath, _bundleVersion, _patchNumber);
        }

        /// <summary>
        /// 빌드 결과물 지정 폴더 정리 및 bin 파일 백업
        /// </summary>
        private void BackupAndOrganizeFiles(string originalBinPath, bool isUpdate = false)
        {
            string targetPlatform = EditorUserBuildSettings.activeBuildTarget.ToString();
            string versionFolder = $"{_bundleVersion}_{_patchNumber}";

            // 1. bin 파일 백업 경로: com.unity.addressables/{TargetPlatform}/{bundleVersion}/addressables_content_state.bin
            string binBackupDir = Path.Combine(BinBackupRootFolder, targetPlatform, _bundleVersion);
            Directory.CreateDirectory(binBackupDir);

            if (File.Exists(originalBinPath) && !isUpdate)
            {
                string destBinPath = Path.Combine(binBackupDir, "addressables_content_state.bin");
                File.Copy(originalBinPath, destBinPath, true);
                Debug.Log($"[.bin 백업 완료] {destBinPath}");
            }

            // 2. ServerData 및 aa 폴더 경로 생성 확인
            string serverDataDir = Path.Combine("ServerData", versionFolder);
            string aaDir = Path.Combine("aa", $"{targetPlatform}_{versionFolder}");

            Directory.CreateDirectory(serverDataDir);
            Directory.CreateDirectory(aaDir);

            Debug.Log($"[폴더링 완료]\nServerData: {serverDataDir}\naa: {aaDir}");

            AssetDatabase.Refresh();
        }
    }
}