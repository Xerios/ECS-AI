using UnityEngine;


public class SingletonScritableObject<T> : ScriptableObject where T : ScriptableObject {
    private static T _instance;

    public static T Instance {
        get {
            if (!_instance) _instance = Resources.Load<T>(typeof(T).Name);
            if (!_instance) _instance = ScriptableObject.CreateInstance<T>();
            
            return _instance;
        }
    }

}