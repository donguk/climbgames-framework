using System;
using UnityEngine;

namespace ClimbGames.Editor.UI
{


    [Serializable]
    public class UISettings : ClimbGamesSettings<UISettings>
    {
        [SerializeField] private PrefabConvertSettings prefabConvertSettings = new PrefabConvertSettings();

        public PrefabConvertSettings PrefabConvertSettings => prefabConvertSettings;

        public override void Reset()
        {
            prefabConvertSettings.Reset();
        }
    }
}