using System;
using System.Reflection;
using UnityEditor;

namespace ClimbGames
{
    public static class SerializedPropertyExtensions
    {
        public static object GetTargetObjectOfProperty(this SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    string elementName = element.Substring(0, element.IndexOf('['));
                    int index = Convert.ToInt32(element.Substring(element.IndexOf('[')).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Index(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Field(obj, element);
                }
            }
            return obj;
        }

        private static object GetValue_Field(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();

            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null) return field.GetValue(source);

                var prop = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop != null) return prop.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Index(object source, string name, int index)
        {
            var enumerable = GetValue_Field(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;

            int i = 0;
            foreach (var item in enumerable)
            {
                if (i == index) return item;
                i++;
            }
            return null;
        }
    }
}