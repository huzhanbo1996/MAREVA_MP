using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionWhenPlayerNearby : MonoBehaviour
{
    public enum REACT_TYPE { Panel, SoundFixed, SoundFollowing, DroppingStuff }
    public bool isPlaying;
    public REACT_TYPE type;
    public GameObject movingPanel;
    public GameObject shatteredPieaces;
    public Vector3 panelOffset;

    public float audioAlertInterval;

    private List<AreaDetection> shatteredDetectors = new List<AreaDetection>();
    private AudioSource audioAlert;
    private float audioTimer;
    private bool audioPlay;
    private AreaDetection detector;
    // Start is called before the first frame update
    void Start()
    {
        detector = GetComponentInChildren<AreaDetection>();
        Debug.Assert(detector != null);
        detector.enterEvents.Add(OnEnter);
        detector.exitEvents.Add(OnExit);

        switch(type)
        {
            case REACT_TYPE.Panel:
                // if (isPlaying) 
                // {
                //     movingPanel.SetActive(false);
                // }
                break;
            case REACT_TYPE.SoundFixed:
            case REACT_TYPE.SoundFollowing:
                audioAlert = GetComponentInChildren<AudioSource>();
                audioPlay = false;
                audioTimer = audioAlertInterval;
                Debug.Assert(audioAlert != null);
                break;
            case REACT_TYPE.DroppingStuff:
                if (isPlaying)
                {
                    movingPanel.SetActive(false);
                }
                shatteredDetectors = new List<AreaDetection>(shatteredPieaces.gameObject.GetComponentsInChildren<AreaDetection>(includeInactive: true));
                Debug.Log("Found shttered pieces with AreaDetectors of " + shatteredDetectors.Count.ToString());
                foreach(var detector in shatteredDetectors)
                {
                    detector.enterEvents.Add(OnShatterPieceEnter);
                }
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
    // Update is called once per frame
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
            switch(type)
            {
                case REACT_TYPE.Panel:
                    movingPanel.SetActive(true);
                    break;
                case REACT_TYPE.SoundFixed:
                case REACT_TYPE.SoundFollowing:
                    if (!isPlaying) 
                    {
                        movingPanel.SetActive(true);
                    }
                    audioPlay = true;
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
            switch(type)
            {
                case REACT_TYPE.Panel:
                    movingPanel.SetActive(false);
                    break;
                case REACT_TYPE.SoundFixed:
                case REACT_TYPE.SoundFollowing:
                    movingPanel.SetActive(false);
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
