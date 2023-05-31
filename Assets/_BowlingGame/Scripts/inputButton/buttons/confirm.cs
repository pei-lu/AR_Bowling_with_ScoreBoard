using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class confirm : MonoBehaviour, buttonInterface
{
    public GameObject inputRecorder;
    updateInput inputUpdator;
    [SerializeField] private GameObject keyboard;
    [SerializeField] private BallManager ballManager;
    [SerializeField] private changeModeButton modeManager;

    private void Start()
    {
        inputUpdator = inputRecorder.GetComponent<updateInput>();
    } 
    public string buttonName()
    {
        return "confirm";
    }

    public void hovered()
    {
        this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void pressed()
    {
        //send float to the weight manager, clear the input box
        inputUpdator.inputWeight = "0";
        if (modeManager.gameMode == "LaunchPad")
        {
            ballManager.GenerateCustomizedBallLane(inputUpdator.weight);
		}
        else if (modeManager.gameMode == "HandThrow")
        {
            ballManager.GenerateCustomizedBallsMachine(inputUpdator.weight);
        }
		inputUpdator.weight = 0f;

        //TODO generate customorized balls(inputUpdator.weight
        //)
        keyboard.SetActive(false);
    }
    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }
    // Start is called before the first frame update
}
