using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class restartButton : MonoBehaviour, buttonInterface
{
    GameObject menu;

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
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		menu.SetActive(false);

    }
    public void unhovered()
    {
        this.GetComponent<Renderer>().material.color = Color.blue;
    }
    // Start is called before the first frame update
}
