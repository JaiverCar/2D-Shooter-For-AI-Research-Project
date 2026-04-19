using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Transform ourFlag;

    public int enemyScore = 0;
    public int playerScore = 0;

    private TMP_Text enemyScoreText;

    public float elapsedTime = 0f;
    public bool timerRunning = true;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update timer
        if (timerRunning)
        {
            elapsedTime += Time.deltaTime;
        }
        UpdateTimerDisplay();

        if (enemyScoreText == null)
        {
            GameObject obj = GameObject.Find("EnemyScore");
            if (obj != null)
                enemyScoreText = obj.GetComponent<TMP_Text>();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (enemyScoreText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);
            enemyScoreText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        }
    }

    public void AddTwoMinutes()
    {
        elapsedTime += 120f; 
    }

    public void stopTimer()
    {
        timerRunning = false;
    }

    public void winGame()
    {
        stopTimer();
        Debug.Log("Player wins! Final time: " + elapsedTime);
    }

    public void AddEnemyScore(int amount = 1)
    {
        enemyScore += amount;
        if (enemyScoreText != null)
            enemyScoreText.text = "" + enemyScore;
    }
}
