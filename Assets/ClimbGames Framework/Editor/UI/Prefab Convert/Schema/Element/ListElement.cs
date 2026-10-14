using System;
using System.Collections.Generic;
using System.Linq;
using ClimbGames.Editor.UI;
using UnityEngine;

namespace ClimbGames.Eidot.UI
{
    public class ListElement : BindingElement
    {
        private BindingElement virtualRoot;
        private List<BindingElement> items;

        public ListElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        protected override void OnInitialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            base.OnInitialize(parent, schema, depth);

            virtualRoot = new BindingElement(Target, schema);
        }

        protected override BindingElement AddChild(BindingElement element)
        {
            if (items == null)
                items = new List<BindingElement>();

            // 첫번째 자식의 타입으로 결정
            if (items.Count <= 0 || items[0].GetFieldTypeName() == element.GetFieldTypeName())
            {
                items.Add(element);

                // create none binding element
                element = new BindingElement(element);
            }

            return base.AddChild(element);
        }

        public override string GetFieldTypeName()
        {
            string fielTypeName = items?.FirstOrDefault()?.GetFieldTypeName() ?? GetFieldTypeName();
            return $"List<{fielTypeName}>";
        }

        public override object GetFieldValue()
        {
            var itemType = items?.FirstOrDefault()?.FieldType ?? FieldType;
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