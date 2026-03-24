using UnityEngine;

namespace ClimbGames
{
    [AssetPath("Assets/ClimbGames/Resources/FrameworkSettings.asset")]
    public class FrameworkSettings : ScriptableSingleton<FrameworkSettings>
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showSceneName = true;

        [Header("Transition Settings")]
        [SerializeField] private bool useDefaultTransition = true;
        [SerializeField] private float defaultTransitionTime = 0.2f;
        [SerializeField] private bool useEmptyScene = true;

        public bool ShowSceneName => showSceneName;

        public bool UseDefaultTransition => useDefaultTransition;
        public float DefaultTransitionTime => defaultTransitionTime;
        public bool UseEmptyScene => useEmptyScene;
    }
}