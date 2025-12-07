using Project.Managers;
using Project.Managers.WebDownload;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Project.Model_Loader
{
    /// <summary>
    /// Singleton for quick access to loaded models from all scenes
    /// <br/>
    /// 1. We can use static, but require another MonoBehaviour for Coroutines. 
    /// 
    /// </summary>
    public class ModelLoadManager : Singleton<ModelLoadManager>
    {
        ///TODO: add check for already downloaded models and create save system to store already downloaded paths
        public List<ModelDownloader> DownloadedModels = new();

        public void DownloadModel(string url, string filePath = null, DownloadStatus status = null)
        {
            if(filePath == null)
            {
                ///TODO: refactor for multiple files
                ///while we can download only one model at a time, it's ok.
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                string defaultPath = Path.Combine(Application.streamingAssetsPath, "Models", "loadedModel.glb");
#else
                string defaultPath = Path.Combine(Application.persistentDataPath, "Models", "loadedModel.glb");
#endif
                filePath = defaultPath;
            }

            Debug.Log($"Downloading model to: {filePath}");
            
            //if (!DownloadedModels.Any(dm => dm.FilePath == filePath))
            {
                ModelDownloader modelDownloader = new ModelDownloader(filePath);
                modelDownloader.OnDownloadCompleted += ModelDownloader_OnDownloadCompleted;
                StartCoroutine(modelDownloader.DownloadModelFromUrlCoroutine(url, status));
            }
        }

        private void ModelDownloader_OnDownloadCompleted(ModelDownloader modelDownloader, bool resultSuccess)
        {
            if (resultSuccess)
            {
                DownloadedModels.Add(modelDownloader);
            }
            modelDownloader.OnDownloadCompleted -= ModelDownloader_OnDownloadCompleted;
        }
    }
}