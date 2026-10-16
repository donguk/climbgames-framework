using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class GroupElement : BindingElement
    {
        private string groupName;
        private GroupDefinition definition;
        private BindingElement virtualRoot;
        private List<BindingElement> targetElements;

        public override Type FieldType
        {
            get
            {
                var type = Type.GetType($"{Schema.Namespace}.{Schema.ScriptName}+{groupName}, Assembly-CSharp");
                return type ?? base.FieldType;
            }
        }

        public GroupElement(GameObject target, Binding binding) : base(target, binding)
        {
        }

        protected override void OnInitialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            base.OnInitialize(parent, schema, depth);

            virtualRoot = new BindingElement(Target, schema);
            groupName = MatchName.ToValidFieldName();
            definition = schema.GroupSchema.AddGroup(groupName);
        }

        protected override BindingElement OnBeforeAdded()
        {
            // 동일한 그룹의 상위 element가 열려 있는 경우, 포함 시키기 위해 일반 element 로 변환
            if (definition.Depth > 0)
            {
                var element = new BindingElement(this, targetElements);
                return element;
            }

            return base.OnBeforeAdded();
        }

        protected override BindingElement AddChild(BindingElement element)
        {
            if (targetElements == null)
                targetElements = new List<BindingElement>();

            element.SetParent(virtualRoot);
            targetElements.Add(element);

            return element;
        }

        protected override void SearchElements(Transform transform, int depth)
        {
            definition.Open();
            base.SearchElements(transform, depth);

            // 최상위 group 의 탐색이 끝난 경우
            if (definition.Depth <= 1)
            {
                // 하위 아이탬 field 수집
                if (targetElements != null)
                {
                    foreach (var element in targetElements)
                        OnRetrieveGroupField(element, definition);
                }
            }
            definition.Close();
        }

        void OnRetrieveGroupField(BindingElement element, GroupDefinition definition)
        {
            if (element.IsValid)
                definition.AddElement(element);

            var children = element.Children;
            if (children != null)
            {
                foreach (var child in children)
                    OnRetrieveGroupField(child, definition);
            }
        }

        public override string GetFieldTypeName()
        {
            return $"{groupName}";
        }

        public override object GetFieldValue()
        {
            var type = FieldType;
            var obj = Activator.CreateInstance(type);

            // root transform 할당
            var fieldInfo = type.GetField("transform", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            fieldInfo.SetValue(obj, Target.transform);

            definition.ClearAndCheckAssignment();
            if (targetElements != null)
            {
                foreach (var element in targetElements)
                    OnAssignGroupValue(element, obj);
            }

            var notAssignedElements = definition.NotAssignedElements;
            if (notAssignedElements.Count > 0)
            {
                string text = string.Empty;
                foreach (var element in notAssignedElements)
                {
                    if (text.Length > 0)
                        text += ", ";
                    text += $"{element.fieldName}";
                }

                Debug.LogError($"[GroupElement] {FieldName} Group has not assigned fields: {text}");
            }

            return obj;
        }

        void OnAssignGroupValue(BindingElement element, object obj)
        {
            if (element.IsValid)
            {
                string fieldName = definition.GetUniqueFieldName(element);
                var fieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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

            var children = element.Children;
            if (children != null)
            {
                foreach (var child in children)
                    OnAssignGroupValue(child, obj);
            }
        }
    }
}