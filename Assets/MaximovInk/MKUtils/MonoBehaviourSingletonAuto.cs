using UnityEngine;

public class MonoBehaviourSingletonAuto<T> : MonoBehaviour
	where T : Component
{
	private static T _instance;
	public static T Instance
	{
		get
		{
			if (_instance == null)
			{
				var instances = FindObjectsByType(typeof(T), FindObjectsSortMode.None) as T[];

				if (instances is { Length: > 0 })
					_instance = instances[0];

				if (instances is { Length: > 1 })
				{
					Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
				}

                if (_instance != null) return _instance;

                var obj = new GameObject();
                _instance = obj.AddComponent<T>();
            }
			return _instance;
		}
	}
}


