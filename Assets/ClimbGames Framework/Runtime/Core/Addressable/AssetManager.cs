using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ClimbGames
{
    public static class AssetManager
    {
        public static string UpdateCatalogPath { get; private set; }

        public static async UniTask Initialize()
        {
            // settings.json 로드 및 cache catalog 확인
            await Addressables.InitializeAsync();
        }

        private static string TransformInternalId(IResourceLocation location)
        {
            string internalId = location.InternalId;
            if (internalId.StartsWith("http://") || internalId.StartsWith("https://"))
            {
                if (internalId.EndsWith(".hash")) // hash 파일 url 로 부터 bin, json 파일 url 생성
                {
                    string catalogPath = UpdateCatalogPath.Trim();
                    if (catalogPath.EndsWith(".json") || catalogPath.EndsWith(".bin"))
                    {
                        var uri = new System.Uri(catalogPath);
                        string newAbsolutePath = Path.ChangeExtension(uri.AbsolutePath, ".hash");

                        var uriBuilder = new System.UriBuilder(uri)
                        {
                            Path = newAbsolutePath
                        };
                        catalogPath = uriBuilder.Uri.AbsoluteUri;
                    }

                    Debug.Log($"[AssetManager] TransformInternalId: {catalogPath}");
                    return catalogPath;
                }
            }
            return internalId;
        }

        public static async UniTask<CatalogUpdateInfo> CheckForCatalogUpdates(string catalogPath)
        {
            UpdateCatalogPath = catalogPath;

            Addressables.InternalIdTransformFunc += TransformInternalId;
            CatalogUpdateInfo updateInfo = new CatalogUpdateInfo();

            var asyncHandle = Addressables.CheckForCatalogUpdates(false);
            await asyncHandle;

            if (asyncHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var locators = asyncHandle.Result;
                if (locators != null && locators.Count > 0)
                {
                    // 카탈로그 업데이트
                    await Addressables.UpdateCatalogs(locators);

                    long sizeBytes = await GetDownloadSizeAsync();
                    float sizeMB = sizeBytes / (1024f * 1024f);

                    updateInfo.downloadSize = sizeMB;
                }
                updateInfo.locators = locators;
            }

            Addressables.Release(asyncHandle);
            Addressables.InternalIdTransformFunc -= TransformInternalId;
            return updateInfo;
        }

        public static async UniTask<long> GetDownloadSizeAsync()
        {
            HashSet<object> keys = new HashSet<object>();
            foreach (var locator in Addressables.ResourceLocators)
            {
                foreach (var key in locator.Keys)
                    keys.Add(key);
            }

            var asyncHandle = Addressables.GetDownloadSizeAsync(keys);
            await asyncHandle;

            long downloadBytes = asyncHandle.Result;
            Addressables.Release(asyncHandle);

            return downloadBytes;
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