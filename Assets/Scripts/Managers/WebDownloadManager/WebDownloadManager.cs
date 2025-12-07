using Project.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Project.Managers.WebDownload
{

    public static class WebDownloadManager
    {
        public static bool IsDebug = true;
        private static Dictionary<string, UnityWebRequest> downloadDictionary = new Dictionary<string, UnityWebRequest>();

        /// <summary>
        /// Download file from <paramref name="url"/> and save data to <paramref name="filePath"/> 
        /// <br/>You can use <paramref name="downloadStatus"/> to listen download
        /// </summary>
        /// <returns></returns>
        public static IEnumerator DownloadFile(string url, string filePath, DownloadStatus downloadStatus = null, string authToken = null)
        {
            PrintLogStatic($"Trying to download data from {url}");
            //Check for null url
            if (string.IsNullOrWhiteSpace(url))
            {
                PrintErrorStatic($"Url is null or empty or has white space. \nURL: {url}");
                yield break;
            }
            if (downloadDictionary.ContainsKey(url))
            {
                PrintErrorStatic($"Can't init download! Download already in progress... \nURL: {url}");
                yield break;
            }
            if (downloadStatus != null)
            {
                downloadStatus.Start(url, filePath);
            }
            PrintLogStatic($"Downloading data from {url}");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                //renaming file that we downloading...
                //Web Download Manager Downloading File - .wdmdf
                string downloadedFilePath = GetNotExistingFileNameInPath(ChangeFileNameInPath(filePath, $"{System.IO.Path.GetFileName(filePath)}.wdmdf"));
                PrintLogStatic($"Request Temp path: {downloadedFilePath}");
                //Init Download Handler for File.
                DownloadHandlerFile handlerFile = new DownloadHandlerFile(downloadedFilePath);
                //Set remove File on Abort
                handlerFile.removeFileOnAbort = true;
                //Set Download Handler to web request
                webRequest.downloadHandler = handlerFile;
                //Wait for download
                yield return DownloadHandler(webRequest, downloadStatus, authToken);
                //Rename back file
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    //if we successfully downloaded file, delete existing
                    yield return RenameFile(downloadedFilePath, filePath, deleteExistingFile: true).AsIEnumerator();
                    PrintLogStatic($"{filePath} was downloaded.");
                }
                //Handling Result 
                ResultHandler(webRequest, downloadStatus);
            }
        }
        /// <summary>
        /// Yielding <paramref name="webRequest"/> and updating <paramref name="downloadStatus"/> 
        /// </summary>
        /// <param name="webRequest"></param>
        /// <param name="downloadStatus"></param>
        /// <returns></returns>
        private static IEnumerator DownloadHandler(UnityWebRequest webRequest, DownloadStatus downloadStatus, string authToken)
        {
            if (authToken != null)
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {authToken}");
            }
            /// <see cref="webRequest.url"/> can be changed while downloading to another uri. So we cache that url right now for add\remove dictionary functionality.
            string url = webRequest.url;
            //add to download dictionary
            downloadDictionary.Add(url, webRequest);
            //Send Request. Request is Async. Wait while it not done
            webRequest.SendWebRequest();
            //Wait for one frame to get answer from request
            yield return null;
            //Init vars
            string contentLength = "0";
            ulong allBytes = 0;
            //Update status
            if (downloadStatus != null)
            {
                downloadStatus.Url = url;
                contentLength = webRequest.GetResponseHeader("Content-Length");
                //Parse length
                ulong.TryParse(contentLength, out allBytes);
            }
            ulong lastdownloadedBytes = 0;
            ulong bytesPerSec = 0;
            float speedLessTimeout = 60;
            float speedLessTimer = 0;
            yield return WaitForRequest(webRequest,
                () =>
                {
                    if (downloadStatus != null)
                    {
                        //Get content length
                        string tempCheckContentLength = webRequest.GetResponseHeader("Content-Length");
                        //check for null or changes
                        if (tempCheckContentLength != contentLength && !string.IsNullOrEmpty(tempCheckContentLength))
                        {
                            //change content length
                            contentLength = tempCheckContentLength;
                            //Parse length
                            allBytes = ulong.Parse(contentLength);
                        }
                        //Update status with progress data
                        downloadStatus.UpdateProgress(webRequest.downloadProgress, webRequest.downloadedBytes, allBytes, bytesPerSec);
                    }
                },
                () =>
                {
                    //get current downloaded all bytes and subtruct last downloaded
                    bytesPerSec = webRequest.downloadedBytes - lastdownloadedBytes;
                    lastdownloadedBytes = webRequest.downloadedBytes;
                    //Use Timeout if bytes aren't received
                    if (speedLessTimeout > 0)
                    {
                        if (bytesPerSec != 0)
                        {
                            speedLessTimer = 0;
                        }
                        else
                        {
                            speedLessTimer++;
                            if (speedLessTimer >= speedLessTimeout)
                            {
                                webRequest.Abort();
                            }
                        }
                    }
                });
            ///Remove cached
            //Remove from dictionary
            downloadDictionary.Remove(url);
        }
        ///<summary>
        ///IEnumerator that can be called after UnityWebRequest.SendWebRequest() to wait while request is not done. Invokes actions each frame and each sec.
        ///</summary>
        public static IEnumerator WaitForRequest(UnityWebRequest sendedRequest, Action whileSendingRequestHandler, Action whileRequestActionPerSec)
        {
            float sec = 1f; //sec variable
            float timer = 0; //timer to update
            whileSendingRequestHandler?.Invoke();
            whileRequestActionPerSec?.Invoke();
            while (!sendedRequest.isDone)
            {
                yield return null;
                timer += Time.deltaTime;
                //Check if a second has passed
                if (timer >= sec)
                {
                    whileRequestActionPerSec?.Invoke();
                    timer -= sec;
                }
                whileSendingRequestHandler?.Invoke();
            }
        }
        private static void ResultHandler(UnityWebRequest webRequest, DownloadStatus status = null,
            Action<UnityWebRequest> successRequest = null)
        {
            if (status != null)
            {
                status.ErrorMessage = webRequest.error;
                status.Result = (webRequest.result);
                status.Finish();
            }
            //Check for result
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    //do not print error if we manually aborted request
                    if (!webRequest.error.Equals("Request aborted"))
                    {
                        PrintErrorStatic("Error: " + webRequest.error + $"\n{webRequest.url}");
                    }
                    else
                    {
                        PrintLogStatic("Error: " + webRequest.error + $"\n{webRequest.url}");
                    }
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    PrintErrorStatic("HTTP Error: " + webRequest.error + $"\n{webRequest.url}");
                    break;
                case UnityWebRequest.Result.Success:
                    PrintLogStatic($"Download complete" + $"\n{webRequest.url}");
                    if (successRequest != null)
                    {
                        successRequest.Invoke(webRequest);
                    }
                    break;
            }
        }
        public static async Task<bool> RenameFile(string sourcePath, string destinationPath, bool deleteExistingFile = true)
        {
            if (File.Exists(sourcePath))
            {
                if (deleteExistingFile && File.Exists(destinationPath))
                {
                    await DeleteFile(destinationPath);
                }
                File.Move(sourcePath, destinationPath);
                return true;
            }
            return false;
        }
        public static async Task DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 1, FileOptions.DeleteOnClose | FileOptions.Asynchronous))
                {
                    await stream.FlushAsync();
                }
                PrintLogStatic($"{filePath} was deleted.");
            }
        }
        public static string ChangeFileNameInPath(string filePath, string newFileName)
        {
            string directoryPath = Path.GetDirectoryName(filePath);
            return CombinePath(directoryPath, $"{newFileName}");
        }
        /// <summary>
        /// Simple increase index for path.
        /// Needs a refactor in future.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetNotExistingFileNameInPath(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);
            string directoryPath = Path.GetDirectoryName(filePath);
            string notExistingPath = CombinePath(directoryPath, $"{fileName}{fileExtension}");
            int fileId = 0;
            while (File.Exists(notExistingPath))
            {
                string newFileName = $"{fileName} - {++fileId}";
                notExistingPath = CombinePath(directoryPath, $"{newFileName}{fileExtension}");
            }
            return notExistingPath;
        }
        /// <summary>
        /// Combine Paths by triming symbols <code>'/' &amp; '\\'</code> and paste <code>'/'</code>
        /// </summary>
        /// <param name="paths"></param>
        /// <returns>
        /// String combined path
        /// </returns>
        public static string CombinePath(params string[] paths)
        {
            string combinedPath = "";
            if (paths != null && paths.Length > 0)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    //PrintLogStatic($"Combine: {combinedPath} AND {paths[i]}");
                    if (paths[i] != null)
                    {
                        combinedPath += paths[i].Trim('/', '\\');
                        if (i < paths.Length - 1)
                        {
                            combinedPath += '/';
                        }
                    }
                }
            }
            else
            {
                PrintErrorStatic($"Cannot combine paths, paths are null");
            }
            return combinedPath;
        }
        private static void PrintLogStatic(string text)
        {
            if (IsDebug)
            {
                Debug.Log(text);
            }
        }
        private static void PrintErrorStatic(string text)
        {
            if (IsDebug)
            {
                Debug.Log(text);
            }
        }

    }
}