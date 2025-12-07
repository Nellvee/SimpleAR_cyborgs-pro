using System;
using System.Collections;
using UnityEngine;

namespace Project.Managers
{
    public static class SingletonUtils
    {
        public static bool IsApplicationQuitting { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void InitializeMethod()
        {
            IsApplicationQuitting = false;
            Application.quitting += OnApplicationQuitting;
        }

        private static void OnApplicationQuitting()
        {
            IsApplicationQuitting = true;
            Application.quitting -= OnApplicationQuitting;
        }
    }
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public delegate IEnumerator InstanceRoutineHandler(T instance);

        /// <summary>
        /// Editor additional.
        /// You can disable DDOL in here from inspector, or from <see cref="DontDestoyOnLoad"/> in script.
        /// </summary>
        [SerializeField]
        private bool dontDestroyOnLoad = true;

        private static object syncLock = new object();

        public static bool DontDestoyOnLoad = true;
        public static bool CanCreateInstance = false;

        private static T instance;
        public static T Instance
        {
            get
            {
                if (SingletonUtils.IsApplicationQuitting)
                {
                    return null;
                }
                ///Check if instance is null
                if (instance == null)
                {
                    ///if null lock while creating or finding new one
                    lock (syncLock)
                    {
                        ///If after lock instance is still null, then it's first entered object in lock or instance was not created
                        if (instance == null)
                        {
                            try
                            {
                                ///Trying to find instance
                                instance = FindFirstObjectByType<T>();
                                ///If instance is still null, Creating new <see cref="GameObject" /> with this component. References on this component will be null, so be carefull
                                if (instance == null && CanCreateInstance)
                                {
                                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                                    Debug.Log($"{typeof(T).FullName}: Instance wasn't found! Created new one");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError(ex);
                            }
                        }
                    }
                }
                return instance;
            }
        }
        protected virtual void Awake()
        {
            ///If on Awake instance already not null and instance is not this object, delete this.
            if (instance != null && instance != this)
            {
                Debug.Log($"{typeof(T).FullName}: destroy " + gameObject.name);
                Destroy(this);
            }
            ///If DDOL
            if (dontDestroyOnLoad && DontDestoyOnLoad)
            {
                //Remove from parent, because child object will be destroyed when parent destroyed
                transform.parent = null;
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(this);
                }
            }
        }
    }
}