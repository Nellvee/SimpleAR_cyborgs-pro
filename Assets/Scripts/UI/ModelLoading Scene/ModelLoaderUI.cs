using Project.Managers;
using Project.Managers.LevelLoad_Manager;
using Project.Managers.WebDownload;
using Project.Model_Loader;
using Project.UI.ProgressBar;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scenes.ModelLoading.UI
{
    
    public class ModelLoaderUI : MonoBehaviour
    {
        [SerializeField]
        private Button loadModelButton;
        [SerializeField]
        private Button startARButton;
        [SerializeField]
        private TMP_InputField urlField;
        [SerializeField]
        private Button copyFromClippboardButton;
        [SerializeField]
        private ProgressBarUI progressBar;
        [SerializeField]
        private Button exitButton;

        private void Awake()
        {
            urlField.onValueChanged.AddListener(OnUrlFieldChanged);
            loadModelButton.onClick.AddListener(OnLoadModelButtonClicked);
            startARButton.onClick.AddListener(OnStartARButtonClicked);
            copyFromClippboardButton.onClick.AddListener(OnCopyFromClickboardButtonClicked);
            exitButton.onClick.AddListener(OnExitButtonClicked);
            //
            ValidateLoadButton(urlField.text);
            startARButton.interactable = false;
            if (progressBar != null)
            {
                progressBar.UpdateValue(0);
            }
        }


        private void OnDestroy()
        {
            urlField.onValueChanged.RemoveListener(OnUrlFieldChanged);
            loadModelButton.onClick.RemoveListener(OnLoadModelButtonClicked);
            startARButton.onClick.RemoveListener(OnStartARButtonClicked);
            copyFromClippboardButton.onClick.RemoveListener(OnCopyFromClickboardButtonClicked);
        }

        private void OnLoadModelButtonClicked()
        {
            //disable interaction when we trying to download another model.
            startARButton.interactable = false;

            if (ModelLoadManager.Instance != null)
            {
                DownloadStatus status = new();
                status.OnProgressUpdated += Status_OnProgressUpdated;
                status.OnDownloadCompleted += Status_OnDownloadCompleted;
                //we don't pass any filePath here, so each time we download model it will rewrite existed one.
                ModelLoadManager.Instance.DownloadModel(urlField.text, status: status);
            }
        }

        private void Status_OnDownloadCompleted(DownloadStatus status)
        {
            ///TODO: in future we can remove this and check if <see cref="ModelLoadManager"/> have any available downloaded models after download.
            if (status.Result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                startARButton.interactable = true;
            }
            else
            {
                startARButton.interactable = false;
            }
        }

        private void Status_OnProgressUpdated(DownloadStatus status)
        {
            if(progressBar != null)
            {
                progressBar.UpdateValue(status.Progress);
            }
        }

        private void OnStartARButtonClicked()
        {
            LevelLoadManager.Instance.LoadScene((int)SceneIndexHelper.ProjectScenes.ARScene);
        }
        private void OnCopyFromClickboardButtonClicked()
        {
            urlField.text = GUIUtility.systemCopyBuffer;
        }
        private void OnUrlFieldChanged(string value)
        {
            ValidateLoadButton(value);
        }
        private void OnExitButtonClicked()
        {
            Application.Quit();
        }

        /// <summary>
        /// Enables button if url is valid and ends on .glb or disable it if it's not.
        /// </summary>
        /// <param name="url"></param>
        private void ValidateLoadButton(string url)
        {
            bool isValidUri = Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                url.EndsWith(".glb");

            if (loadModelButton != null)
            {
                loadModelButton.interactable = isValidUri;
            }
        }
    }
}
