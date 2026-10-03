using ClimbGames.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ClimbGames
{
    public class TitleScene : MonoScene
    {
        public override async UniTask InitializeAsync()
        {
            await AssetManager.Initialize();

            await Tables.LoadAsync<TextAsset>("tables");

            var data = Tables.Character.Datas;

            await UIManager.Instance.ShowUI<UIPanelTitle>("UIPanelTitle", UILayer.View);
        }
    }
}