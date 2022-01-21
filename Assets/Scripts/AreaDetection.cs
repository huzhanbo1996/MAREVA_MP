using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AreaDetection : MonoBehaviour
{
    public List<UnityAction<Collider>> enterEvents = new List<UnityAction<Collider>>();
    public List<UnityAction<Collider>> exitEvents = new List<UnityAction<Collider>>();
    private Collider selfCollider;
    // Start is called before the first frame update
    void Start()
    {
        selfCollider = GetComponent<Collider>();
        Debug.Assert(selfCollider != null);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Enter " + collision.gameObject.name);
        foreach (var e in enterEvents)
        {
            e.Invoke(collision);
        }
    }

    void OnTriggerExit(Collider collision)
    {
        Debug.Log("Exit " + collision.gameObject.name);
        foreach (var e in exitEvents)
        {
            e.Invoke(collision);
        }
    }
}
