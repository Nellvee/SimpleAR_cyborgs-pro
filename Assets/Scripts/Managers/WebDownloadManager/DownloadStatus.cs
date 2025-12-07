using System;
using UnityEngine.Networking;

namespace Project.Managers.WebDownload
{
    public class DownloadStatus
    {
        public event Action<DownloadStatus> OnProgressUpdated;
        public event Action<DownloadStatus> OnDownloadCompleted;

        public string Url;
        public string FilePath;
        public string ErrorMessage;
        public UnityWebRequest.Result Result;

        public float Progress;

        public void Start(string url, string filePath)
        {
            this.Url = url;
            this.FilePath = filePath;
            ErrorMessage = string.Empty;
            Result = UnityWebRequest.Result.InProgress;
        }

        public void UpdateProgress(float downloadProgress, ulong downloadedBytes, ulong allBytes, ulong bytesPerSec)
        {
            Progress = downloadProgress;
            OnProgressUpdated?.Invoke(this);
        }

        internal void Finish()
        {
            OnDownloadCompleted?.Invoke(this);
        }
    }
}