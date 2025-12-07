using Project.Managers;
using Project.Managers.LevelLoad_Manager;
using Project.Model_Loader;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace Project.Scenes.SimpleAR._UI
{
    public class SimpleARSceneControlsUI : MonoBehaviour
    {
        [SerializeField]
        private Button clearAllSpawnedObjectsButton;
        [SerializeField]
        private Button returnButton;
        [SerializeField]
        private ObjectSpawnerFromModel objectSpawner;

        private void Awake()
        {
            clearAllSpawnedObjectsButton.onClick.AddListener(OnClearAllSpawnedButtonClicked);
            returnButton.onClick.AddListener(OnReturnButtonClicked);
        }

        private void OnReturnButtonClicked()
        {
            LevelLoadManager.Instance.LoadScene((int)SceneIndexHelper.ProjectScenes.ModelLoader);
        }

        private void OnClearAllSpawnedButtonClicked()
        {
            if (objectSpawner != null)
            {
                objectSpawner.ClearAll();
            }
        }
    }
}