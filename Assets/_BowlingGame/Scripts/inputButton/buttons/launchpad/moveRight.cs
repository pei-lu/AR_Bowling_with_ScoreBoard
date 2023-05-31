using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveRight : MonoBehaviour, buttonInterface
{
    [HideInInspector] public bool isPressed;

    // Start is called before the first frame update
    void Start()
    {
        //subscribe to the function

    }

    // Update is called once per frame
    void Update()
    {

    }
    public string buttonName()
    {
        return "launch";
    }

    public void hovered()
    {
        isPressed = false;
        this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void pressed()
    {
        // call function
        isPressed = true;
		throw new System.NotImplementedException();
    }

    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }


}
