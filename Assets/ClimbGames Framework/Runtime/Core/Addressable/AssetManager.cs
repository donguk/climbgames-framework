using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ClimbGames
{
    public static class AssetManager
    {
        public static void Initialize()
        {

        }

        public static void Release<T>(T asset)
        {
            Addressables.Release(asset);
        }

        public static T LoadAsset<T>(string key)
        {
            var asyncOperation = Addressables.LoadAssetAsync<T>(key);
            T asset = asyncOperation.WaitForCompletion();

            return asset;
        }

        public static async UniTask<T> LoadAssetAsync<T>(string key)
        {
            var asyncOperation = Addressables.LoadAssetAsync<T>(key);
            await asyncOperation;

            return asyncOperation.Result;
        }

        public static async UniTask<T> InstantiateAsync<T>(string key, Transform parent = null)
        {
            var asyncOperation = Addressables.InstantiateAsync(key, parent);
            await asyncOperation;

            GameObject go = asyncOperation.Result;
            go.AddComponent<AssetInstanceHandler>().Initialize(asyncOperation);
            return go.GetComponent<T>();
        }

        public static async UniTask<GameObject> InstantiateAsync(string key, Transform parent = null)
        {
            var asyncOperation = Addressables.InstantiateAsync(key, parent);
            await asyncOperation;

            GameObject go = asyncOperation.Result;
            go.AddComponent<AssetInstanceHandler>().Initialize(asyncOperation);
            return go;
        }

        public static T Instantiate<T>(string key, Transform parent = null)
        {
            var asyncOperation = Addressables.InstantiateAsync(key, parent);
            GameObject go = asyncOperation.WaitForCompletion();

            go.AddComponent<AssetInstanceHandler>().Initialize(asyncOperation);
            return go.GetComponent<T>();
        }

        public static GameObject Instantiate(string key, Transform parent = null)
        {
            var asyncOperation = Addressables.InstantiateAsync(key, parent);
            GameObject go = asyncOperation.WaitForCompletion();

            go.AddComponent<AssetInstanceHandler>().Initialize(asyncOperation);
            return go;
        }
    }
}