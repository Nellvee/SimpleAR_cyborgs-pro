using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
namespace Project.Model_Loader
{
    /// <summary>
    /// Rewrites default ObjectSpawner list of objects to newly loaded model
    /// </summary>
    [RequireComponent(typeof(ObjectSpawner))]
    public class ObjectSpawnerFromModel : MonoBehaviour
    {
        [SerializeField]
        private ModelLoader modelLoader;
        [SerializeField]
        private SpawnBaseObject spawnBaseObject;

        private ObjectSpawner spawner;
        private SpawnBaseObject instantiatedBaseObject = null;
        private List<GameObject> spawnedObjects = new();

        private void Awake()
        {
            spawner = GetComponent<ObjectSpawner>();
            spawner.objectPrefabs ??= new List<GameObject>();
            spawner.objectSpawned += Spawner_objectSpawned;
        }
        private void OnDestroy()
        {
            if (spawner != null)
            {
                spawner.objectSpawned -= Spawner_objectSpawned;
            }
        }

        private void Start()
        {
            if(modelLoader != null)
            {
                if(modelLoader.LoadedModel == null)
                {
                    modelLoader.OnModelLoaded += ModelLoader_OnModelLoaded;
                }
                else
                {
                    ModelLoader_OnModelLoaded(modelLoader.LoadedModel);
                }    
            }
        }

        private void ModelLoader_OnModelLoaded(GameObject modelBase)
        {
            SpawnBaseObject baseObject = Instantiate(spawnBaseObject, transform);
            baseObject.InitializeModelVisuals(modelBase);
            baseObject.gameObject.SetActive(false);

            modelBase.transform.SetParent(baseObject.transform, false);
            modelBase.SetActive(true);

            instantiatedBaseObject = baseObject;
            spawner.objectPrefabs.RemoveAll(x => x == null);
            spawner.objectPrefabs.Add(instantiatedBaseObject.gameObject);;

        }
        private void Spawner_objectSpawned(GameObject baseObject)
        {
            baseObject.gameObject.SetActive(true);
            spawnedObjects.Add(baseObject);
        }
        public void ClearAll()
        {
            foreach(GameObject obj in spawnedObjects)
            {
                Destroy(obj);
            }
            spawnedObjects.Clear();
        }
        public void TestSpawn()
        {
            Debug.Log("Spawn clicked");
        }
    }
}