using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buttonPresser : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private LayerMask buttonLayer;
    [SerializeField] private LayerMask _menuMask;
    [SerializeField] private LayerMask _launchPadMask;
    private buttonInterface button;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        if ((buttonLayer.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            Debug.Log("Hit with Layermask");
            button = other.gameObject.GetComponent<buttonInterface>();
            button.hovered();
            button.pressed();
        }
        if ((_menuMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            Debug.Log("Hit with Layermask");
            button = other.gameObject.GetComponent<buttonInterface>();
            button.hovered();
            button.pressed();
        }
        if ((_launchPadMask.value & (1 << other.transform.gameObject.layer)) > 0)
        {
            Debug.Log("Hit with Layermask");
            button = other.gameObject.GetComponent<buttonInterface>();
            button.hovered();
            button.pressed();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        button.unhovered();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
