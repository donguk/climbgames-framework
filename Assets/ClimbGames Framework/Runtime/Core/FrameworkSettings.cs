using UnityEngine;

namespace ClimbGames
{
    [AssetPath("Assets/ClimbGames/FrameworkSettings.asset", true)]
    public class FrameworkSettings : ScriptableSingleton<FrameworkSettings>
    {
        [SerializeField] private bool useDefaultTransition = true;
        [SerializeField] private float defaultTransitionTime = 0.2f;
        [SerializeField] private bool useEmptyScene = true;

        public bool ShowSceneName { get; set; }

        public bool UseDefaultTransition => useDefaultTransition;
        public float DefaultTransitionTime => defaultTransitionTime;
        public bool UseEmptyScene => useEmptyScene;
    }
}