using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button6 : MonoBehaviour, buttonInterface
{
    [SerializeField] GameObject inputScreen;
    updateInput visulize;


    private void Start()
    {
        visulize = inputScreen.GetComponent<updateInput>();
    }

    public string buttonName()
    {
        return "6";
    }

    public void hovered()
    {
        this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void pressed()
    {
        visulize.addnNum("6");

    }
    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }

    // Start is called before the first frame update
}