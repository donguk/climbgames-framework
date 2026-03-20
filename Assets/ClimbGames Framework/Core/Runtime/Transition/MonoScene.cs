using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ClimbGames.Core
{
    public class MonoScene : MonoBehaviour
    {
        public virtual UniTask InitializeAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask ActivateAsync()
        {
            return UniTask.CompletedTask;
        }

        public virtual void Deactivate()
        {

        }

        void OnGUI()
        {
            if (FrameworkSettings.Instance.ShowSceneName == false)
                return;

            string content = $"🎬 {GetType().Name}";
            Vector2 size = GUIStyles.SceneLabelStyle.CalcSize(new GUIContent(content));
            Vector2 position = new Vector2(10f, Screen.height - size.y - 15f);

            Rect rect = new Rect(position.x, position.y, size.x, size.y);
            GUI.Box(rect, "", GUIStyles.BackgroundBoxStyle);
            GUI.Label(rect, content, GUIStyles.SceneLabelStyle);
        }
    }
}