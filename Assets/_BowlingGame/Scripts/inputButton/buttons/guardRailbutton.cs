using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class guardRailbutton : MonoBehaviour, buttonInterface
{
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject guardRail;

    private void Start()
    {
        menu = this.transform.parent.gameObject;
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
		guardRail.SetActive(!guardRail.activeSelf);

    }
    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }
    // Start is called before the first frame update
}
 