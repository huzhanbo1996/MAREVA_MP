using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private Vector3 newSpawnObjectOffset;
    [SerializeField] private List<GameObject> objectPrefabs;
    [SerializeField] private List<GameObject> playModeObjectPrefabs;
    [SerializeField] private GameObject arragementObj;

    [System.Serializable]
    public class SerializeablePair
    {
        public GameObject signal;
        public GameObject danger;
    }
    [SerializeField] private List<SerializeablePair> mesurementTimingPair;
    [SerializeField] private List<GameObject> qrCodeObjects;
    public GameObject tmpQrDummyObject;
    private Dictionary<GameObject, float> mesurementResults = new Dictionary<GameObject, float>();

    private Dictionary<GameObject, SceneObjectProps> sceneObjects = new Dictionary<GameObject, SceneObjectProps>();
    private GameObject holdObject;
    // small trick just make manager global static
    private static SceneManager __scene = null;
    public static SceneManager GetInstance()
    {
        Debug.Assert(__scene != null);
        return __scene;
    }

    void Start()
    {
        Debug.Assert(__scene == null);
        __scene = this;
        reloadSceneObjectsIfNeeded();
        // AdjusteScenePose("position1", new Pose(tmpQrDummyObject.transform.position, tmpQrDummyObject.transform.rotation));
    }

    void Update()
    {

    }

    public void reportDroppingResult(GameObject dest, bool isPlayerEscape)
    {
        sceneObjects[dest].isPlayerEscapeDropping = isPlayerEscape;
    }

    public void reportEyeTrack(ReactionWhenPlayerNearby.REACT_TYPE type, GameObject dest)
    {
        foreach(var pair in mesurementTimingPair)
        {
            if (dest == pair.signal && !mesurementResults.ContainsKey(dest))
            {
                mesurementResults.Add(dest, Time.time);
                Debug.Log("First signal deteccted");
            }

            if (dest == pair.danger && mesurementResults.ContainsKey(pair.signal))
            {
                mesurementResults[pair.signal] = Time.time - mesurementResults[pair.signal];
                Debug.Log("Second danger deteccted, saving");
                SaveMesurementResult();
            }
        }

    }
    public void newObject(int prefabIdx)
    {
        var prefab = objectPrefabs[prefabIdx];
        var inst = GameObject.Instantiate(prefab);
        inst.transform.parent = arragementObj.transform;
        inst.transform.position = Camera.main.transform.position  
                + Camera.main.transform.forward * newSpawnObjectOffset.z 
                + Camera.main.transform.up * newSpawnObjectOffset.y
                + Camera.main.transform.right * newSpawnObjectOffset.x;
        // inst.transform.LookAt(Camera.main.transform.position);
        inst.SetActive(true);

        holdObject = inst;

        sceneObjects.Add(inst, new SceneObjectProps(prefabIdx));
        sceneObjects[inst].position = inst.transform.position;
        sceneObjects[inst].rotation = inst.transform.rotation;
        sceneObjects[inst].localScale = inst.transform.localScale;
        sceneObjects[inst].isPlayerEscapeDropping = true;
    }

    public void holdObjectDropped(Microsoft.MixedReality.Toolkit.UI.ManipulationEventData data)
    {
        var obj = data.ManipulationSource;
        Debug.Log("Grapped");
        Debug.Log(obj);
        // recursively found the parrent
        while (!sceneObjects.ContainsKey(obj) && obj.transform.parent)
        {
            obj = obj.transform.parent.gameObject;
        }

        if (obj == null)
        {
            Debug.LogError("Can't found holding object, check object construciton!");
            return;
        }

        sceneObjects[obj].position = obj.transform.position;
        sceneObjects[obj].rotation = obj.transform.rotation;
        sceneObjects[obj].localScale = obj.transform.localScale;
        sceneObjects[obj].isPlayerEscapeDropping = true;

        holdObject = obj;
    }

    public void delLastHoldingObject()
    {
        if (holdObject == null)
        {
            Debug.LogWarning("Tring to delete null");
            return;
        }
        sceneObjects.Remove(holdObject);
        Destroy(holdObject);
    }

    // #if WINDOWS_UWP
    //     Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
    //     Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
    // #endif

    // #if WINDOWS_UWP
    //     async void WriteData()
    //     {
    //         if (firstSave)
    //         {
    //             StorageFile sampleFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
    //             await FileIO.AppendTextAsync(sampleFile, saveInformation + "\r\n");
    //             firstSave = false;
    //         }
    //         else
    //         {
    //             StorageFile sampleFile = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
    //             await FileIO.AppendTextAsync(sampleFile, saveInformation + "\r\n");
    //         }
    //     }
    // #endif

    private int findLastSave()
    {
        int cnt = 0;
        for (; ; cnt++)
        {
            if (!File.Exists(Application.persistentDataPath + "/MySaveData" + cnt.ToString()))
            {
                break;
            }

        }
        return cnt;
    }
    public void saveScene()
    {
        reloadSceneObjectsIfNeeded();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/MySaveData" + findLastSave().ToString());
        var jsonStr = JsonUtility.ToJson(new SaveData(new List<SceneObjectProps>(sceneObjects.Values)));
        bf.Serialize(file, jsonStr);
        file.Close();
        Debug.Log(jsonStr);
        Debug.Log("Game data saved!");
    }

    public void reloadSceneObjectsIfNeeded()
    {
        // in editor, sceneObjects may be lost
#if UNITY_EDITOR     
        if (sceneObjects.Count == 0)
        {
            for(int i=0;i<arragementObj.transform.childCount; i++)
            {    
                var obj = arragementObj.transform.GetChild(i).gameObject;
                var originatedName = obj.name.Replace("(Clone)","").Trim();
                Debug.Log(originatedName);
                sceneObjects.Add(obj, new SceneObjectProps(objectPrefabs.FindIndex(prefab => prefab.name == originatedName)));
                sceneObjects[obj].position = obj.transform.position;
                sceneObjects[obj].rotation = obj.transform.rotation;
                sceneObjects[obj].localScale = obj.transform.localScale;
                sceneObjects[obj].isPlayerEscapeDropping = true;
            }
        }   

#endif
    }
    public void clearScene()
    {
        reloadSceneObjectsIfNeeded();
        foreach (var ins in sceneObjects.Keys)
        {
#if UNITY_EDITOR
            DestroyImmediate(ins);
#else
            Destroy(ins);
#endif
        }
        sceneObjects.Clear();
        Debug.Log("Clear scene!");
    }
    public void loadScene()
    {
        var sceneIdx = findLastSave();
        if (sceneIdx == 0)
        {
            Debug.LogWarning("No savec content");
            return;
        }

        clearScene();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/MySaveData" + (sceneIdx - 1).ToString(), FileMode.Open);
        var jsonStr = (String)bf.Deserialize(file);
        var objectList = JsonUtility.FromJson<SaveData>(jsonStr);
        Debug.Log("Game data loaded!");
        Debug.Log(jsonStr);

        foreach (var target in objectList.objPros)
        {
            var inst = GameObject.Instantiate(objectPrefabs[target.prefabIdx]);
            inst.transform.position = target.position;
            inst.transform.rotation = target.rotation;
            inst.transform.localScale = target.localScale;

            inst.transform.parent = arragementObj.transform;
            inst.SetActive(true);
            target.isPlayerEscapeDropping = true;
            sceneObjects.Add(inst, target);
        }
        Debug.Log("Rebuild scene!");
        file.Close();

    }

    private bool hasAdjusted = false;
    public void AdjusteScenePose(string qrCodeName, Pose qrCodePos)
    {
        if (hasAdjusted)
        {
            return;
        }
        
        Debug.Log("Get qccode: " + qrCodeName + " at[ " + qrCodePos.ToString() + " ]");

        if (Vector3.Distance(qrCodePos.position, gameObject.transform.position)< 0.001f )
        {
            return;
        }

        foreach(var code in qrCodeObjects)
        {
            if (code.name == qrCodeName)
            {
                Debug.Log("Coordinate in Unity is " + code.transform.position.ToString()+ " " + code.transform.rotation.ToString());
                Quaternion rotation1 = code.transform.rotation;
                Vector3 position1 = code.transform.position;
                gameObject.transform.rotation = qrCodePos.rotation * Quaternion.Inverse(rotation1) * gameObject.transform.rotation;
                gameObject.transform.position = qrCodePos.position - code.transform.position;
                // gameObject.transform.Translate(-position1, Space.World);

                // gameObject.transform.Translate(qrCodePos.position, Space.World);
                
                hasAdjusted = true;
            }
        }

        
    }

    public void SaveMesurementResult()
    {
        int cnt = 0;
        for (; ; cnt++)
        {
            if (!File.Exists(Application.persistentDataPath + "/MesureResult" + cnt.ToString()))
            {
                break;
            }

        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/MesureResult" + cnt.ToString());

        List<SceneObjectProps> propsSignal = new List<SceneObjectProps>();
        List<SceneObjectProps> propsDanger = new List<SceneObjectProps>();
        List<float> timing = new List<float>();

        foreach(var pair in mesurementTimingPair)
        {
            foreach(var mesureSignal in mesurementResults.Keys)
            {
                if (mesureSignal == pair.signal)
                {
                    Debug.Log(pair.signal);
                    Debug.Log(pair.danger);
                    propsSignal.Add(sceneObjects[pair.signal]);
                    propsDanger.Add(sceneObjects[pair.danger]);
                    timing.Add(mesurementResults[mesureSignal]);
                }               
            }
        }
        var jsonStr = JsonUtility.ToJson(new SaveMesurement(propsSignal, propsDanger, timing));
        bf.Serialize(file, jsonStr);
        file.Close();
        Debug.Log(jsonStr);
        Debug.Log("Mesurement data saved!");
    }
}
