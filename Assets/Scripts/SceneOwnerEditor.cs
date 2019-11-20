using UnityEngine;
using UnityEditor;

namespace SceneLoading
{
#if UNITY_EDITOR
    [CustomEditor(typeof(SceneOwner))]
    public class SceneOwnerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SceneOwner sceneOwner = target as SceneOwner;

            if (GUILayout.Button("AddToNeigbhors"))
            {
                sceneOwner.AddToNeibgors();
            }

            if (GUILayout.Button("SetAsFirst"))
            {
                SceneOwner[] scenesToReset = FindObjectsOfType<SceneOwner>();
                foreach (SceneOwner scene in scenesToReset)
                {
                    scene.IsFirst = false;
                }
                SceneOwner.CurrentScene = sceneOwner;
                sceneOwner.IsFirst = true;
                sceneOwner.Load();
            }
        }
    }
#endif
}