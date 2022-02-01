using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.MixedReality.Toolkit.Input;

public class SceneManager : MonoBehaviour
{
    [SerializeField] private Vector3 newSpawnObjectOffset;
    [SerializeField] private List<GameObject> objectPrefabs;
    [SerializeField] private List<GameObject> playModeObjectPrefabs;
    [SerializeField] private GameObject arragementObj;
    public GameObject tmpQrDummyObject;
    public bool isPalyerMode;
    [SerializeField] private GameObject penguinPicture;
    [SerializeField] private GazeProvider provider;
    private List<GameObject> detectableObjects = new List<GameObject>();
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

        int cnt = 0;
        for (; ; cnt++)
        {
            if (!File.Exists(Application.persistentDataPath + "/MesureResult" + cnt.ToString()))
            {
                break;
            }
        }
        saveMesurementFileName = Application.persistentDataPath + "/MesureResult" + cnt.ToString();
        using(StreamWriter sw = File.AppendText(saveMesurementFileName))
        {
            sw.Write("{\"joueur\":[");
        }
    }

    private string saveMesurementFileName;
    private SaveMesurementFrame saveMesurementFrame;
    private List<SaveMesurementFrame.EVENT_TYPE> currFrameTypes = new List<SaveMesurementFrame.EVENT_TYPE>();
    private List<SceneObjectProps> currFrameSceneObjectProps = new List<SceneObjectProps>();
    
    private float logInterval = 0.5f;
    private float timeAccum = 0.0f;
    void Update()
    {
        if (provider.HitInfo.collider !=null)
        {
            Debug.Log("Eye Hit something");
            foreach (var det in detectableObjects)
            {
                foreach(var col in det.GetComponentsInChildren<Collider>())
                {
                    if (col == provider.HitInfo.collider)
                    {
                        Debug.Log("Eye Hit target :" + det.ToString());
                        reportEyeTrack(ReactionWhenPlayerNearby.REACT_TYPE.Panel, det);
                    }
                }

            }
        }
        if (currFrameTypes.Count == 0 && currFrameSceneObjectProps.Count == 0 && timeAccum < logInterval)
        {
            timeAccum += Time.deltaTime;
            return;
        }
        if (timeAccum >= logInterval)
        {
            timeAccum -= logInterval;
        }
        var playerProps = new SceneObjectProps(-1);
        playerProps.position = Camera.main.transform.position;
        playerProps.rotation = Camera.main.transform.rotation;
        playerProps.localScale = Vector3.one;
        playerProps.name = "player";

        currFrameTypes.Add(SaveMesurementFrame.EVENT_TYPE.PlayerPosition);
        currFrameSceneObjectProps.Add(playerProps);
        saveMesurementFrame = new SaveMesurementFrame(currFrameTypes, currFrameSceneObjectProps,Time.time);

        SaveMesurementResult();

        currFrameTypes.Clear();
        currFrameSceneObjectProps.Clear();
    }

    public void reportDroppingResult(GameObject dest, bool isPlayerEscape)
    {
        sceneObjects[dest].isPlayerEscapeDropping = isPlayerEscape;
    }

    public void reportEyeTrack(ReactionWhenPlayerNearby.REACT_TYPE type, GameObject dest)
    {
        currFrameTypes.Add(SaveMesurementFrame.EVENT_TYPE.EyeOnTarget);
        currFrameSceneObjectProps.Add(sceneObjects[dest]);
        Debug.Log("report eye on " + gameObject.ToString());
    }

    public void reportAreaEnter(GameObject dest)
    {
        currFrameTypes.Add(SaveMesurementFrame.EVENT_TYPE.EnterArea);
        currFrameSceneObjectProps.Add(sceneObjects[dest]);
    }
    public void reportAreaExit(GameObject dest)
    {
        currFrameTypes.Add(SaveMesurementFrame.EVENT_TYPE.ExitArea);
        currFrameSceneObjectProps.Add(sceneObjects[dest]);
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
        inst.SetActive(true);

        holdObject = inst;

        sceneObjects.Add(inst, new SceneObjectProps(prefabIdx));
        sceneObjects[inst].position = inst.transform.position;
        sceneObjects[inst].rotation = inst.transform.rotation;
        sceneObjects[inst].localScale = inst.transform.localScale;
        sceneObjects[inst].isPlayerEscapeDropping = true;
        sceneObjects[inst].name = inst.name;
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
        sceneObjects[obj].name = obj.name;

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
        sceneObjects.Clear();
        gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
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
        if (sceneObjects.Count == 0)
        {
            for(int i=0;i<arragementObj.transform.childCount; i++)
            {    
                var obj = arragementObj.transform.GetChild(i).gameObject;
                var originatedName = obj.name.Replace("(Clone)","").Trim();
                if (originatedName[originatedName.Length -1]>='0' && originatedName[originatedName.Length -1]<='9' )
                {
                    originatedName = originatedName.Substring(0, originatedName.Length - 1);
                }
                Debug.Log(originatedName);
                sceneObjects.Add(obj, new SceneObjectProps(objectPrefabs.FindIndex(prefab => prefab.name == originatedName)));
                sceneObjects[obj].position = obj.transform.position;
                sceneObjects[obj].rotation = obj.transform.rotation;
                sceneObjects[obj].localScale = obj.transform.localScale;
                sceneObjects[obj].isPlayerEscapeDropping = true;
                sceneObjects[obj].name = obj.transform.name;
                if (obj.GetComponent<ReportEyeTrackEvent>()!=null)
                {
                    detectableObjects.Add(obj);
                }
            }
        }   
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
        gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/MySaveData" + (sceneIdx - 1).ToString(), FileMode.Open);
        var jsonStr = (String)bf.Deserialize(file);
        var objectList = JsonUtility.FromJson<SaveData>(jsonStr);
        Debug.Log("Game data loaded!");
        Debug.Log(jsonStr);

        foreach (var target in objectList.objPros)
        {
            GameObject inst;
            if (!isPalyerMode)
            {
                inst = GameObject.Instantiate(objectPrefabs[target.prefabIdx]);
            } else 
            {
                inst = GameObject.Instantiate(playModeObjectPrefabs[target.prefabIdx]);
            }
            inst.transform.position = target.position;
            inst.transform.rotation = target.rotation;
            inst.transform.localScale = target.localScale;
            inst.name = target.name;

            inst.transform.parent = arragementObj.transform;
            inst.SetActive(true);
            target.isPlayerEscapeDropping = true;
            sceneObjects.Add(inst, target);
        }
        Debug.Log("Rebuild scene!");
        file.Close();

    }
    public void ForceReloadSceneProps()
    {
        sceneObjects.Clear();
        for(int i=0;i<arragementObj.transform.childCount; i++)
        {    
            var obj = arragementObj.transform.GetChild(i).gameObject;
            var originatedName = obj.name.Replace("(Clone)","").Trim();
            if (originatedName[originatedName.Length -1]>='0' && originatedName[originatedName.Length -1]<='9' )
            {
                originatedName = originatedName.Substring(0, originatedName.Length - 1);
            }
            Debug.Log(originatedName);
            sceneObjects.Add(obj, new SceneObjectProps(objectPrefabs.FindIndex(prefab => prefab.name == originatedName)));
            sceneObjects[obj].position = obj.transform.position;
            sceneObjects[obj].rotation = obj.transform.rotation;
            sceneObjects[obj].localScale = obj.transform.localScale;
            sceneObjects[obj].isPlayerEscapeDropping = true;
            sceneObjects[obj].name = obj.name;
        }
    }

    private bool enableAdjusted = true;
    private bool hasAdjusted = false;
    public void ToggleAdjust()
    {
        enableAdjusted = !enableAdjusted;
        hasAdjusted = false;
    }
    private List<string> usedQrCode = new List<string>();
    public void AdjusteScenePose(string qrCodeName, Pose qrCodePos)
    {
        Debug.Log("Get qccode: " + qrCodeName + " at[ " + qrCodePos.ToString() + " ]");
        if (usedQrCode.Contains(qrCodeName))
        {
            return;
        }
        
        Debug.Log("New qr code");

        foreach(var code in sceneObjects.Keys)
        {
            if (code.name == qrCodeName)
            {
                arragementObj.SetActive(false);
                arragementObj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                Debug.Log("Coordinate in Unity is " + code.transform.position.ToString()+ " " + code.transform.rotation.ToString());
                Quaternion rotation1 = code.transform.rotation;
                Vector3 position1 = code.transform.position;
                arragementObj.transform.rotation = qrCodePos.rotation * Quaternion.Inverse(rotation1) * arragementObj.transform.rotation;
                arragementObj.transform.position = qrCodePos.position - code.transform.position;
                arragementObj.SetActive(true);

                Debug.Log("Coordinate adjusted :" + code.transform.position.ToString()+ " " + code.transform.rotation.ToString());
                usedQrCode.Add(code.name);
            }
        }
    }

    private static int mesureCount = 0; 
    public void SaveMesurementResult()
    {
        
        BinaryFormatter bf = new BinaryFormatter();
        var jsonStr = JsonUtility.ToJson(saveMesurementFrame);
        using(StreamWriter sw = File.AppendText(saveMesurementFileName))
        {
            sw.Write(jsonStr);
            sw.WriteLine(",");
        }
        mesureCount++;
        Debug.Log("Mesurement data saved!");
    }

    public void BlinkPenguin()
    {
        StartCoroutine(blinkCoroutine());
    }

    private bool blinking = false;
    IEnumerator blinkCoroutine()
    {
        if (blinking)
        {
            yield break;
        }
        blinking = true;

        penguinPicture.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        penguinPicture.SetActive(false);
        
        blinking = false;
        yield return null;
    }
}
