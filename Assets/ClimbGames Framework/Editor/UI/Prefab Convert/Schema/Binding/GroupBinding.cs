using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class GroupBinding : Binding
    {
        public GroupBinding(string prefix) : base(prefix)
        {

        }

        public override BindingElement CreateElement(GameObject target)
        {
            return new GroupElement(target, this);
        }
    }
}