
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneManager))]
public class EditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneManager myScript = (SceneManager)target;
        if(GUILayout.Button("Read Scene"))
        {
            myScript.loadScene();
        }

        if(GUILayout.Button("Clear Scene"))
        {
            myScript.clearScene();
        }

        if(GUILayout.Button("Save Scene"))
        {
            myScript.saveScene();
        }

        if(GUILayout.Button("Force reload"))
        {
            myScript.ForceReloadSceneProps();
        }
        
    }
}
#endif