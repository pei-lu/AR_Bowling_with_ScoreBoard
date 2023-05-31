using QCHT.Core.Hands;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BallManager : MonoBehaviour
{
	[Range(1, 5)]
	[SerializeField] private int maxLeftBallNumber = 5;
	[Range(3, 10)]
    [SerializeField] private int generateTimeInterval = 5;
	[Range(0.1f, 1f)]
	[SerializeField] private float initialSpeed = 0.5f;
	[SerializeField] private Transform ballSpawnPointMachine;
	[SerializeField] private Transform ballSpawnPointLane;
	[SerializeField] private List<GameObject> ballPrefabs;
	[SerializeField] private arhnadTrack hand;

    private int curLeftBallNumber;
    private float timeSpent;
	private float leftGenerateOffset = 0.07f;
	private float rightGenerateOffset = -0.07f;
	private const float LB_TO_KG = 0.453592f;

	void Start()
    {
		curLeftBallNumber = 0;
		timeSpent = 0;
    }

	void FixedUpdate()
	{
		GenerateDefaultBallsMachine();
	}

	private GameObject RandomInstantiate(float deltaZ, Transform ballSpawnPoint)
    {
        GameObject ball;
        int ballIndex = Random.Range(0, ballPrefabs.Count);
        ball = Instantiate(ballPrefabs[ballIndex], 
						   new Vector3(ballSpawnPoint.position.x, 
									   ballSpawnPoint.position.y, 
									   ballSpawnPoint.position.z + deltaZ), 
						   ballSpawnPoint.rotation);
        return ball;
    }
    public void GenerateDefaultBallsMachine()
    {
		if (curLeftBallNumber < maxLeftBallNumber && (curLeftBallNumber == 0 || timeSpent >= generateTimeInterval))
        {
			GameObject ball = RandomInstantiate(leftGenerateOffset, ballSpawnPointMachine);
			ball.tag = "DefaultBall";
			ball.transform.parent = transform;
			ball.GetComponent<Rigidbody>().velocity = Vector3.left * initialSpeed;
			++curLeftBallNumber;
		}
		if (timeSpent < generateTimeInterval)
		{
			timeSpent += Time.fixedDeltaTime;
		}
		else
		{
			timeSpent = 0;
		}
	}

	public void GenerateCustomizedBallsMachine(float weightLB)
	{
		GameObject ball = RandomInstantiate(rightGenerateOffset, ballSpawnPointMachine);
		ball.tag = "CustomizedBall";
		ball.transform.parent = transform;
		ball.GetComponent<Rigidbody>().mass = weightLB * LB_TO_KG;
		ball.GetComponent<Rigidbody>().velocity = Vector3.left * initialSpeed;

	}

	public void GenerateDefaultBallLane()
	{
		GameObject ball = RandomInstantiate(0, ballSpawnPointLane);
		PickBall(ball);
		ball.transform.parent = transform;
		ball.GetComponent<Rigidbody>().useGravity = false;
	}

	public void GenerateCustomizedBallLane(float weightLB)
	{
		GameObject ball = RandomInstantiate(0, ballSpawnPointLane);
		PickBall(ball);
		ball.transform.parent = transform;
		ball.GetComponent<Rigidbody>().mass = weightLB * LB_TO_KG;
		ball.GetComponent<Rigidbody>().useGravity = false;
	}

	public void PickBall(GameObject ball)
	{
		ball.tag = "PickedBall";
	}

	public void AttachToHand(GameObject ball)
	{
		ball.transform.position = hand._ballAttach.position;
		ball.transform.rotation = hand._ballAttach.rotation;
        ball.GetComponent<Rigidbody>().isKinematic = true;
        ball.GetComponent<Rigidbody>().useGravity = false;
	}

	public void ThrowBall(GameObject ball, Vector3 throwVelocity)
	{
        ball.GetComponent<Rigidbody>().isKinematic = false;
        ball.GetComponent<Rigidbody>().useGravity = true;
        ball.GetComponent<Rigidbody>().velocity = throwVelocity;
        ball.tag = "ThrownBall";
	}

	public void ClearThrownBall()
	{
		Destroy(GameObject.FindWithTag("ThrownBall"));
	}

	public void ClearAllBall()
    {
		/*// Destroy gameobjects in the scene
		for (var i = gameObject.transform.childCount - 1; i >= 0; i--)
		{
			// only destroy object tagged as ball
			if (gameObject.transform.GetChild(i).gameObject.tag == "DefaultBall" ||
				gameObject.transform.GetChild(i).gameObject.tag == "CustomizedBall" ||
				gameObject.transform.GetChild(i).gameObject.tag == "PickedBall" ||
				gameObject.transform.GetChild(i).gameObject.tag == "ThrownBall")
			{
				Destroy(gameObject.transform.GetChild(i).gameObject);
			}
		}*/
		GameObject[] pickedBalls = GameObject.FindGameObjectsWithTag("PickedBall");
        GameObject[] thrownBalls = GameObject.FindGameObjectsWithTag("ThrownBall");
		GameObject[] defaultBall = GameObject.FindGameObjectsWithTag("DefaultBall");
        GameObject[] customizedBalls = GameObject.FindGameObjectsWithTag("CustomizedBall");
		foreach (GameObject ball in pickedBalls)
		{
			Destroy(ball);
		}
        foreach (GameObject ball in thrownBalls)
        {
            Destroy(ball);
        }
        foreach (GameObject ball in defaultBall)
        {
            Destroy(ball);
        }
        foreach (GameObject ball in customizedBalls)
        {
            Destroy(ball);
        }
        curLeftBallNumber = 0;
	}
}
