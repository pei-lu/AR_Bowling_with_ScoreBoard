using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{
	[Range(0.02f, 0.1f)]
	[SerializeField] private float deltaBallPosition = 0.05f;
	[Range(0.1f, 1f)]
	[SerializeField] private float deltaSpeed = 0.5f;
	[Range(0.1f, 3f)] // max(arctan()) = 3.33
	[SerializeField] private float deltaDegree = 0.5f;
	[Range(5f, 10f)]
	[SerializeField] private float frameUpdateWaitTime = 6f;
	[SerializeField] private BallManager ballManager;
	[SerializeField] private ScoreManager scoreManager;

	[SerializeField] private launch launchBtn;
	[SerializeField] private moveLeft moveLeftBtn;
	[SerializeField] private moveRight moveRightBtn;
	[SerializeField] private speedDown speedDownBtn;
	[SerializeField] private speedUp speedUpBtn;
	[SerializeField] private turnLeft turnLeftBtn;
	[SerializeField] private turnRight turnRightBtn;

	private GameObject ball;
	private float launchSpeed;
	private float timeSWaited = -1;
	private const float MAX_VELOCITY = 10f;

	void Start()
	{
		InitializeLauncher();
	}

    void Update()
    {
        if (speedUpBtn.isPressed)
        {
            IncreaseSpeed();
            speedUpBtn.isPressed = false;
        }
        else if (speedDownBtn.isPressed)
        {
            DecreaseSpeed();
            speedDownBtn.isPressed = false;
        }
        else if (moveLeftBtn.isPressed)
        {
            LeftMove();
            moveLeftBtn.isPressed = false;
        }
        else if (moveRightBtn.isPressed)
        {
            RightMove();
            moveRightBtn.isPressed = false;
        }
        else if (turnLeftBtn.isPressed)
        {
            LeftRotate();
            turnLeftBtn.isPressed = false;
        }
        else if (turnRightBtn.isPressed)
        {
            RightRotate();
            turnRightBtn.isPressed = false;
        }
        else if (launchBtn.isPressed)
        {
            timeSWaited = 0;
            LaunchBall();
            launchBtn.isPressed = false;
        }
        if (timeSWaited >= 0)
        {
            timeSWaited += Time.deltaTime;
            if (timeSWaited >= frameUpdateWaitTime && scoreManager.pinManager.PinsAllStill())
            {
                scoreManager.UpdateFrame();
                if (scoreManager.curFrameNum <= 10)
                {
                    InitializeLauncher();
                }
                else
                {
                    scoreManager.pinManager.ClearPins();
                }
                ballManager.ClearThrownBall();
                timeSWaited = -1;
            }
        }
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown("up"))
    //    {
    //        IncreaseSpeed();
    //    }
    //    else if (Input.GetKeyDown("down"))
    //    {
    //        DecreaseSpeed();
    //    }
    //    else if (Input.GetKeyDown("left"))
    //    {
    //        LeftMove();
    //    }
    //    else if (Input.GetKeyDown("right"))
    //    {
    //        RightMove();
    //    }
    //    else if (Input.GetKeyDown("["))
    //    {
    //        LeftRotate();
    //    }
    //    else if (Input.GetKeyDown("]"))
    //    {
    //        RightRotate();
    //    }
    //    else if (Input.GetKeyDown("space"))
    //    {
    //        timeSWaited = 0;
    //        LaunchBall();
    //    }
    //    if (timeSWaited >= 0)
    //    {
    //        timeSWaited += Time.deltaTime;
    //        if (timeSWaited >= frameUpdateWaitTime && scoreManager.pinManager.PinsAllStill())
    //        {
    //            scoreManager.UpdateFrame();
    //            if (scoreManager.curFrameNum <= 10)
    //            {
    //                InitializeLauncher();
    //            }
    //            else
    //            {
    //                scoreManager.pinManager.ClearPins();
    //            }
    //            ballManager.ClearThrownBall();
    //            timeSWaited = -1;
    //        }
    //    }
    //}

    public void InitializeLauncher()
	{
		ballManager.GenerateDefaultBallLane();
		
		ball = GameObject.FindWithTag("PickedBall");
		launchSpeed = 7.0f;
	}

	public void IncreaseSpeed()
	{
		launchSpeed += deltaSpeed;
		Debug.Log(launchSpeed);
	}

	public void DecreaseSpeed()
	{
		launchSpeed -= deltaSpeed;
		Debug.Log(launchSpeed);
	}

	public void LeftMove()
	{
		ball.transform.Translate(0, 0, deltaBallPosition, Space.Self);
		//ball.transform.position = new Vector3(ball.transform.localPosition.x,
		//									  ball.transform.localPosition.y,
		//									  ball.transform.localPosition.z + deltaBallPosition);
	}

	public void RightMove()
	{
        ball.transform.Translate(0, 0, -deltaBallPosition, Space.Self);

        //ball.transform.position = new Vector3(ball.transform.localPosition.x,
								//			  ball.transform.localPosition.y,
								//			  ball.transform.localPosition.z - deltaBallPosition);
	}

	public void RightRotate()
	{
		ball.transform.Rotate(0, deltaDegree, 0, Space.Self);
		//ball.transform.up = Quaternion.Euler(0, deltaDegree, 0) * ball.transform.up;
		Debug.Log(ball.transform.right);
	}

	public void LeftRotate()
	{
        ball.transform.Rotate(0, -deltaDegree, 0, Space.Self);
        //ball.transform.up = Quaternion.Euler(0, -deltaDegree, 0) * ball.transform.up;
        Debug.Log(ball.transform.right);
	}

	public void LaunchBall()
	{
		ballManager.ThrowBall(ball, launchSpeed * ball.transform.right);
	}
}
