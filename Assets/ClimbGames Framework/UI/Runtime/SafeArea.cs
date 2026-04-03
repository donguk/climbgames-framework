using UnityEngine;

namespace ClimbGames
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea = new Rect(0, 0, 0, 0);
        private Vector2 _lastScreenSize = new Vector2(0, 0);

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            Refresh();
        }

        void Update()
        {
            // 해상도나 기기 방향이 바뀔 때만 갱신
            if (_lastSafeArea != Screen.safeArea ||
                _lastScreenSize.x != Screen.width ||
                _lastScreenSize.y != Screen.height)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            Rect area = Screen.safeArea;

            // Screen 좌표를 0~1 사이의 비율(Anchor)로 변환
            Vector2 anchorMin = area.position;
            Vector2 anchorMax = area.position + area.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // 앵커 적용
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // Offset 초기화 (앵커에 딱 맞게)
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            _lastSafeArea = area;
            _lastScreenSize = new Vector2(Screen.width, Screen.height);
        }
    }
}