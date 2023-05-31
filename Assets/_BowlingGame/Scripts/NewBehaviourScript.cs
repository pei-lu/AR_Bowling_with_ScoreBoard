using QCHT.Core.Hands;
using QCHT.Interactions.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] public float dist;
    [SerializeField] public Boolean activited;
    [SerializeField] public Text timeText;
    [SerializeField] public GameObject hands;
    [SerializeField] public GameObject weightDisplay;
    public Pose tracked;
    public Pose tracked2;
    XRHandTrackingManager handData;
    arhnadTrack myHandData;
    updateInput inputInfo;

    // Start is called before the first frame update
    void Start()
    {
        //handData = hands.GetComponent<XRHandTrackingManager>();
        myHandData = hands.GetComponent<arhnadTrack>();
        inputInfo = weightDisplay.GetComponent<updateInput>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject ball = GameObject.FindWithTag("PickedBall");
        float mass;
        if (ball)
        { 
            mass = ball.GetComponent<Rigidbody>().mass; 
        }
        else
        {
            mass = 0;
        }
        tracked = myHandData._leftindexTip;
        tracked2 = myHandData._rightindexTip;
        timeText.text = myHandData.isReached.ToString();

    }
}
