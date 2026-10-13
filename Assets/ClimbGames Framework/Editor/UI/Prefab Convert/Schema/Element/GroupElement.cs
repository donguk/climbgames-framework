using System;
using System.Collections.Generic;
using System.Reflection;
using ClimbGames.Editor.Table;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class GroupElement : BindingElement
    {
        private string groupName;
        private GroupDefinition definition;
        private List<BindingElement> targets;

        public GroupElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        protected override void Initialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            base.Initialize(parent, schema, depth);

            groupName = binding.MatchValue.ToValidFieldName();
            definition = schema.GroupSchema.AddGroup(groupName);
        }

        protected override BindingElement AddChild(BindingElement element)
        {
            if (targets == null)
                targets = new List<BindingElement>();

            targets.Add(element);
            return element;
        }

        protected override void SearchElements(Transform transform, int depth)
        {
            base.SearchElements(transform, depth);

            // 하위 아이탬 수집 후 그룹 field 수집
            if (targets != null)
            {
                foreach (var element in targets)
                    OnRetrieveGroupField(element, definition);
            }
        }

        void OnRetrieveGroupField(BindingElement element, GroupDefinition definition)
        {
            if (element.IsValid)
            {
                // 동일 그룹인 경우 제외
                if (element is GroupElement groupElement == false || groupElement.groupName != groupName)
                    definition.AddElement(element);
            }

            var children = element.Children;
            if (children != null)
            {
                foreach (var child in children)
                    OnRetrieveGroupField(child, definition);
            }
        }

        protected override string GetFieldTypeName()
        {
            return $"{groupName}";
        }

        public override object GetFieldValue()
        {
            var type = Type.GetType($"{schema.Namespace}.{schema.ScriptName}+{groupName}, Assembly-CSharp");
            var obj = Activator.CreateInstance(type);

            var fieldInfo = type.GetField("transform", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fieldInfo.SetValue(obj, target.transform);

            if (targets != null)
            {
                foreach (var element in targets)
                    OnAssignGroupValue(element, obj);
            }

            return obj;
        }

        void OnAssignGroupValue(BindingElement element, object obj)
        {
            if (element.IsValid)
            {
                // 동일 그룹인 경우 제외
                if (element is GroupElement groupElement == false || groupElement.groupName != groupName)
                {
                    var fieldInfo = obj.GetType().GetField(element.FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (fieldInfo == null)
                        Debug.LogError($"[GroupElement] can not find field in {groupName} Group: {element.FieldName}");

                    try
                    {
                        fieldInfo?.SetValue(obj, element.GetFieldValue());
                    }
                    catch
                    {
                        //
                    }
                }
            }

            var children = element.Children;
            if (children != null)
            {
                foreach (var child in children)
                    OnAssignGroupValue(child, obj);
            }
        }
    }
}