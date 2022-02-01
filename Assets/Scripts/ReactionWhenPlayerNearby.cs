using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionWhenPlayerNearby : MonoBehaviour
{
    public enum REACT_TYPE { Panel, PanelFixed, SoundFixed, SoundFollowing, DroppingStuff, Obstacle, PicPenguin, PicPenguinRandom }
    public bool isPlaying;
    public REACT_TYPE type;
    public GameObject movingPanel;
    public GameObject shatteredPieaces;
    public Vector3 panelOffset;

    public float audioAlertInterval;
    public bool showPenguin;

    private List<AreaDetection> shatteredDetectors = new List<AreaDetection>();
    private AudioSource audioAlert;
    private float audioTimer;
    private bool audioPlay;
    private AreaDetection detector;
    void Start()
    {
        detector = GetComponentInChildren<AreaDetection>();
        Debug.Assert(detector != null);
        detector.enterEvents.Add(OnEnter);
        detector.exitEvents.Add(OnExit);

        switch(type)
        {
            case REACT_TYPE.Panel:
                if (isPlaying) 
                {
                    movingPanel.SetActive(false);
                }
                break;
            case REACT_TYPE.SoundFixed:
            case REACT_TYPE.SoundFollowing:
                audioAlert = GetComponentInChildren<AudioSource>();
                audioPlay = false;
                audioTimer = audioAlertInterval;
                Debug.Assert(audioAlert != null);
                Debug.Log(gameObject);
                break;
            case REACT_TYPE.DroppingStuff:
                shatteredDetectors = new List<AreaDetection>(shatteredPieaces.gameObject.GetComponentsInChildren<AreaDetection>(includeInactive: true));
                Debug.Log("Found shttered pieces with AreaDetectors of " + shatteredDetectors.Count.ToString());
                foreach(var detector in shatteredDetectors)
                {
                    detector.enterEvents.Add(OnShatterPieceEnter);
                }
                break;
            case REACT_TYPE.PicPenguinRandom:
                float translate = Random.Range(-0.5f, 0.5f) * gameObject.transform.localScale.z;
                GetComponentInChildren<AreaDetection>().gameObject.transform.Translate(new Vector3(0f,0f,translate), Space.Self);
                break;
            default:
                break;
        }
    }

    private void MovePanelForawd()
    {
        if (isPlaying)
        {
            movingPanel.transform.position = Camera.main.transform.position 
                + Camera.main.transform.forward * panelOffset.z 
                + Camera.main.transform.up * panelOffset.y
                + Camera.main.transform.right * panelOffset.x;
            movingPanel.transform.LookAt(Camera.main.transform.position);
            movingPanel.transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }
    private void RepeatlyPlayAlart()
    {
        
        audioTimer += Time.deltaTime;
        if (audioTimer > audioAlertInterval) 
        {
            audioTimer -= audioAlertInterval;
            audioAlert.Play();
        }
    }
    void Update()
    {
        switch(type)
        {
            case REACT_TYPE.Panel:
                if (movingPanel.activeSelf)
                {
                    MovePanelForawd();
                }
                break;
            case REACT_TYPE.SoundFixed:
                if (audioPlay) 
                {
                    RepeatlyPlayAlart();
                }
                break;
            case REACT_TYPE.SoundFollowing:
                if (audioPlay)
                {
                    MovePanelForawd();
                    RepeatlyPlayAlart();
                }
                break;
            default:
                break;
        }
    }

    private void OnEnter(Collider other)
    {
        Debug.Log("OnEnter");
        if(other.gameObject.tag == "MainCamera")
        {
            Debug.Log("On Player Enter!");
            SceneManager.GetInstance().reportAreaEnter(gameObject);
            if (showPenguin)
            {
                SceneManager.GetInstance().BlinkPenguin();
            }
            switch(type)
            {
                case REACT_TYPE.Panel:
                    movingPanel.SetActive(true);
                    Debug.Log("trigger " + gameObject.ToString());
                    break;
                case REACT_TYPE.SoundFixed:
                case REACT_TYPE.SoundFollowing:
                    audioPlay = true;
                    Debug.Log("trigger " + gameObject.ToString());
                    break;
                case REACT_TYPE.DroppingStuff:
                    shatteredPieaces.SetActive(true);
                    break;
                default:
                    break;
            }
        }

    }

    private void OnExit(Collider other)
    {
        
        Debug.Log("OnExit");
        if(other.gameObject.tag == "MainCamera")
        {
            Debug.Log("On Player Exit!");
            SceneManager.GetInstance().reportAreaExit(gameObject);
            switch(type)
            {
                case REACT_TYPE.Panel:
                    movingPanel.SetActive(false);
                    break;
                case REACT_TYPE.SoundFixed:
                case REACT_TYPE.SoundFollowing:
                    audioPlay = false;
                    break;
                default:
                    break;
            }
        }

    }
    private void OnShatterPieceEnter(Collider other)
    {
        if (other.gameObject.tag == "MainCamera")
        {
            SceneManager.GetInstance().reportDroppingResult(gameObject, false);
            Debug.Log("shattered object hit player!");
        }
    }
}
