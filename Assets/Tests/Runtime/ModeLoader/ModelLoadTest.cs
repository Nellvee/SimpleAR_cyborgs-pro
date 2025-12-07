using NUnit.Framework;
using Siccity.GLTFUtility;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using UnityGLTF;
namespace Testing.ModelLoad
{
    public class ModelLoadTest
    {

        /// <summary>
        /// Testing Siccity.GLTFUtility;
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator GLTFUtilityTest()
        {
            string url = @"https://cyborgs-pro.github.io/test/models/character.glb";

            string defaultPath = Path.Combine(Application.persistentDataPath, "Models", "loadedModel.glb");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                DownloadHandlerFile handlerFile = new DownloadHandlerFile(defaultPath);
                handlerFile.removeFileOnAbort = true;
                webRequest.downloadHandler = handlerFile;
                yield return webRequest.SendWebRequest();
                //Rename back file
                Assert.AreEqual(webRequest.result, UnityWebRequest.Result.Success);
                Debug.Log($"{defaultPath} was downloaded.");
            }
            bool imported = false;
            try
            {
                Importer.ImportGLTFAsync(defaultPath, new ImportSettings(), OnFinishAsync);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                Assert.IsTrue(imported); //create false assert
            }
            yield return new WaitUntil(() => imported);
            //Debug a few seconds to view model

            yield return new WaitForSeconds(10f);

            void OnFinishAsync(GameObject result, AnimationClip[] animations)
            {
                Debug.Log("Finished importing " + result.name);
                imported = true;
            }
        }
        /// <summary>
        /// Testing org.khronos.unitygltf
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator KhronosGLTFTest()
        {
            string url = @"https://cyborgs-pro.github.io/test/models/character.glb";

            string defaultPath = Path.Combine(Application.persistentDataPath, "Models", "loadedModel.glb");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                DownloadHandlerFile handlerFile = new DownloadHandlerFile(defaultPath);
                handlerFile.removeFileOnAbort = true;
                webRequest.downloadHandler = handlerFile;
                yield return webRequest.SendWebRequest();
                //Rename back file
                Assert.AreEqual(webRequest.result, UnityWebRequest.Result.Success);
                Debug.Log($"{defaultPath} was downloaded.");
            }
            bool imported = false;
            GameObject gameObject = new GameObject("Base Object");
            var component = gameObject.AddComponent<GLTFComponent>();
            component.loadOnStart = false;
            component.GLTFUri = defaultPath;
            //component.Collider = GLTFSceneImporter.ColliderType.Box; //doesn't work. Can't scale colliders properly.
            component.onLoadComplete = OnFinishAsync;
            yield return component.Load().AsCoroutine();

            yield return new WaitUntil(() => imported);

            //Debug a few seconds to view model

            //fix colliders
            var renderers = gameObject.GetComponentsInChildren<Renderer>().ToList();
            foreach (Renderer renderer in renderers)
            {
                renderer.gameObject.AddComponent<BoxCollider>();
            }


            yield return new WaitForSeconds(10f);

            void OnFinishAsync()
            {
                Debug.Log("Finished importing ");
                imported = true;
            }


        }
    }
}