// This abstract class SingletonManager<T> is a generic singleton pattern implementation for Unity MonoBehaviour classes.
// It ensures that only one instance of the manager exists in the scene and provides a global access point to it.

using UnityEngine;

public abstract class SingletonManager<T> : MonoBehaviour where T : MonoBehaviour
{
    // This method is called when the script instance is being loaded.
    // It ensures that only one instance of the manager exists in the scene.
    protected virtual void Awake()
    {
        T[] managers = FindObjectsByType<T>(FindObjectsSortMode.None);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    // This static method provides a global access point to the singleton instance of the manager.
    // It finds the manager by its tag or creates a new one if it doesn't exist.
    public static T Get()
    {
        var tag = typeof(T).Name;
        GameObject managerObject = GameObject.FindWithTag(tag);
        if (managerObject != null)
        {
            return managerObject.GetComponent<T>();
        }
        GameObject go = new(tag);
        go.tag = tag;
        return go.AddComponent<T>();
    }
}