using System;
using System.Linq;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public partial class Binding
    {
        [Flags]
        public enum SchemaOption
        {
            None = 0,
            IncludeChildren = 1 << 0,
            Fallback = 1 << 1,
        }

        [Serializable]
        public abstract class Schema
        {
            public string prefix;
            [SelectTypes("Type")]
            public string typeName;
            [SelectTypes("")]
            public string[] fallback;
            public SchemaOption options;
            public bool isReadonly;

            public Schema(string prefix, Type type, Type[] fallback, SchemaOption options = SchemaOption.None, bool isReadonly = false)
            {
                this.prefix = prefix;
                typeName = type.AssemblyQualifiedName;
                this.fallback = fallback.Where(x => x != null).Select(x => x.AssemblyQualifiedName).ToArray();

                this.options = options;
                this.options |= SchemaOption.Fallback;
                this.isReadonly = isReadonly;
            }

            public Schema(string prefix, Type type, SchemaOption options = SchemaOption.None, bool isReadonly = false)
            {
                this.prefix = prefix;
                typeName = type.AssemblyQualifiedName;

                this.options = options;
                this.isReadonly = isReadonly;
            }

            public Schema(string prefix, SchemaOption options = SchemaOption.None, bool isReadonly = false)
            {
                this.prefix = prefix;
                this.options = options;
                this.isReadonly = isReadonly;
            }

            public abstract ElementProvider GetElementProvider(Binding binding);
        }

        [Serializable]
        public class ComponentSchema : Schema
        {
            public ComponentSchema(string prefix, Type type, Type[] fallback, SchemaOption options = SchemaOption.None, bool isReadonly = false)
             : base(prefix, type, fallback, options, isReadonly)
            {
            }

            public ComponentSchema(string prefix, Type type, SchemaOption options = SchemaOption.None, bool isReadonly = false)
             : base(prefix, type, options, isReadonly)
            {
            }

            public override ElementProvider GetElementProvider(Binding binding) => new ComponentElementProvider(binding);
        }

        [Serializable]
        public class ListSchema : Schema
        {
            public ListSchema(string prefix, Type type, SchemaOption options = SchemaOption.None, bool isReadonly = false) : base(prefix, type, options, isReadonly)
            {
            }

            public override ElementProvider GetElementProvider(Binding binding) => new ListElementProvider(binding);
        }

        [Serializable]
        public class GroupSchema : Schema
        {
            public GroupSchema(string prefix, SchemaOption options = SchemaOption.None, bool isReadonly = false) : base(prefix, options, isReadonly)
            {
            }

            public override ElementProvider GetElementProvider(Binding binding) => new GroupElementProvider(binding);
        }
    }
}