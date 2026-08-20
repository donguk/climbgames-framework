using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ClimbGames
{
    public struct AssetPatchInfo
    {
        public List<string> locators;
        public float size;

        public override string ToString()
        {
            string text = string.Empty;
            if (locators != null)
            {
                for (int i = 0; i < locators.Count; ++i)
                {
                    if (i > 0)
                        text += ", \n";
                    text += locators[i];
                }
            }

            return $"locators({text})/ size({size} MB)";
        }
    }

    public static class AssetManager
    {
        public static async UniTask Initialize()
        {
            Addressables.InternalIdTransformFunc += TransformInternalId;
            await Addressables.InitializeAsync();
        }

        private static string TransformInternalId(IResourceLocation location)
        {
            Debug.Log($"[TransformInternalId] location.InternalId({location.InternalId})");
            return location.InternalId;
        }

        public static async UniTask<AssetPatchInfo> CheckForCatalogUpdates(string catalogKey)
        {
            AssetPatchInfo patchInfo = new AssetPatchInfo();

            var checkHandle = Addressables.CheckForCatalogUpdates(false);
            await checkHandle;

            if (checkHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var locators = checkHandle.Result;
                if (locators != null && locators.Count > 0)
                {
                    // 카탈로그 업데이트
                    await Addressables.UpdateCatalogs(locators);




                    // 3. 현재 등록된 모든 카탈로그(ResourceLocator)에서 전체 에셋 Key 추출
                    HashSet<object> allKeys = new HashSet<object>();

                    foreach (var locator in Addressables.ResourceLocators)
                    {
                        foreach (var key in locator.Keys)
                        {
                            // 내부적으로 자동 생성된 GUID나 부가 정보 형식이 아닌 기본 Key들만 수집
                            allKeys.Add(key);
                        }
                    }


                    var sizeHandle = Addressables.GetDownloadSizeAsync(allKeys);
                    await sizeHandle;

                    long sizeBytes = sizeHandle.Result;
                    float sizeMB = sizeBytes / (1024f * 1024f);

                    patchInfo.size = sizeMB;
                    Addressables.Release(sizeHandle);
                }
                patchInfo.locators = locators;
            }

            Addressables.Release(checkHandle);
            return patchInfo;
        }

        public static void Release<T>(T asset)
        {
            Addressables.Release(asset);
        }

        public static T LoadAsset<T>(string key)
        {
            var asyncHandle = Addressables.LoadAssetAsync<T>(key);
            T asset = asyncHandle.WaitForCompletion();

            return asset;
        }

        public static async UniTask<T> LoadAssetAsync<T>(string key)
        {
            var asyncHandle = Addressables.LoadAssetAsync<T>(key);
            await asyncHandle;

            return asyncHandle.Result;
        }

        public static async UniTask<T> InstantiateAsync<T>(string key, Transform parent = null)
        {
            var asyncHandle = Addressables.InstantiateAsync(key, parent);
            await asyncHandle;

            GameObject go = asyncHandle.Result;
            go.AddComponent<AssetInstanceHandler>().Initialize(asyncHandle);
            return go.GetComponent<T>();
        }

        public static async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var asyncHandle = Addressables.InstantiateAsync(key, parent);
            await asyncHandle;

            GameObject go = asyncHandle.Result;
            go.AddComponent<AssetInstanceHandler>().Initialize(asyncHandle);
            return go;
        }

        public static T Instantiate<T>(string key, Transform parent = null)
        {
            var asyncHandle = Addressables.InstantiateAsync(key, parent);
            GameObject go = asyncHandle.WaitForCompletion();

            go.AddComponent<AssetInstanceHandler>().Initialize(asyncHandle);
            return go.GetComponent<T>();
        }

        public static GameObject Instantiate(string key, Transform parent = null)
        {
            var asyncHandle = Addressables.InstantiateAsync(key, parent);
            GameObject go = asyncHandle.WaitForCompletion();

            go.AddComponent<AssetInstanceHandler>().Initialize(asyncHandle);
            return go;
        }
    }
}