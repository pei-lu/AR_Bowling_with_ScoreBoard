using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinManager : MonoBehaviour
{
	[SerializeField] private Transform firstPin;
	[SerializeField] private GameObject pinPrefab;
    [SerializeField] private GameObject refField;


    private float _radius;
    private float _firstPinZ;
    private float _firstPinX;
	private float _firstPinY;
	private float _firstPinRotationX;
    private float _firstPinRotationZ;
    private List<Vector3> pinPositions = new List<Vector3>();

	const float SCALE = 0.8f;
	const float STILL_VELOCITY = 0.1f;
	void Start()
    {
		_radius = firstPin.transform.localScale.x;
		_firstPinZ = firstPin.transform.position.z;
		_firstPinX = firstPin.transform.position.x;
		_firstPinY = firstPin.transform.position.y;

        _firstPinRotationX = -90f;
        _firstPinRotationZ = 0f;
        firstPin.gameObject.SetActive(false);
        //transform.Rotate(Vector3.up, transform.parent.rotation.eulerAngles.y, Space.World);
        pinPrefab.transform.rotation = Quaternion.Euler(_firstPinRotationX, -transform.parent.rotation.y, _firstPinRotationZ);

		GeneratePins();
	}

	public void GeneratePins()
	{
		ComputePinPositions();
		InstantiatePins();
        transform.Rotate(Vector3.up, transform.parent.rotation.eulerAngles.y, Space.World);
    }

	public void ComputePinPositions()
	{
        for (int i = 0; i < 4; i++)
		{
            float rowHeadZ = _firstPinZ + _radius * i;
            float rowHeadX = _firstPinX + SCALE * _radius * i;
            for (int j = 0; j <= i; j++)
			{
                pinPositions.Add(new Vector3(rowHeadX + SCALE * _radius * i, _firstPinY, rowHeadZ - 2 * _radius * j));
            }
		}
	}

	public void InstantiatePins()
	{
		foreach (var pinPos in pinPositions)
		{
			GameObject pin = Instantiate(pinPrefab, pinPos, pinPrefab.transform.rotation);
			pin.transform.parent = transform;
		}
	}

	public bool PinsAllStill()
	{
		foreach (Transform t in transform)
		{
			if (t.gameObject.tag == "Pin" && 
				t.gameObject.GetComponent<Rigidbody>().velocity.magnitude > STILL_VELOCITY)
			{
				return false;
			}
		}
		return true;
	}

	public void ClearPins()
	{
		// Destroy gameobjects in the scene
		for (var i = gameObject.transform.childCount - 1; i >= 0; i--)
		{
			// only destroy object tagged with "Pin"
			if (gameObject.transform.GetChild(i).gameObject.tag == "Pin")
			{
				Destroy(gameObject.transform.GetChild(i).gameObject);
			}
		}
		// Clear the list
		pinPositions.Clear();
	}

	public void ResetPins()
	{
		ClearPins();
		GeneratePins();
	}
}
