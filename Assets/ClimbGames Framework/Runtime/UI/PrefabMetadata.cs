using System;
using UnityEngine;
using ClimbGames.UI;
using System.IO;

namespace ClimbGames
{
    public class PrefabMetadata : MonoBehaviour
    {
        public string baseType;
        public int originHash;
        public int originLength;

        public void Setup(string originPath, Type baseType)
        {
            if (File.Exists(originPath))
            {
                string originYaml = File.ReadAllText(originPath);
                originLength = originYaml.Length;
                originHash = originYaml.GetHashCode();
            }

            this.baseType = baseType.AssemblyQualifiedName;
        }
    }
}