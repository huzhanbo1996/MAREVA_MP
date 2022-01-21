using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneObjectProps
{
    public int prefabIdx;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;

    public SceneObjectProps(int idx)
    {
        this.prefabIdx = idx;
    }
}

[System.Serializable]
public class SaveData
{
    public List<SceneObjectProps> objPros = new List<SceneObjectProps>();
    public SaveData(List<SceneObjectProps> obj)
    {
        objPros = obj;
    }
}