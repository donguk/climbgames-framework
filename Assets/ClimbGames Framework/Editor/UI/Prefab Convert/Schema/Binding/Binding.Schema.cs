using System;
using System.Text;

namespace ClimbGames.Editor.UI
{
    public partial class Binding
    {
        [Serializable]
        public class Schema
        {
            public BindingType bindingType;
            public string prefix;
            public string typeName;
            public bool isReadOnly;

            public Schema(string prefix, Type type, BindingType bindingType, bool isReadOnly = false)
            {
                this.prefix = prefix;
                SetType(type);
                this.bindingType = bindingType;
                this.isReadOnly = isReadOnly;
            }

            public Schema(string prefix, BindingType bindingType, bool isReadOnly = false)
            {
                this.prefix = prefix;
                this.bindingType = bindingType;
                this.isReadOnly = isReadOnly;
            }

            public bool SetType(Type type)
            {
                string assemblyQualifiedName = type.AssemblyQualifiedName;
                if (typeName != assemblyQualifiedName)
                {
                    typeName = assemblyQualifiedName;
                    return true;
                }
                return false;
            }

            private static StringBuilder builder = new StringBuilder();

            public static bool IsNoneType(BindingType bindingType)
            {
                return bindingType == BindingType.None ||
                                    bindingType == BindingType.Group ||
                                    bindingType == BindingType.List;
            }

            public static string GetDisplayName(string typeName)
            {
                builder.Clear();
                string[] value = typeName.Split(',');

                int count = Math.Min(2, value.Length);
                for (int i = 0; i < count; ++i)
                {
                    if (builder.Length > 0)
                        builder.Append(",");

                    builder.Append(value[i]);
                }

                return builder.ToString();
            }
        }
    }
}