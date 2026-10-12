using ClimbGames.Eidot.UI;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class ListBinding : Binding
    {
        public ListBinding(string prefix) : base(prefix)
        {
        }

        public override BindingElement CreateElement(GameObject target)
        {
            return new ListElement(target, this);
        }
    }
}