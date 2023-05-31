using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeModeButton : MonoBehaviour, buttonInterface
{
    [SerializeField] private GameObject launchPad;
    [SerializeField] private BallManager ballManager;
    [SerializeField] private GameObject menu;
    public string gameMode;

    private void Start()
    {
        gameMode = "LaunchPad";
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
        launchPad.SetActive(!launchPad.activeSelf);
        if (launchPad.activeSelf )
        {
            gameMode = "LaunchPad";
		}
        else
        {
            gameMode = "HandThrow";
		}
        ballManager.ClearAllBall();
        menu.SetActive(false);

    }
    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }
    // Start is called before the first frame update
}
