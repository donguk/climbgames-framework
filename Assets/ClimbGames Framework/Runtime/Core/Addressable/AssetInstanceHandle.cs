using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ClimbGames
{
    internal class AssetInstanceHandle : MonoBehaviour
    {
        private AsyncOperationHandle<GameObject> _handle;
        private bool _isInitialized = false;

        public void Initialize(AsyncOperationHandle<GameObject> handle = default)
        {
            _handle = handle;
            _isInitialized = true;
        }

        private void OnDestroy()
        {
            // 1. 핸들이 유효하다면 핸들을 통해 해제
            if (_isInitialized && _handle.IsValid())
            {
                Addressables.Release(_handle);
            }
            else
            {
                // 2. 핸들이 없더라도 인스턴스 자체를 넘겨 안전하게 해제 (Fallback)
                Addressables.ReleaseInstance(gameObject);
            }
        }
    }
}