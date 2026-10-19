using System;
using System.Collections.Generic;
using System.IO;
using ClimbGames.UI;
using UnityEditor;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public enum PrefabConvertStatus
    {
        NotExistPrefab,
        OriginModified,
        BaseTypeChanged,
        InvalidMetadata,
        Success,
    }

    public class PrefabConvertData : FileEntry
    {
        public static readonly string SelectTypesID = "Select Types...";

        public GameObject origin;
        public string originPath;
        private int originHash;

        public GameObject prefab;
        public PrefabMetadata metadata;

        private List<Type> baseTypes;
        private List<string> baseTypeNames;
        private int baseTypeIndex;

        public List<string> BaseTypeNames => baseTypeNames;
        public Type SelectedBaseType
        {
            get
            {
                if (baseTypeIndex >= 0 && baseTypeIndex < baseTypes.Count)
                    return baseTypes[baseTypeIndex];
                return typeof(UIBase);
            }
        }

        public string SelectedBaseTypeName => SelectedBaseType.Name;

        public PrefabConvertStatus Status
        {
            get
            {
                if (prefab == null)
                    return PrefabConvertStatus.NotExistPrefab;

                if (metadata == null)
                    return PrefabConvertStatus.InvalidMetadata;

                if (metadata.baseType != SelectedBaseType.AssemblyQualifiedName)
                    return PrefabConvertStatus.BaseTypeChanged;

                if (metadata.hashCode != originHash)
                    return PrefabConvertStatus.OriginModified;

                return PrefabConvertStatus.Success;
            }
        }

        public PrefabConvertData(int id, string path) : base(id, path)
        {
            if (isDirectory == false)
            {
                originPath = path.ToUnityRelativePath();
                origin = AssetDatabase.LoadAssetAtPath<GameObject>(originPath);

                string originYaml = File.ReadAllText(originPath);
                originHash = originYaml.GetHashCode();

                baseTypes = new List<Type>() { typeof(UIBase), typeof(MonoBehaviour) };
                baseTypeNames = new List<string>() { "UIBase", "MonoBehaviour", SelectTypesID };

                Refresh();
            }
        }

        public void Refresh()
        {
            if (isDirectory == false)
            {
                string relativePath = Path.GetRelativePath(PrefabConvertSettings.OriginPath, originPath);
                string prefabPath = Path.Combine(PrefabConvertSettings.OutputPath, relativePath);

                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                    metadata = prefab.GetComponent<PrefabMetadata>();

                Type baseType = typeof(UIBase);
                if (metadata != null && string.IsNullOrEmpty(metadata.baseType) == false)
                    baseType = Type.GetType(metadata.baseType);

                SelectBaseType(baseType);
            }
        }

        public int SelectBaseType(string typeName)
        {
            return baseTypeIndex = Mathf.Max(0, baseTypes.FindIndex(x => x.Name == typeName));
        }

        public int SelectBaseType(Type type)
        {
            if (type == null)
                return baseTypeIndex;

            if (baseTypes.Contains(type) == false)
            {
                baseTypes.Add(type);
                baseTypeNames.Insert(baseTypeNames.Count - 1, type.Name);
            }

            return SelectBaseType(type.Name);
        }

        public List<string> GetSelectableBaseTypes()
        {
            if (baseTypeNames == null)
                baseTypeNames = new List<string>();

            baseTypeNames.Clear();
            foreach (var type in baseTypes)
                baseTypeNames.Add(type.Name);

            baseTypeNames.Add("Select Types...");
            return baseTypeNames;
        }
    }
}