using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerScoreObject : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    public void ChangeTextColor(Color textColor)
    {
        scoreText.color = textColor;
    }

    public void ChangeScore(int score)
    {
        scoreText.text = score.ToString();
    }
}
