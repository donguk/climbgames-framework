using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ClimbGames.Editor.Table;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class BindingElement
    {
        protected PrefabSchema schema;
        protected GameObject target;
        protected Binding binding;

        private BindingElement parent;
        private List<BindingElement> children;
        private int depth;

        public GameObject Target => target;
        public BindingElement Parent => parent;
        public IReadOnlyList<BindingElement> Children => children;
        public string FieldName { get; protected set; }
        public Type FieldType => binding != null ? binding.FieldType : typeof(Transform);
        public bool IsValid => binding != null;

        private BindingElement(GameObject target)
        {
            this.target = target;
        }

        public BindingElement(GameObject target, Binding binding) : this(target)
        {
            this.binding = binding;
        }

        public BindingElement(GameObject target, PrefabSchema schema) : this(target)
        {
            this.schema = schema;
        }

        protected virtual void Initialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            this.parent = parent;
            this.schema = schema;
            this.depth = depth;

            FieldName = target.name.ToValidFieldName();
        }

        protected virtual BindingElement AddChild(BindingElement element)
        {
            if (children == null)
                children = new List<BindingElement>();

            children.Add(element);
            return element;
        }

        public void SearchElements()
        {
            SearchElements(target.transform, depth);
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
                    element.Initialize(this, schema, depth + 1);

                    element = AddChild(element);
                    element.SearchElements();
                }
                else
                {
                    SearchElements(chlid, depth + 1);
                }
            }
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
            string fieldName = schema.GetUniqueFieldName(this);

            return $"{GetIndent(2)}[SerializeField] private {typeName} {fieldName};";
        }

        public string GetGroupFieldText()
        {
            string typeName = GetFieldTypeName();
            string fieldName = FieldName;

            return $"{GetIndent(3)}public {typeName} {fieldName};";
        }

        protected virtual string GetFieldTypeName()
        {
            return GetTypeName(FieldType);
        }

        protected string GetTypeName(Type type)
        {
            string fullName = type.FullName;
            return fullName.TrimStart("ClimbGames.UI.", "UnityEngine.UI.", "UnityEngine.", "TMPro.", $"{schema.Namespace}.");
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
            string fieldName = schema.GetUniqueFieldName(this);

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
            return target.transform;
        }

        public void LogElements()
        {
            Debug.Log($"[{GetType().Name}] {target.name}: depth({depth})");

            if (children != null)
            {
                foreach (var child in children)
                    child.LogElements();
            }
        }
    }
}