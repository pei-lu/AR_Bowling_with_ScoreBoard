using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeWeight : MonoBehaviour, buttonInterface
{
    [SerializeField] GameObject keyBoard;
    [SerializeField] GameObject menu;

    private void Start()
    {
        
    }
    public string buttonName()
    {
        return "changeWeight";
    }

    public void hovered()
    {
        this.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void pressed()
    {
        //disable the quick menu, enable keybord and move it to this location.
        keyBoard.SetActive(true);
        keyBoard.transform.position = menu.transform.position;
        keyBoard.transform.rotation = menu.transform.rotation;
        menu.SetActive(false);
        
    }
    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }
    // Start is called before the first frame update
}
