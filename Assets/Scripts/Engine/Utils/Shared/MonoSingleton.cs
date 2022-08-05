using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : Component
{
    #region Fields

    /// <summary>
    /// The instance.
    /// </summary>
    private static T instance;
    private static bool quitting;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            // if (quitting) Debug.Log("Quitting, why request?");
            if (instance == null && !quitting) {
                instance = FindObjectOfType<T>();
                if (instance == null) {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    #endregion

    #region Methods

    void OnApplicationQuit () => quitting = true;
    /// <summary>
    /// Use this for initialization.
    /// </summary>
    protected virtual void Awake ()
    {
        if (instance == null) {
            instance = this as T;
            // DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);
        }
    }

    #endregion
}