using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class BindingElement : IUniquableName
    {
        private Binding binding;
        private BindingElement parent;
        private List<BindingElement> children;

        protected PrefabSchema Schema { get; private set; }
        public GameObject Target { get; private set; }
        public string MatchName { get; private set; }
        public string FieldName { get; private set; }

        public bool IsValid => binding != null;
        public BindingElement Parent => parent;
        public IReadOnlyList<BindingElement> Children => children;
        public virtual Type FieldType => binding?.FieldType ?? typeof(Transform);
        public int Depth { get; private set; }

        IUniquableName IUniquableName.Parent => Parent;
        public string UniqueName => parent != null ? FieldName : string.Empty;

        public BindingElement(GameObject target, Binding binding)
        {
            Target = target;
            this.binding = binding;

            MatchName = binding.MatchValue;
            FieldName = Target.name.ToValidFieldName();
        }

        public BindingElement(GameObject target, PrefabSchema schema)
        {
            Target = target;
            FieldName = Target.name.ToValidFieldName();

            Schema = schema;
        }

        public BindingElement(BindingElement other)
        {
            Target = other.Target;
            Schema = other.Schema;
            Depth = other.Depth;

            parent = other.parent;
            children = other.children;

            FieldName = Target.name.ToValidFieldName();
        }

        public BindingElement(BindingElement other, List<BindingElement> children) : this(other)
        {
            this.children = children;

            if (children != null)
            {
                foreach (var child in children)
                    child.SetParent(this);
            }
        }

        protected virtual void OnInitialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            this.parent = parent;
            Schema = schema;
            Depth = depth;
        }

        public void SetParent(BindingElement parent)
        {
            this.parent = parent;
        }


        public void SearchElements()
        {
            SearchElements(Target.transform, Depth);
        }

        protected virtual void SearchElements(Transform transform, int depth)
        {
            int count = transform.childCount;
            for (int i = 0; i < count; ++i)
            {
                var chlid = transform.GetChild(i);
                if (Binding.TryGetMatch(chlid.name, out var binding))
                {
                    var element = binding.CreateElement(chlid.gameObject);
                    element.OnInitialize(this, Schema, depth + 1);

                    element.SearchElements();
                    AddChild(element.OnBeforeAdded());
                }
                else
                {
                    SearchElements(chlid, depth + 1);
                }
            }
        }

        protected virtual BindingElement OnBeforeAdded()
        {
            return this;
        }

        protected virtual BindingElement AddChild(BindingElement element)
        {
            if (children == null)
                children = new List<BindingElement>();

            children.Add(element);
            return element;
        }

        public void RetrieveFieldText(StringBuilder builder)
        {
            if (IsValid)
            {
                if (OnRetrieveFieldText(builder) == false)
                    return;
            }

            if (children != null)
            {
                foreach (var child in children)
                    child.RetrieveFieldText(builder);
            }
        }

        protected virtual bool OnRetrieveFieldText(StringBuilder builder)
        {
            builder.AppendLine(GetFieldText());
            return true;
        }

        public string GetFieldText()
        {
            string typeName = GetFieldTypeName();
            string fieldName = Schema.GetUniqueFieldName(this);

            return $"{GetIndent(2)}[SerializeField] private {typeName} {fieldName};";
        }

        public virtual string GetFieldTypeName()
        {
            return GetTypeName(FieldType);
        }

        protected string GetTypeName(Type type)
        {
            string fullName = type.FullName;
            return fullName.TrimStart("ClimbGames.UI.", "UnityEngine.UI.", "UnityEngine.", "TMPro.", $"{Schema.Namespace}.");
        }

        protected string GetIndent(int level)
        {
            string indent = string.Empty;
            for (int i = 0; i < level; ++i)
                indent += "    ";

            return indent;
        }

        public void AssignFieldValue(object obj)
        {
            if (IsValid)
            {
                if (OnAssignFieldValue(obj) == false)
                    return;
            }

            if (children != null)
            {
                foreach (var child in children)
                    child.AssignFieldValue(obj);
            }
        }

        protected virtual bool OnAssignFieldValue(object obj)
        {
            Type type = obj.GetType();
            string fieldName = Schema.GetUniqueFieldName(this);

            var fieldInfo = type.GetField($"{fieldName}", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
                Debug.LogError($"[BindingElement] can not find field({FieldName})");

            var fieldValue = GetFieldValue();
            try
            {
                fieldInfo?.SetValue(obj, fieldValue);
            }
            catch
            {
                //
            }
            return true;
        }

        public virtual object GetFieldValue()
        {
            return Target.transform;
        }

        public void LogElements()
        {
            Debug.Log($"[{GetType().Name}] {Target.name}: depth({Depth})");
            if (children != null)
            {
                foreach (var child in children)
                    child.LogElements();
            }
        }
    }
}