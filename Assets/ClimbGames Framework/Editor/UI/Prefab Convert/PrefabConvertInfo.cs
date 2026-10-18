using System;
using System.Collections.Generic;
using System.IO;
using ClimbGames.UI;
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

    public class PrefabConvertInfo
    {
        public static readonly string SelectTypesID = "Select Types...";

        public FileEntry entry;
        private int originHash;
        private int originLength;

        public GameObject prefab;
        public PrefabMetadata metadata;

        private int selectedTypeIndex;
        private List<Type> baseTypes;
        private List<string> baseTypeNames;

        public List<string> BaseTypeNames => baseTypeNames;
        public Type SelectedBaseType
        {
            get
            {
                if (selectedTypeIndex >= 0 && selectedTypeIndex < baseTypes.Count)
                    return baseTypes[selectedTypeIndex];
                return typeof(UIBase);
            }
        }

        public string SelectedBaseTypeName => SelectedBaseType.Name;

        public PrefabConvertStatus Status { get; private set; }

        public PrefabConvertInfo(FileEntry entry, GameObject prefab)
        {
            this.entry = entry;

            string originYaml = File.ReadAllText(entry.path.ToUnityRelativePath());
            originLength = originYaml.Length;
            originHash = originYaml.GetHashCode();

            baseTypes = new List<Type>() { typeof(UIBase), typeof(MonoBehaviour) };
            baseTypeNames = new List<string>() { "UIBase", "MonoBehaviour", SelectTypesID };

            Setup(prefab);
        }

        public void Setup(GameObject prefab)
        {
            this.prefab = prefab;

            if (prefab != null)
                metadata = prefab.GetComponent<PrefabMetadata>();

            Type baseType = typeof(UIBase);
            if (metadata != null)
            {
                if (string.IsNullOrEmpty(metadata.baseType) == false)
                    baseType = Type.GetType(metadata.baseType);
            }

            SelectBaseType(baseType);
            Status = GetStatus();
        }

        PrefabConvertStatus GetStatus()
        {
            if (prefab == null)
                return PrefabConvertStatus.NotExistPrefab;

            if (metadata == null)
                return PrefabConvertStatus.InvalidMetadata;

            if (metadata.baseType != SelectedBaseType.AssemblyQualifiedName)
                return PrefabConvertStatus.BaseTypeChanged;

            if (metadata.originHash != originHash)
                return PrefabConvertStatus.OriginModified;

            return PrefabConvertStatus.Success;
        }

        public int SelectBaseType(string typeName)
        {
            selectedTypeIndex = baseTypes.FindIndex(x => x.Name == typeName);
            selectedTypeIndex = Mathf.Max(0, selectedTypeIndex);

            Status = GetStatus();
            return selectedTypeIndex;
        }

        public int SelectBaseType(Type type)
        {
            if (type == null)
                return selectedTypeIndex;

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