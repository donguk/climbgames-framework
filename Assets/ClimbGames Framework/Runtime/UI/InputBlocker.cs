using System;
using UnityEngine;
using UnityEngine.UI;

namespace ClimbGames.UI
{
    public struct InputBlockScope : IDisposable
    {
        private readonly UILayer layer;

        public InputBlockScope(UILayer layer)
        {
            this.layer = layer;
            InputBlocker.Instance.SetBlock(this.layer, true);
            Debug.Log($"InputBlock >>> {layer}");
        }

        public readonly void Dispose()
        {
            InputBlocker.Instance.SetBlock(layer, false);
            Debug.Log($"InputBlock <<< {layer}");
        }
    }

    [SingletonConfig(true)]
    public class InputBlocker : MonoSingleton<InputBlocker>
    {
        [Serializable]
        public class BlockState
        {
            public GameObject blocker;
            public int count;

            public void SetBlock(bool value)
            {
                if (value)
                {
                    count++;

                    if (blocker.activeSelf == false)
                        blocker.SetActive(true);

                    blocker.transform.SetAsLastSibling();
                }
                else
                {
                    count = Mathf.Clamp(count - 1, 0, count);

                    if (count <= 0 && blocker.activeSelf)
                        blocker.SetActive(false);
                }
            }
        }

        [SerializeField] private SerializableDictionary<UILayer, BlockState> blockStates;

        public static InputBlockScope Block(UILayer layer = UILayer.Transition) => new InputBlockScope(layer);

        protected override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var pair in blockStates)
                GameObject.Destroy(pair.Value.blocker);
        }

        private BlockState GetBlockState(UILayer layer)
        {
            if (blockStates == null)
                blockStates = new SerializableDictionary<UILayer, BlockState>();

            if (blockStates.TryGetValue(layer, out var state) == false || state == null || state.blocker == null)
            {
                state = new BlockState() { blocker = CreateBlocker(layer), count = 0 };
                blockStates[layer] = state;
            }

            return state;
        }

        private GameObject CreateBlocker(UILayer layer)
        {
            Transform parent = UIManager.Instance.GetLayer(layer);
            GameObject blocker = CreateBlocker(parent);
            blocker.SetActive(false);
            blocker.transform.SetAsFirstSibling();
            return blocker;
        }

        private GameObject CreateBlocker(Transform parent)
        {
            var go = new GameObject($"Blocker");
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.AddComponent<Touchable>();

            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            return go;
        }

        public void SetBlock(UILayer layer, bool value)
        {
            BlockState state = GetBlockState(layer);
            state?.SetBlock(value);
        }
    }

    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public class Touchable : Graphic
    {
        public override Material material { get; set; }

        protected Touchable()
        {
            useLegacyMeshGeneration = false;
            raycastTarget = true;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }

        public override void SetVerticesDirty()
        {

        }

        public override void SetMaterialDirty()
        {

        }
    }
}