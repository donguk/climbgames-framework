using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClimbGames.Core
{
    public static class GameObjectExtensions
    {
        public static T FindComponentInRootObjects<T>(this Scene scene, bool findInDeactiveObject = true)
            where T : Component
        {
            if (scene.isLoaded)
            {
                using var scope = new ListPoolScope<GameObject>(out var gameObjects);

                scene.GetRootGameObjects(gameObjects);
                foreach (var gameObject in gameObjects)
                {
                    if (!findInDeactiveObject && !gameObject.activeInHierarchy)
                        continue;

                    T component = gameObject.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }

            return null;
        }

        public static GameObject FindGameObjectInRootObjects(this Scene scene, string name, bool findInDeactiveObject = true)
        {
            if (scene.isLoaded)
            {
                using var scope = new ListPoolScope<GameObject>(out var gameObjects);

                scene.GetRootGameObjects(gameObjects);
                foreach (var gameObject in gameObjects)
                {
                    if (!findInDeactiveObject && !gameObject.activeInHierarchy)
                        continue;

                    if (gameObject.name == name)
                        return gameObject;
                }
            }

            return null;
        }

        public static void FindAll(this Transform transform, Func<Transform, bool> predicate, IList<Transform> result)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                if (predicate.Invoke(child))
                    result.Add(child);
            }
        }

        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            if (go == null) return;

            go.layer = layer;

            foreach (Transform child in go.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, layer);
                }
            }
        }

        public static Transform FindChildByName(this Transform transform, string name)
        {
            return FindGameObjectRecursively(transform, name);
        }

        static Transform FindGameObjectRecursively(Transform transform, string name)
        {
            if (transform.name == name)
                return transform;

            foreach (Transform child in transform)
            {
                Transform target = FindGameObjectRecursively(child, name);
                if (target == null)
                    continue;
                return target;
            }
            return null;
        }

        public static T FindComponentInChildren<T>(this GameObject go, string name) where T : Component
        {
            if (go == null)
                return null;

            Transform child = go.transform.FindChildByName(name);
            if (child != null)
                return child.GetComponent<T>();

            return null;
        }
    }
}