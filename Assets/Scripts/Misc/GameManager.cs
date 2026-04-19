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
    
    [SerializeField] private GameObject winUI; // Assign in Inspector
    [SerializeField] private TMP_Text finalTimeText; // CHANGED: Assign directly in Inspector

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
        
        // Make sure WinUI is hidden at start
        if (winUI != null)
        {
            winUI.SetActive(false);
        }
        else
        {
            Debug.LogError("WinUI is NOT assigned in GameManager Inspector!");
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

        // Testing: Press P key to auto win
        if (Input.GetKeyDown(KeyCode.P))
        {
            winGame();
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

        if (winUI == null)
        {
            Debug.LogError("WinUI is NULL!");
            return;
        }

        winUI.SetActive(true);
        Debug.Log("WinUI activated");

        if (finalTimeText == null)
        {
            Debug.LogError("FinalTime text is NOT assigned in Inspector!");
            return;
        }

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 1000f) % 1000f);
        string timeString = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        finalTimeText.text = timeString;
        Debug.Log($"Set FinalTime to: {timeString}");
    }

    public void AddEnemyScore(int amount = 1)
    {
        enemyScore += amount;
        if (enemyScoreText != null)
            enemyScoreText.text = "" + enemyScore;
    }
}
