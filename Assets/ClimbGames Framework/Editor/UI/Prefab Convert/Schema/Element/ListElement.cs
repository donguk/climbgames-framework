using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class ListElement : BindingElement
    {
        private List<BindingElement> items;

        public ListElement(GameObject target, Binding binding) : base(target, binding)
        {
        }


        protected override void SearchElements(Transform transform, int depth)
        {
            // 
            SearchElementsInternal(transform, depth);
        }

        protected override BindingElement AddChild(BindingElement element)
        {
            if (items == null)
                items = new List<BindingElement>();

            // 첫번째 자식의 타입으로 결정
            if (items.Count <= 0 || items[0].GetFieldTypeName() == element.GetFieldTypeName())
            {
                items.Add(element);

                // 
                List<BindingElement> children = null;
                if (IsIncludeChildren)
                    children = element.GetChildren();

                // create none binding element
                element = new BindingElement(element, children);
            }

            return base.AddChild(element);
        }

        public override string GetFieldTypeName()
        {
            string fielTypeName = items?.FirstOrDefault()?.GetFieldTypeName() ?? GetTypeName(typeof(Transform));
            return $"List<{fielTypeName}>";
        }

        public override object GetFieldValue()
        {
            var itemType = items?.FirstOrDefault()?.FieldType ?? typeof(Transform);
            var type = typeof(List<>).MakeGenericType(itemType);
            var list = (System.Collections.IList)Activator.CreateInstance(type);

            if (items != null)
            {
                foreach (var item in items)
                {
                    var value = item.GetFieldValue();
                    if (value == null)
                    {
                        Debug.LogError($"[ListElement] item value is null: {item.FieldName}");
                        continue;
                    }

                    if (itemType.IsAssignableFrom(value.GetType()))
                        list.Add(value);
                    else
                        Debug.LogError($"[ListElement] can not add item: {item.FieldName}");
                }
            }

            return list;
        }
    }
}