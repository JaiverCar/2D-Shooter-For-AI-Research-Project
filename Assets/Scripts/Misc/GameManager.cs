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

    private TMP_Text playerScoreText;
    private TMP_Text enemyScoreText;

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
        // Lazily find the score texts once the UICanvas has been instantiated
        if (playerScoreText == null)
        {
            GameObject obj = GameObject.Find("PlayerScore");
            if (obj != null)
                playerScoreText = obj.GetComponent<TMP_Text>();
        }

        if (enemyScoreText == null)
        {
            GameObject obj = GameObject.Find("EnemyScore");
            if (obj != null)
                enemyScoreText = obj.GetComponent<TMP_Text>();
        }
    }

    public void AddEnemyScore(int amount = 1)
    {
        enemyScore += amount;
        if (enemyScoreText != null)
            enemyScoreText.text = "" + enemyScore;
    }

    public void AddPlayerScore(int amount = 1)
    {
        playerScore += amount;
        if (playerScoreText != null)
            playerScoreText.text = "" + playerScore;
    }
}
