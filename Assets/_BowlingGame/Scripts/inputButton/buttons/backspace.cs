using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backspace : MonoBehaviour, buttonInterface
{
    [SerializeField] GameObject inputScreen;
    updateInput visulize;


    private void Start()
    {
        visulize = inputScreen.GetComponent<updateInput>();
    }

    public string buttonName()
    {
        return "back space";
    }

    public void hovered()
    {
        this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void pressed()
    {
        visulize.removenNum();
    }
    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }
}
