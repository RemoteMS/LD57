using UnityEngine;

namespace Helpers
{
    public static class GameObjects
    {
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (!component) component = gameObject.AddComponent<T>();

            return component;
        }

        public static T FindInterfaceInParent<T>(Transform child) where T : class
        {
            var current = child;
            while (current)
            {
                var interfaceComponent = current.GetComponent<T>();
                if (interfaceComponent != null)
                {
                    return interfaceComponent;
                }

                current = current.parent;
            }

            return null;
        }
    }
}