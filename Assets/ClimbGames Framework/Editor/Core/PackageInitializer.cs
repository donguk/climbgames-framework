using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor.PackageManager;

namespace ClimbGames.Editor
{
    public static class PackageInitializer
    {
        [Serializable]
        private class Manifest
        {
            public Dictionary<string, string> dependencies;
            public ScopedRegistry[] scopedRegistries;
        }

        [Serializable]
        private class ScopedRegistry
        {
            public string name;
            public string url;
            public string[] scopes;
        }

        private const string RegistryName = "package.openupm.com";
        private const string RegistryUrl = "https://package.openupm.com";

        private static readonly string[] RequiredScopes =
        {
            "com.cysharp.unitask",
            "com.github.exceldatareader.exceldatareader"
        };

        private static readonly string ManifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "manifest.json");

        [MenuItem("Tools/ClimbGames/Resolve Dependencies")]
        public static void RegisterRegistry()
        {
            if (File.Exists(ManifestPath) == false)
                return;

            try
            {
                string json = File.ReadAllText(ManifestPath);
                var manifest = JsonConvert.DeserializeObject<Manifest>(json);
                //var manifest = JsonUtility.FromJson<Manifest>(json);

                if (manifest == null)
                    return;

                manifest.scopedRegistries ??= Array.Empty<ScopedRegistry>();

                var registries = manifest.scopedRegistries.ToList();

                // URL이 같은 Registry를 우선 찾는다.
                var registry = registries.FirstOrDefault(x => string.Equals(
                                                                x.url,
                                                                RegistryUrl,
                                                                StringComparison.OrdinalIgnoreCase));

                if (registry == null)
                {
                    registry = new ScopedRegistry
                    {
                        name = RegistryName,
                        url = RegistryUrl,
                        scopes = Array.Empty<string>()
                    };

                    registries.Add(registry);
                }

                var scopes = registry.scopes?.ToList()
                             ?? new System.Collections.Generic.List<string>();

                bool changed = false;

                foreach (string scope in RequiredScopes)
                {
                    if (scopes.Contains(scope))
                        continue;

                    scopes.Add(scope);
                    changed = true;
                }

                if (registry.scopes == null)
                    changed = true;

                registry.scopes = scopes.ToArray();
                manifest.scopedRegistries = registries.ToArray();

                // Registry가 새로 추가된 경우에도 저장한다.
                if (!changed && registries.Count == manifest.scopedRegistries.Length)
                    return;

                json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                json += Environment.NewLine;

                File.WriteAllText(ManifestPath, json);
                Client.Resolve();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClimbGames] Registry 설정에 실패했습니다.\n{e}");
            }
        }
    }
}