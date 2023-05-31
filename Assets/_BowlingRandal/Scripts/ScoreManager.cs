using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ScoreManager : MonoBehaviour
{
    public PinManager pinManager;
    // Arrays for texts of each frame
	[SerializeField] private Text[] firstRollResults;
	[SerializeField] private Text[] secondRollResults;
	[SerializeField] private Text[] frameScoreResults;
	[SerializeField] private Text finallRollResult; // final frame could have a third roll
	[SerializeField] private Text totalScore;

	[HideInInspector] public int curFrameNum = 1;
    private int curRollThisFrame = 1; // the roll to look at for this frame
    private int curScore = 0; // # of pins that has been knocked over

	private int[] cumulatedScores; // scores for each frame and the total
	private string[] frameScoreTypes;

	const int maxFrameNum = 10;

	void Start()
    {
		cumulatedScores = new int[maxFrameNum + 1]; // frame 1-10
		frameScoreTypes = new string[maxFrameNum]; // frame 1-9

	}

	public void UpdateFrame()
    {
		curScore = ComputeScore();

        if (curFrameNum < maxFrameNum) // Final frame should be handled differently
        {
			RecordRollScore();
			RecordFrameScore();
		}
        else if (curFrameNum == maxFrameNum) // Final frame
		{
			RecordRollScoreFinalFrame();
			RecordFrameScoreFinalFrame();
		}
		RecordTotalScore();
		UpdateFrameRollNum();
	}

	public void RecordRollScore()
	{
		if (curRollThisFrame == 1)
		{
			if (curScore == 10) // strike
			{
				secondRollResults[curFrameNum - 1].text = "X";
				curRollThisFrame += 2;

				pinManager.ResetPins();
			}
			else // spare of standard, will know after sencond roll
			{
				firstRollResults[curFrameNum - 1].text = "" + curScore;
				curRollThisFrame += 1;
			}
		}
		else if (curRollThisFrame == 2)
		{
			if (curScore == 10) // spare
			{
				secondRollResults[curFrameNum - 1].text = "/";
				curRollThisFrame += 1;

				pinManager.ResetPins();
			}
			else
			{
				secondRollResults[curFrameNum - 1].text = "" + (curScore - Int32.Parse(firstRollResults[curFrameNum - 1].text));
				curRollThisFrame += 1;

				pinManager.ResetPins();
			}
		}
	}

	public void RecordFrameScore()
	{
		Debug.Log(curRollThisFrame);
		if(curRollThisFrame == 2) // only if first roll text is a number
		{
			if (curFrameNum >= 2) //consider last frame
			{
				if (curFrameNum >= 3) //consider last last frame
				{
					if (frameScoreTypes[curFrameNum - 3] == "strike" && frameScoreTypes[curFrameNum - 2] == "strike")
					{
						cumulatedScores[curFrameNum - 3] += curScore;
						frameScoreResults[curFrameNum - 3].text = "" + cumulatedScores[curFrameNum - 3];
						cumulatedScores[curFrameNum - 2] += curScore;
					}
				}
				if (frameScoreTypes[curFrameNum - 2] == "spare") // last frame was a spare
				{
					cumulatedScores[curFrameNum - 2] += curScore;
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
				}
			}
		}
		else if(curRollThisFrame == 3)
		{
			// for each frame, it could influence previous two frames (if exist)
			frameScoreTypes[curFrameNum - 1] = GetFrameScoreType(firstRollResults[curFrameNum - 1].text, secondRollResults[curFrameNum - 1].text);
			int curFrameScore = GetScoreFromTwoRollRecords(firstRollResults[curFrameNum - 1].text, secondRollResults[curFrameNum - 1].text);

			if (curFrameNum == 1)
			{
				cumulatedScores[curFrameNum - 1] = curFrameScore;

				if (frameScoreTypes[curFrameNum - 1] == "standard") // record frame score only if it's a standard
				{
					frameScoreResults[curFrameNum - 1].text = "" + cumulatedScores[curFrameNum - 1];
				}
			}
			else if (curFrameNum == 2) // need to consider last frame
			{
				if (frameScoreTypes[curFrameNum - 2] == "strike") // last frame was a strike
				{
					cumulatedScores[curFrameNum - 2] += curFrameScore;
					cumulatedScores[curFrameNum - 1] = cumulatedScores[curFrameNum - 2] + curFrameScore;

					if (frameScoreTypes[curFrameNum - 1] != "strike")
					{
						frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
					}
					if (frameScoreTypes[curFrameNum - 1] == "standard")
					{
						frameScoreResults[curFrameNum - 1].text = "" + cumulatedScores[curFrameNum - 1];
					}
				}
				else if (frameScoreTypes[curFrameNum - 2] == "spare") // last frame was a spare
				{
					if (frameScoreTypes[curFrameNum - 1] == "strike")
					{
						cumulatedScores[curFrameNum - 2] += 10;
					}
					cumulatedScores[curFrameNum - 1] = cumulatedScores[curFrameNum - 2] + curFrameScore;
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];

					if (frameScoreTypes[curFrameNum - 1] == "standard")
					{
						frameScoreResults[curFrameNum - 1].text = "" + cumulatedScores[curFrameNum - 1];
					}
				}
				else if (frameScoreTypes[curFrameNum - 2] == "standard") // last frame was a standard
				{
					cumulatedScores[curFrameNum - 1] = cumulatedScores[curFrameNum - 2] + curFrameScore;
					if (frameScoreTypes[curFrameNum - 1] == "standard")
					{
						frameScoreResults[curFrameNum - 1].text = "" + cumulatedScores[curFrameNum - 1];
					}
				}
			}
			else if (curFrameNum >= 3) // need to consider last last frame and last frame
			{
				if (frameScoreTypes[curFrameNum - 3] == "strike" && frameScoreTypes[curFrameNum - 2] == "strike")
				{
					if (frameScoreTypes[curFrameNum - 1] == "strike")
					{
						// update & record curFrameNum - 3
						cumulatedScores[curFrameNum - 3] += 10;
						frameScoreResults[curFrameNum - 3].text = "" + cumulatedScores[curFrameNum - 3];
						cumulatedScores[curFrameNum - 2] += 10;
					}
					// update & record curFrameNum - 2
					cumulatedScores[curFrameNum - 2] += curFrameScore;
					if (frameScoreTypes[curFrameNum - 1] != "strike")
					{
						frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
					}
					// update curFrameNum - 1
					cumulatedScores[curFrameNum - 1] = cumulatedScores[curFrameNum - 2] + curFrameScore;
				}
				else if (frameScoreTypes[curFrameNum - 2] == "strike")
				{
					cumulatedScores[curFrameNum - 2] += curFrameScore;
					if (frameScoreTypes[curFrameNum - 1] != "strike")
					{
						frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
					}
				}
				else if (frameScoreTypes[curFrameNum - 2] == "spare")
				{
					if (frameScoreTypes[curFrameNum - 1] == "strike")
					{
						cumulatedScores[curFrameNum - 2] += curFrameScore;
						frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
					}
				}
				cumulatedScores[curFrameNum - 1] = cumulatedScores[curFrameNum - 2] + curFrameScore;

				if (frameScoreTypes[curFrameNum - 1] == "standard")
				{
					frameScoreResults[curFrameNum - 1].text = "" + cumulatedScores[curFrameNum - 1];
				}
			}
		}
	}

	public void RecordRollScoreFinalFrame()
	{
		bool hasThirdRoll = false;
		if (curRollThisFrame == 1) 
		{
			if (curScore == 10) // Strike
			{
				firstRollResults[curFrameNum - 1].text = "X";
				++curRollThisFrame;
				pinManager.ResetPins();
			}
			else // others
			{
				firstRollResults[curFrameNum - 1].text = "" + curScore;
				++curRollThisFrame;
			}
			
		}
		else if (curRollThisFrame == 2)
		{
			if (firstRollResults[curFrameNum - 1].text == "X" && curScore == 10) // strike again
			{
				secondRollResults[curFrameNum - 1].text = "X";
				pinManager.ResetPins();
			}
			else if (firstRollResults[curFrameNum - 1].text != "X" && curScore == 10) // spare
			{
				secondRollResults[curFrameNum - 1].text = "/";
				pinManager.ResetPins();
			}
			else if (curScore != 10)// standard
			{
				secondRollResults[curFrameNum - 1].text = "" + (curScore - Int32.Parse(firstRollResults[curFrameNum - 1].text));

			}
			++curRollThisFrame;
		}
		// The player is allowed to try a third roll in the final frame only if
		// the first two rolls are not all numbers
		else if (curRollThisFrame == 3)
		{
			hasThirdRoll = !(int.TryParse(firstRollResults[curFrameNum - 1].text, out _) && int.TryParse(secondRollResults[curFrameNum - 1].text, out _));
			if (hasThirdRoll)
			{
				if (!int.TryParse(secondRollResults[curFrameNum - 1].text, out _) && curScore == 10) // strike
				{
					finallRollResult.text = "X";
				}
				else if (int.TryParse(secondRollResults[curFrameNum - 1].text, out _) && curScore == 10) // spare
				{
					finallRollResult.text = "/";
				}
				else if (!int.TryParse(secondRollResults[curFrameNum - 1].text, out _) && curScore != 10)
				{
					finallRollResult.text = "" + curScore;
				}
				else if (int.TryParse(secondRollResults[curFrameNum - 1].text, out _) && curScore != 10)
				{
					finallRollResult.text = "" + (curScore - Int32.Parse(secondRollResults[curFrameNum - 1].text));
				}
				++curRollThisFrame;
			}
		}
	}

	public void RecordFrameScoreFinalFrame()
	{
		// The final frame has 7 cases in total
		// X X X
		// X X 3
		// X 3 /
		// X 3 6
		// or
		// 3 / X
		// 3 / 6
		// 3 6
		if (curRollThisFrame == 2) // have thrown a ball at the first roll: X ? ? or 3 ? ?
		{
			if (firstRollResults[curFrameNum - 1].text == "X")
			{
				if (frameScoreTypes[curFrameNum - 3] == "strike" && frameScoreResults[curFrameNum - 3].text == "")
				{
					cumulatedScores[curFrameNum - 3] += 10;
					frameScoreResults[curFrameNum - 3].text = "" + cumulatedScores[curFrameNum - 3];
					cumulatedScores[curFrameNum - 2] += 10; // change with curFrameNum - 3
				}
				if (frameScoreTypes[curFrameNum - 2] == "spare")
				{
					cumulatedScores[curFrameNum - 2] += 10;
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
				}
			}
			else if (int.TryParse(firstRollResults[curFrameNum - 1].text, out _))
			{
				int firstRollNum = Int32.Parse(firstRollResults[curFrameNum - 1].text);
				if (frameScoreTypes[curFrameNum - 3] == "strike")
				{
					cumulatedScores[curFrameNum - 3] += firstRollNum;
					frameScoreResults[curFrameNum - 3].text = "" + cumulatedScores[curFrameNum - 3];
					cumulatedScores[curFrameNum - 2] += firstRollNum; // change with curFrameNum - 3
				}
				if (frameScoreTypes[curFrameNum - 2] == "spare")
				{
					cumulatedScores[curFrameNum - 2] += firstRollNum;
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
				}
			}
		}
		else if (curRollThisFrame == 3)
		{
			if (firstRollResults[curFrameNum - 1].text == "X" && secondRollResults[curFrameNum - 1].text == "X")
			{
				if (frameScoreTypes[curFrameNum - 2] == "strike")
				{
					cumulatedScores[curFrameNum - 2] += 20;
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
				}
			}
			else if (firstRollResults[curFrameNum - 1].text == "X" && int.TryParse(secondRollResults[curFrameNum - 1].text, out _))
			{
				if (frameScoreTypes[curFrameNum - 2] == "strike")
				{
					cumulatedScores[curFrameNum - 2] += 10 + Int32.Parse(secondRollResults[curFrameNum - 1].text);
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
				}
			}
			else if (int.TryParse(firstRollResults[curFrameNum - 1].text, out _) && secondRollResults[curFrameNum - 1].text == "/")
			{
				if (frameScoreTypes[curFrameNum - 2] == "strike")
				{
					cumulatedScores[curFrameNum - 2] += 10;
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
				}
			}
			else if (int.TryParse(firstRollResults[curFrameNum - 1].text, out _) && int.TryParse(secondRollResults[curFrameNum - 1].text, out _))
			{
				if (frameScoreTypes[curFrameNum - 2] == "strike")
				{
					cumulatedScores[curFrameNum - 2] += Int32.Parse(firstRollResults[curFrameNum - 1].text) + Int32.Parse(secondRollResults[curFrameNum - 1].text);
					frameScoreResults[curFrameNum - 2].text = "" + cumulatedScores[curFrameNum - 2];
				}
				cumulatedScores[curFrameNum - 1] = cumulatedScores[curFrameNum - 2] +
												   Int32.Parse(firstRollResults[curFrameNum - 1].text) +
												   Int32.Parse(secondRollResults[curFrameNum - 1].text);
				frameScoreResults[curFrameNum - 1].text = "" + cumulatedScores[curFrameNum - 1];
			}
		}
		else if (curRollThisFrame == 4)
		{
			int bonus = 0;
			if (firstRollResults[curFrameNum - 1].text == "X" &&
				secondRollResults[curFrameNum - 1].text == "X" &&
				finallRollResult.text == "X")
			{
				bonus = 30;
			}
			else if (firstRollResults[curFrameNum - 1].text == "X" &&
					 secondRollResults[curFrameNum - 1].text == "X" &&
					 int.TryParse(finallRollResult.text, out _))
			{
				bonus = 20 + Int32.Parse(finallRollResult.text);
			}
			else if (firstRollResults[curFrameNum - 1].text == "X" &&
					 int.TryParse(secondRollResults[curFrameNum - 1].text, out _) &&
					 finallRollResult.text == "/")
			{
				bonus = 20;
			}
			else if (firstRollResults[curFrameNum - 1].text == "X" &&
					 int.TryParse(secondRollResults[curFrameNum - 1].text, out _) &&
					 int.TryParse(finallRollResult.text, out _))
			{
				bonus = 10 + Int32.Parse(secondRollResults[curFrameNum - 1].text) + Int32.Parse(finallRollResult.text);
			}
			else if (int.TryParse(firstRollResults[curFrameNum - 1].text, out _) &&
					 secondRollResults[curFrameNum - 1].text == "/" &&
					 finallRollResult.text == "X")
			{
				bonus = 20;
			}
			else if (int.TryParse(firstRollResults[curFrameNum - 1].text, out _) &&
					 secondRollResults[curFrameNum - 1].text == "/" &&
					 int.TryParse(finallRollResult.text, out _))
			{
				bonus = 10 + Int32.Parse(finallRollResult.text);
			}
			cumulatedScores[curFrameNum - 1] = cumulatedScores[curFrameNum - 2] + bonus;
			frameScoreResults[curFrameNum - 1].text = "" + cumulatedScores[curFrameNum - 1];
		}
	}

	public void RecordTotalScore()
	{
		if (frameScoreResults[curFrameNum - 1].text != "")
		{
			totalScore.text = frameScoreResults[curFrameNum - 1].text;
		}
		else if (curFrameNum >= 2 && frameScoreResults[curFrameNum - 2].text != "")
		{
			totalScore.text = frameScoreResults[curFrameNum - 2].text;
		}
		else if (curFrameNum >= 3 && frameScoreResults[curFrameNum - 3].text != "")
		{
			totalScore.text = frameScoreResults[curFrameNum - 3].text;
		}
	}

	string GetFrameScoreType(string firstRollRecord, string secondRollRecord)
	{
		if (int.TryParse(firstRollRecord, out _) && int.TryParse(secondRollRecord, out _))
		{
			return "standard";
		}
		else if (secondRollRecord == "/")
		{
			return "spare";
		}
		else if (secondRollRecord == "X")
		{
			return "strike";
		}
		else
		{
			return "Unknown frame score type";
		}
	}

	int GetScoreFromTwoRollRecords(string firstRollRecord, string secondRollRecord)
	{
		int firstRollScore, secondRollScore;
		if (int.TryParse(firstRollRecord, out firstRollScore) && int.TryParse(secondRollRecord, out secondRollScore))
		{
			return firstRollScore + secondRollScore;
		}
		else if (secondRollRecord == "/" || secondRollRecord == "X")
		{
			return 10;
		}
		else
		{
			return 0;
		}
	}

	int ComputeScore() //Count Fallen Pins
	{
        int rollScore = 0;
		foreach (Transform child in pinManager.transform)
		{
			if (child.gameObject.tag == "Pin" && (child.position.y > 0.04 || child.position.y < 0))
            {
                ++rollScore;
            }
        }
		return rollScore;
	}

    void UpdateFrameRollNum()
	{
		// frame 1-9
		if (curFrameNum < maxFrameNum && curRollThisFrame >= 3)
		{
			++curFrameNum;
			curRollThisFrame = 1;
		}
		// frame 10
		else if (curFrameNum == maxFrameNum && curRollThisFrame >= 4)
		{
			++curFrameNum;
			curRollThisFrame = 1;
		}
	}

}
