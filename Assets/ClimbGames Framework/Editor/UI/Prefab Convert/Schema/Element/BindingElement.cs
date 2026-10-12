using System.Collections.Generic;
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
        protected BindingElement parent;
        protected int depth;
        protected List<BindingElement> children;

        public BindingElement Parent => parent;
        public virtual string FieldName { get; private set; }

        public BindingElement(GameObject target, Binding binding)
        {
            this.target = target;
            this.binding = binding;
        }

        public BindingElement(GameObject target, PrefabSchema schema)
        {
            this.target = target;
            this.schema = schema;
        }

        protected virtual void Initialize(BindingElement parent, PrefabSchema schema, int depth = 0)
        {
            this.parent = parent;
            this.schema = schema;
            this.depth = depth;

            FieldName = target.name.ToValidFieldName();
        }

        protected void AddChild(BindingElement element)
        {
            if (children == null)
                children = new List<BindingElement>();

            children.Add(element);
        }

        public void SearchElements()
        {
            SearchElements(target.transform, depth);
        }

        protected void SearchElements(Transform transform, int depth)
        {
            int count = transform.childCount;
            for (int i = 0; i < count; ++i)
            {
                var chlid = transform.GetChild(i);
                if (Binding.TryGetMatch(chlid.name, out var binding))
                {
                    var element = binding.CreateElement(chlid.gameObject);
                    element.Initialize(this, schema, depth + 1);

                    AddChild(element);
                    element.SearchElements();
                }
                else
                {
                    SearchElements(chlid, depth + 1);
                }
            }
        }

        public virtual void RetrieveGroup(GroupSchema groupSchema)
        {
            if (children != null)
            {
                foreach (var child in children)
                    child.RetrieveGroup(groupSchema);
            }
        }

        public virtual void RetrieveGroupChildren(ref List<BindingElement> list)
        {
            if (children != null)
            {
                foreach (var child in children)
                    child.RetrieveGroupChildren(ref list);
            }
        }

        public virtual void RetrieveFieldText(StringBuilder builder)
        {
            if (children != null)
            {
                foreach (var child in children)
                    child.RetrieveFieldText(builder);
            }
        }

        public virtual string GetFieldText(string indent, bool uniqueName)
        {
            return string.Empty;
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