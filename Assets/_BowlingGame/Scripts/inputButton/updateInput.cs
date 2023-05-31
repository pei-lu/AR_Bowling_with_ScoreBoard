using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class updateInput : MonoBehaviour
{
    [SerializeField] public Text inputDisplay;
     
    public string inputWeight;
    public float weight;

// Start is called before the first frame update
    void Start()
    {
        inputWeight = "";
    }

    // Update is called once per frame
    void Update()
    {
        inputDisplay.text = weight.ToString();
    }

    public void addnNum(string Num)
    {
        if(inputWeight.Length < 6 ) {
        inputWeight += Num;
        weight = float.Parse(inputWeight);
        }
        
    }
    public void removenNum() {
        if(inputWeight.Length == 1)
        {
            inputWeight = "";
            weight = 0f;
        }
        inputWeight = inputWeight.Remove(inputWeight.Length - 1, 1);
        weight = float.Parse(inputWeight);
    }
}
