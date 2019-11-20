using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneLoading
{
    enum SceneState
    {
        Unloaded,
        InProgress,
        Loaded,
        Active
    }

    public class SceneOwner : MonoBehaviour
    {
        [SerializeField]
        private List<SceneOwner> neighboringScenes;

        [SerializeField]
        private string sceneName;

        [SerializeField]
        private bool isFirst;

        private SceneState state;

        private static SceneOwner currentScene;

        public static SceneOwner CurrentScene { get => currentScene; set => currentScene = value; }
        public bool IsFirst { get => isFirst; set => isFirst = value; }
        internal SceneState State { get => state; set => state = value; }

        private void Start()
        {
            if (!isFirst) { return; }

            currentScene = this;
            state = SceneState.Active;
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
                if (state != SceneState.Unloaded) { return; }
                state = SceneState.InProgress;
                StartCoroutine(LoadScene());
            }
        }

        private IEnumerator LoadScene()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
            while (!operation.isDone)
            {
                yield return null;
            }

            state = SceneState.Loaded;
        }

        public void Unload()
        {
            if (state != SceneState.Loaded) { return; }
            
            state = SceneState.InProgress;
            StartCoroutine(UnloadScene());
            Resources.UnloadUnusedAssets();
        }

        private IEnumerator UnloadScene()
        {
            AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);
            // yield return operation;

            while (!operation.isDone)
            {
                yield return null;
            }

            state = SceneState.Unloaded;
        }

        private void OnTriggerEnter(Collider other)
        {
            Load();
            LoadNeighbors();
            currentScene = this;
            state = SceneState.Active;
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
            List<SceneOwner> scenesToIgnore = new List<SceneOwner>();
            scenesToIgnore.Add(currentScene);

            state = SceneState.Loaded;

            UnloadNeighbors(scenesToIgnore);

            if (currentScene != this) { return; }
            Unload();
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
