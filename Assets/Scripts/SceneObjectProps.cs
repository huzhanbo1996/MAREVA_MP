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
    public bool isPlayerEscapeDropping;

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

[System.Serializable]
public class SaveMesurement
{
    public List<SceneObjectProps> propsSignal = new List<SceneObjectProps>();
    public List<SceneObjectProps> propsDanger = new List<SceneObjectProps>();
    public List<float> timing = new List<float>();

    public SaveMesurement(List<SceneObjectProps> propsSignal, List<SceneObjectProps> propsDanger, List<float> timing) 
    {
        this.propsSignal = propsSignal;
        this.propsDanger = propsDanger;
        this.timing = timing;
    }
}