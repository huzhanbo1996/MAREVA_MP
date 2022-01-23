using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportEyeTrackEvent : MonoBehaviour
{
    public ReactionWhenPlayerNearby.REACT_TYPE type;
    public void OnEyeTrackStart()
    {
        SceneManager.GetInstance().reportEyeTrack(type, gameObject);
        Debug.Log("eye on " + gameObject.ToString());
    }
}
