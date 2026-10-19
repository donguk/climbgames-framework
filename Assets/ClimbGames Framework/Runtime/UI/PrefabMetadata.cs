using System;
using UnityEngine;
using ClimbGames.UI;
using System.IO;

namespace ClimbGames
{
    public class PrefabMetadata : MonoBehaviour
    {
        public string baseType;
        public int hashCode;

        public void Setup(string originPath, Type baseType)
        {
            if (File.Exists(originPath))
            {
                string originYaml = File.ReadAllText(originPath);
                hashCode = originYaml.GetHashCode();
            }

            if (typeof(UIBase).IsAssignableFrom(baseType))
                baseType = typeof(UIBase);

            this.baseType = baseType.AssemblyQualifiedName;
        }
    }
}