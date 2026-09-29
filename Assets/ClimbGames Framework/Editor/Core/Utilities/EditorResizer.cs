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

        private float firstSize;
        private bool isResizing;

        public EditorResizer(Direction direction, float firstSize, float minFirstSize = 10f, float minSecondSize = 10f, float splitterSize = 5f)
        {
            this.direction = direction;
            this.firstSize = firstSize;

            this.minFirstSize = minFirstSize;
            this.minSecondSize = minSecondSize;
            this.splitterSize = splitterSize;
        }

        public void Resize(Rect rect, out Rect firstRect, out Rect secondRect)
        {
            float totalSize = direction == Direction.Horizontal ? rect.width : rect.height;
            float maxFirstSize = totalSize - splitterSize - minSecondSize;

            firstSize = Mathf.Clamp(firstSize, minFirstSize, maxFirstSize);

            if (direction == Direction.Horizontal)
            {
                firstRect = new Rect(rect.x, rect.y, firstSize, rect.height);
                Rect splitterRect = new Rect(firstRect.xMax, rect.y, splitterSize, rect.height);
                secondRect = new Rect(splitterRect.xMax, rect.y, rect.xMax - splitterRect.xMax, rect.height);

                HandleResize(rect, splitterRect, maxFirstSize);
            }
            else
            {
                firstRect = new Rect(rect.x, rect.y, rect.width, firstSize);
                Rect splitterRect = new Rect(rect.x, firstRect.yMax, rect.width, splitterSize);
                secondRect = new Rect(rect.x, splitterRect.yMax, rect.width, rect.yMax - splitterRect.yMax);

                HandleResize(rect, splitterRect, maxFirstSize);
            }
        }

        private void HandleResize(Rect parentRect, Rect splitterRect, float maxFirstSize)
        {
            Event e = Event.current;

            EditorGUIUtility.AddCursorRect(splitterRect, direction == Direction.Horizontal ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);

            if (e.type == EventType.MouseDown && e.button == 0 && splitterRect.Contains(e.mousePosition))
            {
                isResizing = true;
                e.Use();
            }

            if (e.type == EventType.MouseDrag && isResizing)
            {
                float newSize;

                if (direction == Direction.Horizontal)
                {
                    newSize = e.mousePosition.x - parentRect.x;
                }
                else
                {
                    newSize = e.mousePosition.y - parentRect.y;
                }

                firstSize = Mathf.Clamp(newSize, minFirstSize, maxFirstSize);

                GUI.changed = true;
                e.Use();
            }

            if (e.type == EventType.MouseUp && isResizing)
            {
                isResizing = false;
                e.Use();
            }
        }
    }
}