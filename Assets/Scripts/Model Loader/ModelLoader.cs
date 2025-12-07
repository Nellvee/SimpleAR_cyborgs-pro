using System;
using System.IO;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityGLTF;
using UnityGLTF.Loader;

namespace Project.Model_Loader
{
    /// <summary>
    /// Loads model from file
    /// </summary>
    public class ModelLoader : MonoBehaviour
    {
        public event Action<GameObject> OnModelLoaded;

        public GameObject LoadedModel = null;
        private void Start()
        {
            if (ModelLoadManager.Instance != null && ModelLoadManager.Instance.DownloadedModels.Count > 0)
            {
                var model = ModelLoadManager.Instance.DownloadedModels[0];
                string path = model.FilePath;

                Debug.Log($"Loading model from: {path} = FileExists({File.Exists(path)})");
//#if UNITY_ANDROID && !UNITY_EDITOR
//                path  = "file://" + path;
//#endif
                
                GameObject gameObject = new GameObject("Base Object");
                gameObject.transform.SetParent(transform, false);


                var component = gameObject.AddComponent<GLTFComponent>();
                component.loadOnStart = false;
                component.LoadFromStreamingAssets = false;
                //component.Collider = GLTFSceneImporter.ColliderType.MeshConvex;
                component.GLTFUri = path;
                component.onLoadComplete = OnFinishAsync;
                component.Load();

                LoadedModel = gameObject;
            }
        }
        void OnFinishAsync()
        {
            Debug.Log("Finished importing ");
            LoadedModel.SetActive(false);
            OnModelLoaded?.Invoke(LoadedModel);
        }
    }
}