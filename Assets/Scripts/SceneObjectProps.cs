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
    public string name;

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
public class SaveMesurementFrame
{
    public enum EVENT_TYPE {PlayerPosition, EnterArea, ExitArea, EyeOnTarget};

    public List<EVENT_TYPE> type;
    public List<SceneObjectProps> targets;
    public float timing;
    public SaveMesurementFrame(List<EVENT_TYPE> type, List<SceneObjectProps> targets, float timing) 
    {
        this.type = type;
        this.targets = targets;
        this.timing = timing;
    }
}