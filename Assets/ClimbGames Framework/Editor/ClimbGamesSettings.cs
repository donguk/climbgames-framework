using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;

namespace ClimbGames.Editor
{
    public interface IClimbGamesSettings
    {
        SettingsProvider CreateProvider();
        void Reset();
    }

    public abstract class ClimbGamesSettings<T> : IClimbGamesSettings where T : class, IClimbGamesSettings, new()
    {
        public static T Instance
        {
            get
            {
                return GetOrCreateIfNotExist();
            }
        }

        internal static T GetOrCreateIfNotExist()
        {
            var _instance = ClimbGamesSettings.instance.GetSettings<T>();
            if (_instance == null)
            {
                //Debug.Log($"GetOrCreateIfNotExist: {typeof(T).Name}");
                ClimbGamesSettings.instance.AddSettings(_instance = new T());
                _instance.Reset();
            }

            return _instance;
        }

        public void Save()
        {
            ClimbGamesSettings.instance.Save();
        }

        public virtual SettingsProvider CreateProvider()
        {
            string label = typeof(T).Name.TrimEnd("Settings");
            return new ClimbGamesSettingsProvider<T>($"Project/ClimbGames/{label}", SettingsScope.Project, new[] { "ClimbGames", "Framework" });
        }

        public virtual void Reset()
        {
        }
    }

    [FilePath("ProjectSettings/ClimbGamesSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ClimbGamesSettings : UnityEditor.ScriptableSingleton<ClimbGamesSettings>
    {
        [NonSerialized] internal static DateTime lastWriteTime;
        [SerializeReference] private List<IClimbGamesSettings> settings = new List<IClimbGamesSettings>();

        //
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            instance.settings.RemoveAll(x => x == null);

            var settingTypes = TypeCache.GetTypesDerivedFrom<IClimbGamesSettings>().Where(t => !t.IsAbstract && !t.IsInterface);
            foreach (var type in settingTypes)
            {
                // 1. ClimbGamesSettings<> 제네릭 타입 정의를 가져온 뒤, 현재 자식 타입(type)을 주입하여 
                //    완전한 닫힌 제네릭 타입(Closed Generic Type)을 만듭니다.
                // (주의: ClimbGamesSettings가 프로젝트 내 다른 namespace에 있다면 정해진 네임스페이스를 포함해야 합니다)
                Type genericBaseType = typeof(ClimbGamesSettings<>).MakeGenericType(type);

                // 2. 이제 해당 제네릭 부모 타입에서 CreateIfNotExist 메서드를 정확히 찾을 수 있습니다.
                var method = genericBaseType.GetMethod("GetOrCreateIfNotExist", BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                    method.Invoke(null, null);
            }

            lastWriteTime = GetLastWriteTime();
            EditorApplication.focusChanged += OnFocusChanged;
        }

        public void Save()
        {
            Save(true);

            lastWriteTime = GetLastWriteTime();
        }

        public T GetSettings<T>() where T : class, IClimbGamesSettings, new()
        {
            return (T)settings.FirstOrDefault(x => x != null && x is T);
        }

        public void AddSettings<T>(T settings) where T : class, IClimbGamesSettings, new()
        {
            T _instance = GetSettings<T>();
            if (_instance == null)
                this.settings.Add(settings);
        }

        private static DateTime GetLastWriteTime()
        {
            string filePath = Path.GetFullPath("ProjectSettings/ClimbGamesSettings.asset");
            return File.Exists(filePath) ? File.GetLastWriteTimeUtc(filePath) : DateTime.MinValue;
        }

        private static void OnFocusChanged(bool hasFocus)
        {
            if (hasFocus == false)
                return;

            var currentWriteTime = GetLastWriteTime();
            if (currentWriteTime == lastWriteTime)
                return;

            lastWriteTime = currentWriteTime;
            Reload();
        }

        public static void Reload()
        {
            //
            //Debug.Log("ClimbGamesSettings Reload...");

            var singletonType = typeof(UnityEditor.ScriptableSingleton<ClimbGamesSettings>);
            var instanceField = singletonType.GetField("s_Instance", BindingFlags.Static | BindingFlags.NonPublic);

            if (instanceField == null)
                return;

            var oldInstance = instance;

            // 기존 싱글턴 인스턴스 초기화
            instanceField.SetValue(null, null);

            if (oldInstance != null)
                UnityEngine.Object.DestroyImmediate(oldInstance);

            // 디스크 파일에서 다시 로드 //
            _ = instance;
        }

        [SettingsProviderGroup]
        public static SettingsProvider[] SettingsProvider()
        {
            return instance.settings.Select(x => x.CreateProvider()).ToArray();
        }
    }
}