using UnityEngine;
using UnityEditor;

namespace ClimbGames.Editor
{
    public class EditorResizer
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }

        private readonly Direction direction;

        private readonly float minFirstSize;
        private readonly float minSecondSize;
        private readonly float splitterSize;

        private float sizeRatio;
        private bool isResizing;

        public EditorResizer(Direction direction, float sizeRatio, float minFirstSize, float minSecondSize, float splitterSize = 2f)
        {
            this.direction = direction;
            this.sizeRatio = sizeRatio;

            this.minFirstSize = minFirstSize;
            this.minSecondSize = minSecondSize;
            this.splitterSize = splitterSize;
        }

        public EditorResizer(Direction direction, float sizeRatio = 0.5f, float splitterSize = 2f) : this(direction, sizeRatio, 10f, 10f, splitterSize)
        {

        }

        public void Resize(Rect rect, out Rect firstRect, out Rect secondRect)
        {
            float totalSize = direction == Direction.Horizontal ? rect.width : rect.height;
            float maxFirstSize = totalSize - splitterSize - minSecondSize;

            float firstSize = totalSize * sizeRatio;
            firstSize = Mathf.Clamp(firstSize, minFirstSize, maxFirstSize);

            Rect splitterRect;
            if (direction == Direction.Horizontal)
            {
                firstRect = new Rect(rect.x, rect.y, firstSize, rect.height);
                splitterRect = new Rect(firstRect.xMax, rect.y, splitterSize, rect.height);
                secondRect = new Rect(splitterRect.xMax, rect.y, rect.xMax - splitterRect.xMax, rect.height);
            }
            else
            {
                firstRect = new Rect(rect.x, rect.y, rect.width, firstSize);
                splitterRect = new Rect(rect.x, firstRect.yMax, rect.width, splitterSize);
                secondRect = new Rect(rect.x, splitterRect.yMax, rect.width, rect.yMax - splitterRect.yMax);
            }

            Rect splitterHitRect = DrawSpliter(splitterRect);
            HandleResize(rect, splitterHitRect, maxFirstSize);
        }

        private Rect DrawSpliter(Rect rect)
        {
            Rect splitterHitRect;

            if (direction == Direction.Horizontal)
            {
                splitterHitRect = new Rect(rect.x - 3f, rect.y, rect.width + 6f, rect.height);
            }
            else
            {
                splitterHitRect = new Rect(rect.x, rect.y - 3f, rect.width, rect.height + 6f);
            }

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.65f, 0.65f, 0.65f));
            EditorGUIUtility.AddCursorRect(splitterHitRect, direction == Direction.Horizontal ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);

            return splitterHitRect;
        }

        private void HandleResize(Rect parentRect, Rect splitterRect, float maxFirstSize)
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && splitterRect.Contains(e.mousePosition))
            {
                isResizing = true;
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);

                e.Use();
            }

            if (e.type == EventType.MouseDrag && isResizing)
            {
                float newSize;
                float totalSize = direction == Direction.Horizontal ? parentRect.width : parentRect.height;

                if (direction == Direction.Horizontal)
                {
                    newSize = e.mousePosition.x - parentRect.x;
                }
                else
                {
                    newSize = e.mousePosition.y - parentRect.y;
                }

                newSize = Mathf.Clamp(newSize, minFirstSize, maxFirstSize);
                sizeRatio = Mathf.Clamp(newSize / totalSize, 0f, 1f);

                GUI.changed = true;
                e.Use();
            }

            if (e.type == EventType.MouseUp && isResizing)
            {
                isResizing = false;
                GUIUtility.hotControl = 0;

                e.Use();
            }
        }
    }
}