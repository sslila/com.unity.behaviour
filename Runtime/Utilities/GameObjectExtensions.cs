using UnityEngine;

namespace Unity.Behavior
{
    /// <summary>
    /// Useful methods for GameObjects.
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Gets or adds a component to a GameObject.
        /// </summary>
        /// <param name="gameObject"> the gameObject targeted</param>
        /// <typeparam name="T"> The type of the component required</typeparam>
        /// <returns>The requested component type, either already on the GameObject or created and added within this method.</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
                return null;

            var result = gameObject.GetComponent<T>();
            if (result == null)
                result = gameObject.AddComponent<T>();
            
            return result;
        }
    }
}