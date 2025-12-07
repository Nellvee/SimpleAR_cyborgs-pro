using Project.Managers.WebDownload;
using System;
using System.Collections;

namespace Project.Model_Loader
{
    /// <summary>
    /// Downloads model from url
    /// </summary>
    public class ModelDownloader
    {
        public delegate void ModelDownloadCompletedHandler(ModelDownloader modelDownloader, bool resultSuccess);
        public event ModelDownloadCompletedHandler OnDownloadCompleted;

        private string filePath;

        public string FilePath { get => filePath; }

        public ModelDownloader(string filePath)
        {
            SetFilePath(filePath);
        }

        public void SetFilePath(string filePath)
        {
            this.filePath = filePath;
        }
        public IEnumerator DownloadModelFromUrlCoroutine(string url, DownloadStatus status = null)
        {
            status ??= new();
            yield return WebDownloadManager.DownloadFile(url, FilePath, status);
            OnDownloadCompleted?.Invoke(this, status.Result == UnityEngine.Networking.UnityWebRequest.Result.Success);
        }
    }
}