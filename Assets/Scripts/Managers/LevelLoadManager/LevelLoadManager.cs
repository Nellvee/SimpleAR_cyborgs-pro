using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
namespace Project.Managers.LevelLoad_Manager
{
    public class LevelLoadManager : Singleton<LevelLoadManager>
    {
        #region Events
        public delegate IEnumerator LoadEventHandler();
        /// <summary>
        /// Event invokes before async Scene Load method will be invoked and before Loading Canvas enabled
        /// </summary>
        public event LoadEventHandler LoadStartingEvent;
        /// <summary>
        /// Event invokes after async Scene Load method will be invoked, after loading canvas enabled
        /// </summary>
        public event LoadEventHandler LoadStartedEvent;
        /// <summary>
        /// Event invokes after Scene loaded, before loading canvas disabled
        /// </summary>
        public event LoadEventHandler LoadEndingEvent;
        /// <summary>
        /// Event invokes after Scene loaded, after loading canvas disabled
        /// </summary>
        public event LoadEventHandler LoadEndedEvent;
        public event LoadEventHandler LoadingObjectEnabledEvent;
        public event LoadEventHandler LoadingObjectDisabledEvent;

        private IEnumerator OnLoadStartingEvent()
        {
            yield return LoadEventCoroutineHandler(LoadStartingEvent);
        }
        private IEnumerator OnLoadStartedEvent()
        {
            yield return LoadEventCoroutineHandler(LoadStartedEvent);
        }
        private IEnumerator OnLoadEndingEvent()
        {
            yield return LoadEventCoroutineHandler(LoadEndingEvent);
        }
        private IEnumerator OnLoadEndedEvent()
        {
            yield return LoadEventCoroutineHandler(LoadEndedEvent);
        }
        private IEnumerator OnLoadingObjectEnabledEvent()
        {
            yield return LoadEventCoroutineHandler(LoadingObjectEnabledEvent);
        }
        private IEnumerator OnLoadingObjectDisabledEvent()
        {
            yield return LoadEventCoroutineHandler(LoadingObjectDisabledEvent);
        }
        private IEnumerator LoadEventCoroutineHandler(LoadEventHandler handler)
        {
            if (handler != null)
            {
                foreach (LoadEventHandler listHandler in handler.GetInvocationList())
                {
                    yield return listHandler.Invoke();
                }
            }
        }
        #endregion
        #region Private Variables
        [Header("Loading")]
        [Tooltip("Game object that will be activated when the level will be loading")]
        [SerializeField]
        private GameObject loadingGameObject = null;
        [SerializeField]
        private bool useLoadingFillImage = false;
        /// <summary>
        /// TODO: remove UI from here
        /// </summary>
        [Tooltip("Image that will be filled with loading progress")]
        [SerializeField]
        private Image[] loadingFillImages = null;
        [Header("Loading Debug")]
        [SerializeField]
        private bool debugMode = false;
        //[ShowIf("debugMode", true)]
        [Tooltip("If true, debugMode will be diactivated in build")]
        [SerializeField]
        private bool disableDebugModeInBuild = true;
        //[ShowIf("debugMode", true)]
        [Tooltip("After loading scene, Coroutine wait for this time before disable loading object")]
        [SerializeField]
        private float additionalLoadingTime = 5f;
        //Loading swticher. Used for check that Enable\Disable loading go will be once
        private bool loadingGameObjectWasEnabled = false;
        //Coroutine for current loading
        private Coroutine loadingRoutine = null;
        //Variable for getting loading time 
        private float loadingTime;
        #endregion
        #region Public Variables
        public bool IsLoading { get; set; } = false;
        /// <summary>
        /// Return last loading time.
        /// </summary>
        /// <remarks>
        /// Set After <see cref="LoadEndingEvent"/>, <see cref="LoadingObjectDisabledEvent"/> - Before <see cref="LoadEndedEvent"/>
        /// </remarks>
        public float LastLoadingTime { get; private set; }
        #endregion
        #region Unity methods
        protected override void Awake()
        {
            base.Awake();
            PrintLog("Disable Loading GO");
            //Disable Loading Go
            if (loadingGameObject != null)
                loadingGameObject.SetActive(false);
            //Disable debug mode, if we are not in the editor
            if (disableDebugModeInBuild && !Application.isEditor)
            {
                debugMode = false;
                PrintLog("Disable Debug Mode");
            }
        }
        #endregion
        #region Loading methods
        /// <summary>
        /// Load scene by scene index with timeToWait seconds
        /// </summary>
        /// <param name="sceneBuildIndex"></param>
        /// <param name="timeToWait"></param>
        public void LoadScene(int sceneBuildIndex, UnityEngine.SceneManagement.LoadSceneMode loadMode = UnityEngine.SceneManagement.LoadSceneMode.Single, float timeToWait = 0f)
        {
            Debug.Log($"Load Scene: {sceneBuildIndex}");
            ResetLoadingRoutine();
            loadingRoutine = StartCoroutine(LoadSceneRoutine(sceneBuildIndex, loadMode, timeToWait));
        }
        /// <summary>
        /// Load scene by scene index with timeToWait seconds
        /// </summary>
        /// <param name="sceneBuildIndex"></param>
        /// <param name="timeToWait"></param>
        public IEnumerator LoadSceneCoroutine(int sceneBuildIndex, UnityEngine.SceneManagement.LoadSceneMode loadMode = UnityEngine.SceneManagement.LoadSceneMode.Single, float timeToWait = 0f)
        {
            ResetLoadingRoutine();
            loadingRoutine = StartCoroutine(LoadSceneRoutine(sceneBuildIndex, loadMode, timeToWait));
            yield return loadingRoutine;
        }
        /// <summary>
        /// Unloads Scene Async in Coroutine
        /// </summary>
        /// <param name="sceneBuildIndex"></param>
        /// <returns></returns>
        private IEnumerator UnloadSceneAsyncRoutine(int sceneBuildIndex)
        {
            PrintLog($"Start to async unload scene");
            AsyncOperation async = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneBuildIndex);
            if (async != null)
            {
                while (!async.isDone)
                {
                    yield return null;
                }
            }
            PrintLog($"Scene was unloaded!");
        }
        /// <summary>
        /// Loading Scene Routine that Handles Loading and Events
        /// </summary>
        /// <param name="sceneBuildIndex"></param>
        /// <param name="timeToWait"></param>
        /// <returns></returns>
        private IEnumerator LoadSceneRoutine(int sceneBuildIndex, UnityEngine.SceneManagement.LoadSceneMode loadMode = UnityEngine.SceneManagement.LoadSceneMode.Single, float timeToWait = 0)
        {
            PrintLog($"Starting Loading Scene.. Time to Wait: {timeToWait}");
            //Handle wait Time
            if (timeToWait > 0)
                yield return new WaitForSeconds(timeToWait);
            //Set Loading 
            IsLoading = true;
            //Enabling loading go
            yield return EnableLoadingObject();
            //Call before loading scene event
            yield return OnLoadStartingEvent();
            PrintLog($"Start async load scene");
            //Loading scene
            AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBuildIndex, loadMode);
            //Wait for event after start loading scene
            Coroutine onLoadStartedCoroutine = StartCoroutine(OnLoadStartedEvent());
            //Wait for scene to load
            while (!async.isDone)
            {
                //Get Progress
                float progress = async.progress / .9f; // / .9f;
                SetFillImagesProgress(progress);
                //yield
                PrintLog($"Scene is loading... {progress * 100f} %");
                yield return null;
            }
            //Wait for end of frame
            yield return null;
            //Wait for started Coroutine ended
            yield return onLoadStartedCoroutine;
            //Debug Wait
            if (debugMode)
            {
                PrintLog($"Debug Mode! Wait For {additionalLoadingTime} seconds.");
                yield return new WaitForSeconds(additionalLoadingTime);
            }
            //Wait for ending event - before loading object will be disabled
            yield return OnLoadEndingEvent();
            //Disabling Loading Go
            yield return DisableLoadingObject();
            //Reset Loading
            IsLoading = false;
            //Reset Routine
            loadingRoutine = null;
            PrintLog($"Scene was loaded. Loaded Time: {LastLoadingTime} sec. Call and wait for OnLoadEndedEvent");
            //Wait for Ended event - after loading object was disabled
            yield return OnLoadEndedEvent();
        }
        #endregion
        #region Enable\Disable Loading Game Object
        /// <summary>
        /// Enable all GO for Loading Screen
        /// </summary>
        /// <remarks>
        /// You can enable loading earlier, but to disable you need to load scene.
        /// </remarks>
        public IEnumerator EnableLoadingObject()
        {
            if (loadingGameObjectWasEnabled) yield break;
            loadingGameObjectWasEnabled = true;
            //Get time when load started
            loadingTime = Time.time;
            //Modify Loading Object
            SetFillImagesProgress(0f);

            if (loadingGameObject != null)
            {
                loadingGameObject.SetActive(true);
            }
            PrintLog("Loading Game Object was Enabled. Call and wait for OnLoadingObjectEnabledEvent");
            //wait for event to complete
            yield return OnLoadingObjectEnabledEvent();
        }

        /// <summary>
        /// Disabling all GO for LS
        /// </summary>
        public IEnumerator DisableLoadingObject()
        {
            if (!loadingGameObjectWasEnabled) yield break;
            loadingGameObjectWasEnabled = false;
            //Modify Loading Object
            if (loadingGameObject != null && loadingGameObject.activeSelf)
            {
                loadingGameObject.SetActive(false);
            }
            PrintLog("Loading Game Object Disabled. Call and wait for OnLoadingObjectDisabledEvent");
            //Wait for event to complete
            yield return OnLoadingObjectDisabledEvent();
            //Get load time
            loadingTime = Time.time - loadingTime;
            //Set last loading time
            LastLoadingTime = loadingTime;
        }
        #endregion
        #region Private methods
        /// <summary>
        /// Reset already activated routine. Just for Check
        /// </summary>
        private void ResetLoadingRoutine()
        {
            if (loadingRoutine != null)
            {
                PrintLog("Loading Routine Was Stopped, Be Aware of this");
                StopCoroutine(loadingRoutine);
                loadingRoutine = null;
            }
        }
        /// <summary>
        /// setting fill images progress
        /// </summary>
        /// <param name="progress"></param>
        private void SetFillImagesProgress(float progress)
        {
            //Set Fill Image
            if (useLoadingFillImage && loadingFillImages != null)
            {
                foreach (Image fillImage in loadingFillImages)
                {
                    if (fillImage != null)
                    {
                        fillImage.fillAmount = progress;
                    }
                }
            }
        }
        #endregion

        private void PrintLog(string message)
        {
            Debug.Log(message);
        }
    }
}