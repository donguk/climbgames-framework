using System;
using System.Collections.Generic;
using System.Text;
using ClimbGames.Editor.UI;
using UnityEngine;

namespace ClimbGames.Eidot.UI
{
    public class ListElement : BindingElement
    {
        private Type itemType = typeof(Transform);
        private List<BindingElement> items;

        public ListElement(GameObject target, Binding binding = null) : base(target, binding)
        {
        }

        protected override BindingElement AddChild(BindingElement element)
        {
            if (items == null)
                items = new List<BindingElement>();

            // 첫번째 자식의 타입으로 결정
            if (items.Count == 0 || itemType == element.FieldType)
            {
                items.Add(element);
                itemType = element.FieldType;

                // 새로운 루트 생성
                element = new BindingElement(element.Target, schema);
            }

            return base.AddChild(element);
        }

        protected override string GetFieldTypeName()
        {
            string typeName = GetTypeName(itemType);
            return $"List<{typeName}>";
        }

        public override object GetFieldValue()
        {
            var type = typeof(List<>).MakeGenericType(itemType);
            var list = (System.Collections.IList)Activator.CreateInstance(type);

            if (items != null)
            {
                foreach (var item in items)
                {
                    var value = item.GetFieldValue();
                    if (value == null)
                    {
                        Debug.Log($"[ListElement] item value is null: {item.FieldName}");
                        continue;
                    }

                    if (itemType.IsAssignableFrom(value.GetType()))
                        list.Add(value);
                    else
                        Debug.Log($"[ListElement] can not add item: {item.FieldName}");
                }
            }

            return list;
        }
    }
}