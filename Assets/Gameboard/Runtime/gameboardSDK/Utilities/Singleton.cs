using UnityEngine;

namespace Gameboard.Utilities
{
    public abstract class Singleton<T> : Singleton where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (Quitting)
                {
                    GameboardLogging.Warning($"[{nameof(Singleton)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                    return null;
                }

                // Add an instance if none exist, find any older instances that exist and remove them
                lock (Lock)
                {
                    if (instance != null)
                        return instance;
                    var instances = FindObjectsOfType<T>();
                    var count = instances.Length;
                    if (count > 0)
                    {
                        if (count == 1)
                            return instance = instances[0];
                        GameboardLogging.Warning(
                            $"[{nameof(Singleton)}<{typeof(T)}>] There should never be more than one {nameof(Singleton)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                        for (var i = 0; i < instances.Length - 1; i++)
                            Destroy(instances[i].gameObject);
                        return instance = instances[instances.Length - 1];
                    }

                    GameboardLogging.Verbose(
                        $"[{nameof(Singleton)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
                    return instance = new GameObject($"({nameof(Singleton)}){typeof(T)}")
                        .AddComponent<T>();
                }
            }
        }

        private static T instance;

        private static readonly object Lock = new object();

        /// <summary>
        /// Should be set to true if the singleton should be retained between scenes, this is used to set DontDestroyOnLoad
        /// </summary>
        [SerializeField] private bool persistent = true;

        private void Awake()
        {
            var instances = FindObjectsOfType<T>();
            var count = instances.Length;
            if (count > 1)
            {
                GameboardLogging.Warning(
                $"[{nameof(Singleton)}<{typeof(T)}>] There should never be more than one {nameof(Singleton)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");

                if (instance != this)
                {
                    DestroyImmediate(this.gameObject);
                    return;
                }
            }

            if (count == 1)
                instance = instances[0];

            if (persistent)
                DontDestroyOnLoad(gameObject);

            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }
    }

    public abstract class Singleton : MonoBehaviour
    {
        public static bool Quitting { get; private set; }

        private void OnApplicationQuit()
        {
            Quitting = true;
        }
    }
}