using QCHT.Core.Hands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class relocate : MonoBehaviour
{
    [SerializeField] GameObject trackdata;
    private arhnadTrack refData;
    // Start is called before the first frame update
    void Start()
    {
        refData = trackdata.GetComponent<arhnadTrack>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = refData._rightindexTip.position;
    }
}
