using ClimbGames.UI;
using Cysharp.Threading.Tasks;

namespace ClimbGames
{
    public class TitleScene : MonoScene
    {
        public override async UniTask InitializeAsync()
        {
            await AssetManager.Initialize();

            await UIManager.Instance.ShowUI<UIPanelTitle>("UIPanelTitle", UILayer.View);
        }
    }
}