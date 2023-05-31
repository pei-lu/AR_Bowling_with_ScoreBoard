using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class leftButtonPresser : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private LayerMask buttonLayer;
    private buttonInterface button;
    //public GameObject buttonColided;
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
