using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneLoading
{
    public class SceneOwner : MonoBehaviour
    {
        [SerializeField]
        private List<SceneOwner> neighboringScenes;

        [SerializeField]
        private string sceneName;

        [SerializeField]
        private bool isFirst;

        private static SceneOwner currentScene;

        public static SceneOwner CurrentScene { get => currentScene; set => currentScene = value; }
        public bool IsFirst { get => isFirst; set => isFirst = value; }

        private void Start()
        {
            if (!isFirst) { return; }

            currentScene = this;
        }

        public void Load()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                string pathToScene = EditorSceneManager.GetSceneByName(sceneName).path;
                EditorSceneManager.OpenScene(pathToScene, OpenSceneMode.Additive);
            }
            else
            {
                if (SceneManager.GetSceneByName(sceneName).isLoaded) { return; }
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
        }

        public void Unload()
        {
            if (!SceneManager.GetSceneByName(sceneName).isLoaded) { return; }
            StartCoroutine(UnloadScene());
            Resources.UnloadUnusedAssets();
        }

        private IEnumerator UnloadScene()
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
            yield return operation;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Enter");
            LoadNeighbors();
            currentScene = this;
        }

        public void LoadNeighbors()
        {
            foreach (SceneOwner sceneOwner in neighboringScenes)
            {
                sceneOwner.Load();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exit");
            List<SceneOwner> scenesToIgnore = new List<SceneOwner>();
            scenesToIgnore.Add(currentScene);

            UnloadNeighbors(scenesToIgnore);
        }

        public void UnloadNeighbors(List<SceneOwner> sceneOwnersToIgnore)
        {
            List<SceneOwner> scenesToUnload = new List<SceneOwner>(neighboringScenes);

            foreach (SceneOwner ignoredScene in sceneOwnersToIgnore)
            {
                scenesToUnload.Remove(ignoredScene);
            }

            foreach (SceneOwner sceneToUndload in scenesToUnload)
            {
                sceneToUndload.Unload();
            }
        }

        public void UnloadNeighbors()
        {
            UnloadNeighbors(new List<SceneOwner>());
        }

        public void AddNeibgor(SceneOwner newSceneOwner)
        {
            if (neighboringScenes.Contains(newSceneOwner)) { return; }

            neighboringScenes.Add(newSceneOwner);
        }

#if UNITY_EDITOR
        public void AddToNeibgors()
        {
            foreach (SceneOwner neighbor in neighboringScenes)
            {
                neighbor.AddNeibgor(this);
            }
        }
#endif

    }
}
