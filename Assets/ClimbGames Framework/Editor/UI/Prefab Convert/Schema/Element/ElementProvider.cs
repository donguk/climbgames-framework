using System;
using ClimbGames.Eidot.UI;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public abstract class ElementProvider
    {
        protected Binding binding;

        public ElementProvider(Binding binding)
        {
            this.binding = binding;
        }

        public abstract BindingElement CreateElement(GameObject target);
    }

    public class ListElementProvider : ElementProvider
    {
        public ListElementProvider(Binding binding) : base(binding)
        {
        }

        public override BindingElement CreateElement(GameObject target) => new ListElement(target, binding);
    }

    public class GroupElementProvider : ElementProvider
    {
        public GroupElementProvider(Binding binding) : base(binding)
        {
        }

        public override BindingElement CreateElement(GameObject target) => new GroupElement(target, binding);
    }

    public class UIBaseElementProvider : ElementProvider
    {
        public UIBaseElementProvider(Binding binding) : base(binding)
        {
        }

        public override BindingElement CreateElement(GameObject target) => new UIBaseElement(target, binding);
    }

    public class ComponentElementProvider : ElementProvider
    {
        public ComponentElementProvider(Binding binding) : base(binding)
        {
        }

        public override BindingElement CreateElement(GameObject target) => new ComponentElement(target, binding);
    }
}